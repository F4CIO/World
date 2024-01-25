using System.Net.Mail;
using InfoCompass.World.DataAccessContracts;
using Errors = InfoCompass.World.Common.Entities.Errors;

namespace InfoCompass.World.BusinessLogic;

public interface IServiceForUsers
{
	Task<User> GetByEMail(string email);
	Task<ServerResponse<User>> StartRegistrationProcess(RegisterRequest request);
	Task<ServerResponse<User>> FinishRegistrationProcess(string token);
	Task<ServerResponse<User>> Register(long userId);
	Task<ServerResponse<User>> ValidateLoginCredentials(LoginRequest loginRequest);
	Task<ServerResponse<User>> UpdateMomentOfLastLogin(long userId, DateTime momentOfLastLogin);
	Task<ServerResponse<bool>> Logout();
	Task<ServerResponse<string>> CreatePasswordResetRequest(string eMail);
	Task<ServerResponse<bool>> VerifyPasswordResetToken(string token);
	Task<ServerResponse<User>> ChangePasswordUsingToken(ChangePasswordUsingTokenRequest request);
	Task<ServerResponse<User>> ChangePassword(ChangePasswordRequest request);
	Task<ServerResponse<User>> EditDetails(User request);
	Task<ServerResponse<string>> PostUserMessage(UserMessage userMessage);
}

public sealed class ServiceForUsers:InfoCompass.World.BusinessLogic.ServiceBase, IServiceForUsers
{
	InfoCompass.World.DataAccessContracts.IServiceForPasswordResetRequests _serviceForPasswordResetRequests;
	InfoCompass.World.BusinessLogic.IServiceForSettings _serviceForSettings;
	InfoCompass.World.DataAccessContracts.IServiceForRegistrationConfirmations _serviceForRegistrationConfirmations;
	InfoCompass.World.DataAccessContracts.IServiceForUserMessages _serviceForUserMessages;

	public ServiceForUsers(ServiceForCOE c, IServiceForLogs serviceForLogs, InfoCompass.World.DataAccessContracts.IServiceForUsers serviceForUsers, InfoCompass.World.DataAccessContracts.IServiceForPasswordResetRequests serviceForPasswordResetRequests, IServiceForSettings serviceForSettings, IServiceForRegistrationConfirmations serviceForRegistrationConfirmations, IServiceForUserMessages serviceForUserMessages)
		: base(c, serviceForLogs, serviceForUsers)
	{
		_serviceForPasswordResetRequests = serviceForPasswordResetRequests;
		_serviceForSettings = serviceForSettings;//if user is logged in preferred use is _c.Settings. This one can be used if no users are logged in
		_serviceForRegistrationConfirmations = serviceForRegistrationConfirmations;
		_serviceForUserMessages = serviceForUserMessages;
	}

	public async Task<User> GetByEMail(string email)
	{
		User r = null;

		try
		{
			r = await _serviceForUsers.GetByEMail(email);
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing GetByEMail({email})");
		}

		return r;
	}

	public async Task<ServerResponse<User>> StartRegistrationProcess(RegisterRequest request)
	{
		ServerResponse<User> r = null;
		var errors = new Errors();

		try
		{
			request.Trim();
			request.Validate(_c, errors);

			if(errors.Count > 0)
			{
				r = new ServerResponse<User>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{
				_c.Log($"Request={Json.Serialize<RegisterRequest>(request)}...");

				User user = await _serviceForUsers.GetByEMail(request.EMail);
				if(user != null && user.IsRegistered)
				{
					r = new ServerResponse<User>(false, "User with same email is already registered. If you are that user please use Login page.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
				}
				else
				{
					if(user != null && !user.IsActive)
					{
						r = new ServerResponse<User>(false, "User with this email has been deactivated in our records. If you believe this is an error contact us.", null, null, errors, null, System.Net.HttpStatusCode.Unauthorized);
					}
					else
					{
						using(_c.LogBeginScope($"Initiation registration process for new user:{request?.EMail})..."))
						{
							DateTime now = DateTime.Now;

							if(user == null)
							{
								_c.Log("Creating user record in db...");
								user = new User()
								{
									EMail = request.EMail,
									PasswordHash = null,
									MomentOfCreation = now,
									MomentOfLastUpdate = now,
									MomentOfLastLogin = null,
									IsRegistered = false,  //needs to click on confirmation link first
									IsAdministrator = false,
									IsActive = true,
									FirstName = request.FirstName,
									LastName = request.LastName,
									MomentOfLastVisit = now,
									Subscribed = request.SubscribeMe == true,
									PaidPlanId = null,
									Note = null
								};
								user = await _serviceForUsers.Insert(user);
							}
							else
							{
								await _serviceForUsers.Update(user);
							}

							_c.Log("Creating RegistrationConfirmation in db...");
							var registrationConfirmation = new RegistrationConfirmation()
							{
								UserId = user.Id,
								MomentOfCreation = now,
								MomentOfLastUpdate = now,
								RawData = Guid.NewGuid().ToString().Replace("-", ""),
								MomentOfExpirationAsUtc = DateTime.UtcNow.AddDays(1),
								IsActive = true,
								ReasonForDeactivation = null,

								PendingFirstName = request.FirstName,
								PendingLastName = request.LastName,
								PendingEMail = request.EMail,
								PendingSubscribeMe = request.SubscribeMe,
								PendingPasswordHash = InfoCompass.World.Common.Helper.CreateHashFromPassword(request.NewPassword)
							};
							registrationConfirmation = await _serviceForRegistrationConfirmations.Insert(registrationConfirmation);

							_c.Log("Preparing email...");
							var mailMessage = new MailMessage(new MailAddress(_c.Settings.MailServerFromEMail, _c.Settings.MailServerFromName), new MailAddress(user.EMail));
							mailMessage.IsBodyHtml = true;
							string link = CraftSynth.BuildingBlocks.UI.Web.UriHandler.UriCombine(_serviceForSettings.GetCachedOrFromDbForUserDefault().Result.GuiUrl, "auth", "register-confirm") + "?token=" + registrationConfirmation.RawData;
							mailMessage.Subject = $"Confirm your registration at {_c.Settings.InstanceName}!";
							mailMessage.Body = $"To confirm your registration at {_c.Settings.InstanceName} click this link below: <br/><br/><a href=\"{link}\">CONFIRM</a><br/><br/>Alternatevly, if clicking on a link does not work for any reason, you can copy paste this to your browser url field: {link} <br/><br/>Note that above link will expire in one day.<br/><br/>If you did not initiated this registration you can report case by replying to this email.";

							_c.Log($"Sending email to {user.EMail}...");
							CraftSynth.BuildingBlocks.IO.EMail.SendMail(_c.Settings.MailServerHostName,
																		_c.Settings.MailServerPort,
																		_c.Settings.MailServerUsername,
																		_c.Settings.MailServerPassword,
																		mailMessage,
																		new CustomTraceLog(),
																		true,
																		true,
																		null,
																		SmtpDeliveryMethod.Network);
							_c.Log($"Mail sent successfully to {user.EMail}.");

							long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Registration_Confirmation_Link_Sent, _c.CurrentlyLoggedInUser, $"Registration confirmation link sent to {user.EMail} with token={registrationConfirmation.RawData.Bubble(10, "...")}", LogExtraColumnNames.UserId, user.Id);
							_c.Log($"Registration confirmation link sent to {user.EMail} with token={registrationConfirmation.RawData.Bubble(10, "...")}. LogId={logId}");
							user.PasswordHash = "(removed for safety)";
							r = new ServerResponse<User>(true, "Check your inbox and confirm registration!", null, user, errors, null, System.Net.HttpStatusCode.OK);
							_c.Log("All done!");
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing StartRegistrationProcess({request.EMail},...)");
		}

		return r;
	}

	public async Task<ServerResponse<User>> FinishRegistrationProcess(string token)
	{
		ServerResponse<User> r = null;
		var errors = new Errors();

		try
		{
			token = token.ToNonNullString().Trim();

			if(errors.Count > 0)
			{
				r = new ServerResponse<User>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{
				using(_c.LogBeginScope($"Finishing registration process using token:{token})..."))
				{
					//_c.Log($"Request={Json.Serialize<ChangePasswordUsingTokenRequest>(request)}...");

					_c.Log("Verifying token...");
					//string token = InfoCompass.World.Common.Helper.CreateHashFromPassword(request?.Token ?? "");
					RegistrationConfirmation? registrationConfirmationFromDb = (await _serviceForRegistrationConfirmations.Get<RegistrationConfirmation>(prr => prr.RawData == token)).SingleOrDefault();
					if(registrationConfirmationFromDb == null)
					{
						r = new ServerResponse<User>(false, "Something went wrong. Token was not recognized. Please go to Register page and start the process again.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
					}
					else if(registrationConfirmationFromDb.MomentOfExpirationAsUtc.Ticks < DateTime.UtcNow.Ticks)
					{
						r = new ServerResponse<User>(false, "Your registration request has expired. Please go Register page and start the process again.", null, null, errors, null, System.Net.HttpStatusCode.Unauthorized);
					}
					else if(!registrationConfirmationFromDb.IsActive)
					{
						r = new ServerResponse<User>(false, "You already confirmed your registration. Please use Login page.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
					}
					else
					{
						User user = await _serviceForUsers.GetById<User>(registrationConfirmationFromDb.UserId);
						if(user == null)
						{
							r = new ServerResponse<User>(false, "User from registration request was not found.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
						}
						else
						{
							if(!user.IsActive)
							{
								r = new ServerResponse<User>(false, "User from registration request is deactivated in our records. If you believe this is an error contact us.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
							}
							else
							{
								if(user.IsRegistered)
								{
									r = new ServerResponse<User>(false, "You are already registered. Please use login page.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
								}
								else
								{
									_c.Log("Updating user record in db...");
									//user = await _serviceForUsers.GetById<User>(user.Id);
									user.FirstName = registrationConfirmationFromDb.PendingFirstName;
									user.LastName = registrationConfirmationFromDb.PendingLastName;
									//user.EMail = registrationConfirmationFromDb.PendingEMail;	
									user.Subscribed = registrationConfirmationFromDb.PendingSubscribeMe == true;
									user.PasswordHash = registrationConfirmationFromDb.PendingPasswordHash;
									user.IsRegistered = true;
									user.MomentOfLastUpdate = DateTime.Now;
									await _serviceForUsers.Update(user);

									_c.Log("Updating RegistrationConfirmation in db...");
									//registrationConfirmationFromDb = (await _serviceForPasswordResetRequests.Get<PasswordResetRequest>(prr => prr.Token == token)).SingleOrDefault();
									registrationConfirmationFromDb.IsActive = false;
									registrationConfirmationFromDb.ReasonForDeactivation = PasswordResetRequestReasonForDeactivation.UsedUp.ToString();
									registrationConfirmationFromDb.MomentOfLastUpdate = DateTime.Now;
									await _serviceForRegistrationConfirmations.Update(registrationConfirmationFromDb);

									_c.Log("Preparing email...");
									var mailMessage = new MailMessage(new MailAddress(_c.Settings.MailServerFromEMail, _c.Settings.MailServerFromName), new MailAddress(user.EMail));
									mailMessage.IsBodyHtml = true;
									string link = _serviceForSettings.GetCachedOrFromDbForUserDefault().Result.GuiUrl;
									mailMessage.Subject = $"Welcome to {_c.Settings.InstanceName}!";
									mailMessage.Body = $"Congratulations, your account at {_c.Settings.InstanceName} has just been created.<br/><br/>Now go <a href=\"{link}\">{_c.Settings.InstanceName}</a> and start generating your books.<br/><br/>If by any chance above link does not work you can copy paste this into your browser: {link}<br/><br/>If this is unexpected please report case by replying to this email.";
									_c.Log($"Sending email to {user.EMail}...");
									CraftSynth.BuildingBlocks.IO.EMail.SendMail(_c.Settings.MailServerHostName,
																				_c.Settings.MailServerPort,
																				_c.Settings.MailServerUsername,
																				_c.Settings.MailServerPassword,
																				mailMessage,
																				new CustomTraceLog(),
																				true,
																				true,
																				null,
																				SmtpDeliveryMethod.Network);
									_c.Log($"Mail sent successfully to {user.EMail}.");

									long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Registered, _c.CurrentlyLoggedInUser, $"User registered after receiving token={token}. userId={user.Id}. EMail={user.EMail}", LogExtraColumnNames.UserId, user.Id);
									_c.Log($"User registered after receiving token={token}. userId={user.Id}. EMail={user.EMail}. LogId={logId}");
									user.PasswordHash = "(removed for safety)";
									r = new ServerResponse<User>(true, "Welcome!", null, user, errors, null, System.Net.HttpStatusCode.OK);
									_c.Log("All done!");
								}
							}
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing FinishRegistrationProcess({token}");
		}

		return r;
	}

	public async Task<ServerResponse<User>> Register(long userId)
	{
		ServerResponse<User> r = null;
		var errors = new Errors();

		try
		{
			if(errors.Count > 0)
			{
				r = new ServerResponse<User>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{
				using(_c.LogBeginScope($"Registering user with id:{userId})..."))
				{
					User user = await _serviceForUsers.GetById<User>(userId);
					if(user == null)
					{
						r = new ServerResponse<User>(false, "User from registration request was not found.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
					}
					else
					{
						if(!user.IsActive)
						{
							r = new ServerResponse<User>(false, "User from registration request is deactivated in our records. If you believe this is an error contact us.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
						}
						else
						{
							if(user.IsRegistered)
							{
								r = new ServerResponse<User>(false, "You are already registered. Please use login page.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
							}
							else
							{
								_c.Log("Updating user record in db...");
								user.IsRegistered = true;
								user.MomentOfLastUpdate = DateTime.Now;
								await _serviceForUsers.Update(user);

								_c.Log("Preparing email...");
								var mailMessage = new MailMessage(new MailAddress(_c.Settings.MailServerFromEMail, _c.Settings.MailServerFromName), new MailAddress(user.EMail));
								mailMessage.IsBodyHtml = true;
								string link = _serviceForSettings.GetCachedOrFromDbForUserDefault().Result.GuiUrl;
								mailMessage.Subject = $"Welcome to {_c.Settings.InstanceName}!";
								mailMessage.Body = $"Congratulations, your account at {_c.Settings.InstanceName} has just been created.<br/><br/>Now go <a href=\"{link}\">{_c.Settings.InstanceName}</a> and start generating your books.<br/><br/>If by any chance above link does not work you can copy paste this into your browser: {link}<br/><br/>If this is unexpected please report case by replying to this email.";
								_c.Log($"Sending email to {user.EMail}...");
								CraftSynth.BuildingBlocks.IO.EMail.SendMail(_c.Settings.MailServerHostName,
																			_c.Settings.MailServerPort,
																			_c.Settings.MailServerUsername,
																			_c.Settings.MailServerPassword,
																			mailMessage,
																			new CustomTraceLog(),
																			true,
																			true,
																			null,
																			SmtpDeliveryMethod.Network);
								_c.Log($"Mail sent successfully to {user.EMail}.");

								long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Registered, _c.CurrentlyLoggedInUser, $"User registered. userId={user.Id}. EMail={user.EMail}", LogExtraColumnNames.UserId, user.Id);
								_c.Log($"User registered. userId={user.Id}. EMail={user.EMail}. LogId={logId}");
								user.PasswordHash = "(removed for safety)";
								r = new ServerResponse<User>(true, "Welcome!", null, user, errors, null, System.Net.HttpStatusCode.OK);
								_c.Log("All done!");
							}
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing Register({userId}");
		}

		return r;
	}

	public async Task<ServerResponse<User>> ValidateLoginCredentials(LoginRequest request)
	{
		ServerResponse<User> r = null;
		var errors = new Errors();

		try
		{
			request.Trim();
			request.Validate(_c, errors);


			if(errors.Count > 0)
			{
				r = new ServerResponse<User>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{
				r = await this.Login_Inner(request);
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing Login. GenerateInitialRequest={CraftSynth.BuildingBlocks.IO.Json.Serialize(request)}");
		}

		return r;
	}

	private async Task<ServerResponse<User>> Login_Inner(LoginRequest request)
	{
		ServerResponse<User> r = null;

		User userFromDb = await _serviceForUsers.GetByEMail(request.Username);
		if(userFromDb == null)
		{
			r = new ServerResponse<User>(false, "Bad email or password", null, null, null, null, System.Net.HttpStatusCode.Forbidden);
		}
		else
		{
			string requestPasswordHash = InfoCompass.World.Common.Helper.CreateHashFromPassword(request.Password);
			if(userFromDb.PasswordHash != requestPasswordHash)
			{
				r = new ServerResponse<User>(false, "Bad email or password", null, null, null, null, System.Net.HttpStatusCode.Forbidden);
			}
			else
			{
				long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Logged_In, userFromDb.Id, $"User credentials valid. user.Id={userFromDb.Id}, user.EMail={userFromDb.EMail}");
				userFromDb.PasswordHash = "(removed for safety)";
				r = new ServerResponse<User>(true, "Ok", logId, userFromDb, null, null, System.Net.HttpStatusCode.OK);
			}
		}

		return r;
	}

	public async Task<ServerResponse<User>> UpdateMomentOfLastLogin(long userId, DateTime momentOfLastLogin)
	{
		ServerResponse<User> r = null;

		try
		{
			User user = await _serviceForUsers.GetById<User>(userId);
			user.MomentOfLastLogin = momentOfLastLogin;
			await _serviceForUsers.Update(user);
			r = new ServerResponse<User>(true, "Ok", null, user, null, null, System.Net.HttpStatusCode.OK);
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing UpdateMomentOfLastLogin({momentOfLastLogin.ToDDateAndTimeAs_MM_DD_YYYY__HH_MM_SS()}). ");
		}

		return r;
	}

	public async Task<ServerResponse<bool>> Logout()
	{
		throw new NotImplementedException();
	}

	public async Task<ServerResponse<string>> CreatePasswordResetRequest(string eMail)
	{
		ServerResponse<string> r = null;
		var errors = new Errors();

		try
		{
			eMail = (eMail ?? "").Trim();
			if(!CraftSynth.BuildingBlocks.Validation.IsEMail(eMail))
			{
				errors.Add("EMail", "Please enter valid email address");
			}

			if(errors.Count > 0)
			{
				r = new ServerResponse<string>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{
				User user = await _serviceForUsers.GetByEMail(eMail);
				if(user == null)
				{
					r = new ServerResponse<string>(false, "No user with this email was found", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
				}
				else
				{
					using(_c.LogBeginScope($"Processing password reset request for email {eMail}..."))
					{
						string token = Guid.NewGuid().ToString().Replace("-", "").ToUpper();

						_c.Log("Inserting new password reset request...");
						var passwordResetRequest = new PasswordResetRequest()
						{
							UserId = user.Id,
							MomentOfCreation = DateTime.Now,
							MomentOfLastUpdate = DateTime.Now,
							MomentOfExpirationAsUtc = DateTime.Now.AddDays(1),
							Token = InfoCompass.World.Common.Helper.CreateHashFromPassword(token),
							IsActive = true,
							ReasonForDeactivation = null
						};
						passwordResetRequest = await _serviceForPasswordResetRequests.Insert<PasswordResetRequest>(passwordResetRequest);

						_c.Log("Preparing email...");
						string link = CraftSynth.BuildingBlocks.UI.Web.UriHandler.UriCombine(_serviceForSettings.GetCachedOrFromDbForUserDefault().Result.GuiUrl, "auth", "password-reset") + "?token=" + token;

						var mailMessage = new MailMessage(new MailAddress(_c.Settings.MailServerFromEMail, _c.Settings.MailServerFromName), new MailAddress(user.EMail));
						mailMessage.IsBodyHtml = true;
						mailMessage.Subject = "Reset password";
						mailMessage.Body = $"To reset your password at {_c.Settings.InstanceName} click this link below: <br/><br/><a href=\"{link}\">RESET PASSWORD</a><br/><br/>Alternatevly, if clicking on a link does not work for any reason, you can copy paste this to your browsers url field: {link} <br/><br/>Note that above link will expire in one day.<br/><br/>If you did not ask to reset your password you can report case by replying to this email.";

						_c.Log($"Sending email to {user.EMail}...");
						CraftSynth.BuildingBlocks.IO.EMail.SendMail(_c.Settings.MailServerHostName,
																	_c.Settings.MailServerPort,
																	_c.Settings.MailServerUsername,
																	_c.Settings.MailServerPassword,
																	mailMessage,
																	new CustomTraceLog(),
																	true,
																	true,
																	null,
																	SmtpDeliveryMethod.Network);
						_c.Log($"Mail sent successfully to {user.EMail}.");
						long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__Password_Reset_Request_Created, _c.CurrentlyLoggedInUser, $"Password reset request created for email: {eMail} and userId={user.Id}", LogExtraColumnNames.UserId, user.Id);
						_c.Log($"Password reset request created for email: {eMail} and userId={user.Id}. LogId={logId}");
						r = new ServerResponse<string>(true, "Email has been sent to you with instructions for reseting the password.", null, null, errors, null, System.Net.HttpStatusCode.OK);
						_c.Log("All done!");
					}
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing CreatePasswordResetRequest({eMail}");
		}

		return r;
	}

	public async Task<ServerResponse<bool>> VerifyPasswordResetToken(string token)
	{
		ServerResponse<bool> r = null;
		var errors = new Errors();
		bool tokenOk = false;

		try
		{
			using(_c.LogBeginScope($"Verifying token:{token ?? "null"})..."))
			{
				token = token.Trim();
				token = InfoCompass.World.Common.Helper.CreateHashFromPassword(token ?? "");
				PasswordResetRequest? passwordResetRequestFromDb = (await _serviceForPasswordResetRequests.Get<PasswordResetRequest>(prr => prr.Token == token)).SingleOrDefault();
				if(passwordResetRequestFromDb == null)
				{
					r = new ServerResponse<bool>(false, "Password request was not found. Please go to Login page > Forgot passowrd to create a new one.", null, false, errors, null, System.Net.HttpStatusCode.NotFound);
				}
				else if(passwordResetRequestFromDb.MomentOfExpirationAsUtc.Ticks < DateTime.UtcNow.Ticks)
				{
					r = new ServerResponse<bool>(false, "Your password reset request has expired. Please go to Login page > Forgot passowrd to create a new one.", null, false, errors, null, System.Net.HttpStatusCode.Unauthorized);
				}
				else if(!passwordResetRequestFromDb.IsActive)
				{
					r = new ServerResponse<bool>(false, "Your password reset request has already been used up. Please go to Login page > Forgot passowrd to create a new one.", null, false, errors, null, System.Net.HttpStatusCode.Unauthorized);
				}
				else
				{
					User user = await _serviceForUsers.GetById<User>(passwordResetRequestFromDb.UserId);
					if(user == null)
					{
						r = new ServerResponse<bool>(false, "User for password reset request was not found.", null, false, errors, null, System.Net.HttpStatusCode.NotFound);
					}
					else
					{
						tokenOk = true;
						r = new ServerResponse<bool>(true, "Ok", null, true, errors, null, System.Net.HttpStatusCode.OK);
						_c.Log("Token ok!");
					}
				}

				if(!tokenOk)
				{
					long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__Password_Reset_Request_Token_Verification_Failed, _c.CurrentlyLoggedInUser, $"Password reset request token verification failed for token {token} with message {r.Message}");
					_c.Log($"Password reset request token verification failed for token {token} with message {r.Message}. LogId={logId}");
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing VerifyToken({token}");
		}

		return r;
	}

	public async Task<ServerResponse<User>> ChangePasswordUsingToken(ChangePasswordUsingTokenRequest request)
	{
		ServerResponse<User> r = null;
		var errors = new Errors();

		try
		{
			request.Trim();
			request.Validate(_c, errors);

			if(errors.Count > 0)
			{
				r = new ServerResponse<User>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{
				using(_c.LogBeginScope($"Changing password using token:{request?.Token})..."))
				{
					_c.Log($"Request={Json.Serialize<ChangePasswordUsingTokenRequest>(request)}...");

					_c.Log("Verifying token...");
					string token = InfoCompass.World.Common.Helper.CreateHashFromPassword(request?.Token ?? "");
					PasswordResetRequest? passwordResetRequestFromDb = (await _serviceForPasswordResetRequests.Get<PasswordResetRequest>(prr => prr.Token == token)).SingleOrDefault();
					if(passwordResetRequestFromDb == null)
					{
						r = new ServerResponse<User>(false, "Password request was not found. Please go to Login page > Forgot passowrd to create a new one.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
					}
					else if(passwordResetRequestFromDb.MomentOfExpirationAsUtc.Ticks < DateTime.UtcNow.Ticks)
					{
						r = new ServerResponse<User>(false, "Your password reset request has expired. Please go to Login page > Forgot passowrd to create a new one.", null, null, errors, null, System.Net.HttpStatusCode.Unauthorized);
					}
					else if(!passwordResetRequestFromDb.IsActive)
					{
						r = new ServerResponse<User>(false, "Your password reset request has already been used up. Please go to Login page > Forgot passowrd to create a new one.", null, null, errors, null, System.Net.HttpStatusCode.Unauthorized);
					}
					else
					{
						User user = await _serviceForUsers.GetById<User>(passwordResetRequestFromDb.UserId);
						if(user == null)
						{
							r = new ServerResponse<User>(false, "User for password reset request was not found.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
						}
						else
						{
							_c.Log("Preparing email...");

							var mailMessage = new MailMessage(new MailAddress(_c.Settings.MailServerFromEMail, _c.Settings.MailServerFromName), new MailAddress(user.EMail));
							mailMessage.IsBodyHtml = true;
							mailMessage.Subject = "Password changed!";
							string link = _c.Settings.GuiUrl;
							mailMessage.Body = $"Your password has just been changed. If this is unexpected please report case by replying to this email.<br/><br/>To create another book go to <a href=\"{link}\">{_c.Settings.InstanceName}</a>.<br/><br/>Sincerely,<br/>Your {_c.Settings.InstanceName} Team";

							_c.Log($"Sending email to {user.EMail}...");
							CraftSynth.BuildingBlocks.IO.EMail.SendMail(_c.Settings.MailServerHostName,
																		_c.Settings.MailServerPort,
																		_c.Settings.MailServerUsername,
																		_c.Settings.MailServerPassword,
																		mailMessage,
																		new CustomTraceLog(),
																		true,
																		true,
																		null,
																		SmtpDeliveryMethod.Network);
							_c.Log($"Mail sent successfully to {user.EMail}.");

							_c.Log("Updating PasswordResetRequest in db...");
							passwordResetRequestFromDb = (await _serviceForPasswordResetRequests.Get<PasswordResetRequest>(prr => prr.Token == token)).SingleOrDefault();
							passwordResetRequestFromDb.IsActive = false;
							passwordResetRequestFromDb.ReasonForDeactivation = PasswordResetRequestReasonForDeactivation.UsedUp.ToString();
							await _serviceForPasswordResetRequests.Update(passwordResetRequestFromDb);
							_c.Log("Updating user record in db...");
							user = await _serviceForUsers.GetById<User>(user.Id);
							user.PasswordHash = InfoCompass.World.Common.Helper.CreateHashFromPassword(request.NewPassword);
							user.MomentOfLastUpdate = DateTime.Now;
							await _serviceForUsers.Update(user);
							long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__Password_Changed, _c.CurrentlyLoggedInUser, $"Password changed using token {request.Token} and passwordResetRequestId={passwordResetRequestFromDb.Id}", LogExtraColumnNames.UserId, user.Id);
							_c.Log($"Password changed using token {request.Token} and passwordResetRequestId={passwordResetRequestFromDb.Id}. LogId={logId}");
							user.PasswordHash = "(removed for safety)";
							r = new ServerResponse<User>(true, "You successfully changed your password!", null, user, errors, null, System.Net.HttpStatusCode.OK);
							_c.Log("All done!");
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing ChangePasswordUsingToken({request?.Token}");
		}

		return r;
	}

	public async Task<ServerResponse<User>> ChangePassword(ChangePasswordRequest request)
	{
		ServerResponse<User> r = null;
		var errors = new Errors();

		try
		{
			this.RequireCurrentlyLoggedInUser();

			request.Trim();
			request.Validate(_c, errors);

			if(errors.Count > 0)
			{
				r = new ServerResponse<User>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{

				using(_c.LogBeginScope($"Changing password:{request?.UserId})..."))
				{
					_c.Log($"Request={Json.Serialize<ChangePasswordRequest>(request)}...");


					if(_c.CurrentlyLoggedInUser.Id != request.UserId && !_c.CurrentlyLoggedInUser.IsAdministrator)
					{
						r = new ServerResponse<User>(false, "Non-admin users can change only own password.", null, null, errors, null, System.Net.HttpStatusCode.Unauthorized);
					}
					else
					{
						User user = await _serviceForUsers.GetById<User>(request.UserId);
						if(user == null)
						{
							r = new ServerResponse<User>(false, "User for password change request was not found.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
						}
						else
						{

							_c.Log("Checking by old password...");
							if(user.PasswordHash != InfoCompass.World.Common.Helper.CreateHashFromPassword(request.OldPassword))
							{
								r = new ServerResponse<User>(false, "Old password does not match our records. If you forgot your password log out and use Forgot password option.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
							}
							else
							{

								_c.Log("Preparing email...");

								var mailMessage = new MailMessage(new MailAddress(_c.Settings.MailServerFromEMail, _c.Settings.MailServerFromName), new MailAddress(user.EMail));
								mailMessage.IsBodyHtml = true;
								mailMessage.Subject = "Password changed!";
								string link = _c.Settings.GuiUrl;
								mailMessage.Body = $"Your password has just been changed. If this is unexpected please report case by replying to this email.<br/><br/>To create another book go to <a href=\"{link}\">{_c.Settings.InstanceName}</a>.<br/><br/>Sincerely,<br/>Your {_c.Settings.InstanceName} Team";

								_c.Log($"Sending email to {user.EMail}...");
								CraftSynth.BuildingBlocks.IO.EMail.SendMail(_c.Settings.MailServerHostName,
																			_c.Settings.MailServerPort,
																			_c.Settings.MailServerUsername,
																			_c.Settings.MailServerPassword,
																			mailMessage,
																			new CustomTraceLog(),
																			true,
																			true,
																			null,
																			SmtpDeliveryMethod.Network);
								_c.Log($"Mail sent successfully to {user.EMail}.");

								_c.Log("Updating user record in db...");
								user = await _serviceForUsers.GetById<User>(user.Id);
								user.PasswordHash = InfoCompass.World.Common.Helper.CreateHashFromPassword(request.NewPassword);
								user.MomentOfLastUpdate = DateTime.Now;
								await _serviceForUsers.Update(user);
								_c.Log("Db records updated...");
								long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__Password_Changed, _c.CurrentlyLoggedInUser, $"Password changed by logged in user with id {request.UserId}", LogExtraColumnNames.UserId, user.Id);
								_c.Log($"Password changed by logged in user with id {request.UserId}. LogId={logId}");
								user.PasswordHash = "(removed for safety)";
								r = new ServerResponse<User>(true, "You successfully changed your password!", null, user, errors, null, System.Net.HttpStatusCode.OK);
								_c.Log("All done!");
							}
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing ChangePassword({request?.UserId}");
		}

		return r;
	}

	public async Task<ServerResponse<User>> EditDetails(User request)
	{
		ServerResponse<User> r = null;
		var errors = new Errors();

		try
		{
			this.RequireCurrentlyLoggedInUser();

			request.Trim();
			request.Validate(_c, errors);

			if(errors.Count > 0)
			{
				r = new ServerResponse<User>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{

				using(_c.LogBeginScope($"Updating user with id:{request?.Id} and email:{request?.EMail}..."))
				{
					string requestAsJson = Json.Serialize<User>(request);
					_c.Log($"Request={requestAsJson}...");


					if(_c.CurrentlyLoggedInUser.Id != request.Id && !_c.CurrentlyLoggedInUser.IsAdministrator)
					{
						r = new ServerResponse<User>(false, "Non-admin users can change only own user details.", null, null, errors, null, System.Net.HttpStatusCode.Unauthorized);
					}
					else
					{
						User user = await _serviceForUsers.GetById<User>(request.Id);
						if(user == null)
						{
							r = new ServerResponse<User>(false, "User for was not found.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
						}
						else
						{
							_c.Log("Updating user record in db...");
							user.FirstName = request.FirstName;
							user.LastName = request.LastName;
							user.MomentOfLastUpdate = DateTime.Now;
							await _serviceForUsers.Update(user);
							_c.Log("Db records updated...");
							long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Details_Changed, _c.CurrentlyLoggedInUser, requestAsJson, LogExtraColumnNames.UserId, user.Id);
							_c.Log($"User details changed by logged in user with id {_c.CurrentlyLoggedInUser.Id} for user with id {request.Id}. LogId={logId}");
							user.PasswordHash = "(removed for safety)";
							r = new ServerResponse<User>(true, "You successfully changed your details!", null, user, errors, null, System.Net.HttpStatusCode.OK);
							_c.Log("All done!");
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing EditDetails({request?.Id}");
		}

		return r;
	}

	public async Task<ServerResponse<string>> PostUserMessage(UserMessage userMessage)
	{
		ServerResponse<string> r = null;
		var errors = new Errors();

		try
		{
			userMessage.Trim();
			userMessage.Validate(_c, errors);

			if(errors.Count > 0)
			{
				r = new ServerResponse<string>(false, "Please fill in form properly.", null, null, errors, null, System.Net.HttpStatusCode.BadRequest);
			}
			else
			{
				using(_c.LogBeginScope($"Posting user message:..."))
				{
					User user = await _serviceForUsers.GetById<User>(userMessage.UserId);
					if(user == null && userMessage.UserEMail != null)
					{
						user = await _serviceForUsers.GetByEMail(userMessage.UserEMail);
					}

					if(userMessage.UserId > 0 && user == null)
					{
						r = new ServerResponse<string>(false, $"User from message request was not found ({userMessage.UserId}).", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
					}
					else
					{
						if(user != null && !user.IsActive)
						{
							r = new ServerResponse<string>(false, $"User from message request is deactivated in our records ({userMessage.UserId}). If you believe this is an error contact us.", null, null, errors, null, System.Net.HttpStatusCode.NotFound);
						}
						else
						{
							_c.Log("Updating userMessage record in db...");
							if(user != null && userMessage.UserEMail == null)
							{
								userMessage.UserEMail = user.EMail;
							}
							DateTime now = DateTime.Now;
							if(userMessage.MomentOfCreation == null)
							{
								userMessage.MomentOfCreation = now;
							}
							if(userMessage.MomentOfLastUpdate == null)
							{
								userMessage.MomentOfLastUpdate = now;
							}
							await _serviceForUserMessages.Insert(userMessage);
							_c.Log("Db records updated...");

							_c.Log("Preparing email...");
							string fromEMail = (user?.EMail).ToNonNullNonEmptyString((userMessage?.UserEMail).ToNonNullNonEmptyString(_c.Settings.MailServerFromEMail));
							var mailMessage = new MailMessage(new MailAddress(fromEMail), new MailAddress(_c.Settings.ContactEmail));
							mailMessage.IsBodyHtml = false;
							string link = _serviceForSettings.GetCachedOrFromDbForUserDefault().Result.GuiUrl;
							mailMessage.Subject = $"Message from user of {_c.Settings.InstanceName}!";

							string userMessageMessage = userMessage.Message;
							string userMessageUserEMail = userMessage.UserEMail;
							userMessage.Message = "(See above)";
							userMessage.UserEMail = userMessage.UserEMail.ToNonNullString() + $" (autodetected: {fromEMail})";
							string userName = "";
							if((user?.FirstName).IsNOTNullOrWhiteSpace() || (user?.LastName).IsNOTNullOrWhiteSpace())
							{
								userName = $"FirstName: {user?.FirstName}\r\nLastName: {user?.LastName}\r\n";
							}
							mailMessage.Body = $"{userMessageMessage}\r\n\r\n-----------------------------\r\n{userName}{userMessage.ToString("\r\n")}";

							userMessage.Message = userMessageMessage;
							userMessage.UserEMail = userMessageUserEMail;

							_c.Log($"Sending email to {_c.Settings.ContactEmail}...");
							CraftSynth.BuildingBlocks.IO.EMail.SendMail(_c.Settings.MailServerHostName,
																		_c.Settings.MailServerPort,
																		_c.Settings.MailServerUsername,
																		_c.Settings.MailServerPassword,
																		mailMessage,
																		new CustomTraceLog(),
																		true,
																		true,
																		null,
																		SmtpDeliveryMethod.Network);
							_c.Log($"Mail sent successfully from {fromEMail} to {_c.Settings.ContactEmail}.");

							long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Posted_Message, _c.CurrentlyLoggedInUser, $"User posted message with id={userMessage?.Id}. userId={userMessage?.UserId}. EMail={userMessage?.UserEMail}", LogExtraColumnNames.UserId, user?.Id);
							_c.Log($"User posted message with id={userMessage.Id}. userId={userMessage.UserId}. EMail={userMessage.UserEMail}. LogId={logId}");
							r = new ServerResponse<string>(true, "Thank you! Your message has been sent.", null, "Thank you! Your message has been sent.", errors, null, System.Net.HttpStatusCode.OK);
							_c.Log("All done!");
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while PostUserMessage({userMessage.ToString(",")}");
		}

		return r;
	}
}

public class MockedServiceForUsers:IServiceForUsers
{
	readonly ServiceForCOE _c;
	readonly InfoCompass.World.DataAccessContracts.IServiceForUsers _serviceForUsers;

	public MockedServiceForUsers(ServiceForCOE c, InfoCompass.World.DataAccessContracts.IServiceForUsers serviceForUsers)
	{
		_c = c;
		_serviceForUsers = serviceForUsers;
	}
	public Task<User> GetByEMail(string email)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<User>> ValidateLoginCredentials(LoginRequest loginRequest)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<bool>> Logout()
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<string>> CreatePasswordResetRequest(string eMail)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<User>> ChangePasswordUsingToken(ChangePasswordUsingTokenRequest request)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<User>> ChangePassword(ChangePasswordRequest request)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<User>> UpdateMomentOfLastLogin(long userId, DateTime momentOfLastLogin)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<bool>> VerifyPasswordResetToken(string token)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<User>> EditDetails(User request)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<User>> StartRegistrationProcess(RegisterRequest request)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<User>> FinishRegistrationProcess(string token)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<User>> Register(long userId)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<string>> PostUserMessage(UserMessage userMessage)
	{
		throw new NotImplementedException();
	}
}