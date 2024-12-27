using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MyCompany.World.BusinessLogic;
using MyCompany.World.Public.Api;
using MyCompany.World.UiWebApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Error = MyCompany.World.Common.Entities.Error;

namespace MyCompany.World.UiWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController:BaseController
{
	readonly ServiceForCOE _c;
	readonly IServiceForLogs _serviceForLogs;
	readonly IServiceForSettings _serviceForSettings;
	readonly IServiceForUsers _serviceForUsers;
	readonly IServiceForJwts _serviceForJwts;

	public UsersController(ServiceForCOE c, IServiceForLogs logs, IServiceForUsers serviceForUsers, IServiceForSettings serviceForSettings, IServiceForJwts serviceForJwts)
	{
		_c = c;
		_serviceForLogs = logs;
		_serviceForSettings = serviceForSettings;
		_serviceForUsers = serviceForUsers;
		_serviceForJwts = serviceForJwts;
	}

	//[HttpGet(Name = "GetWeatherForecast")]
	//public async Task<IActionResult> Get()
	//{
	//	JsonResult r;

	//	using(_c.LogBeginScope($"Executing api call IssueByScript on url:{this.CurrentRequestUri}... "))
	//	{
	//		try
	//		{
	//			//---api method body--
	//			WeatherForecast[] w = Enumerable.Range(1, 5).Select(index => new WeatherForecast
	//			{
	//				Date = DateTime.Now.AddDays(index),
	//				TemperatureC = Random.Shared.Next(-20, 55),
	//				Summary = _c.Settings.Description// Summaries[Random.Shared.Next(Summaries.Length)]
	//			}).ToArray();
	//			r = new JsonResult(w);
	//			//--------------------
	//		}
	//		catch(Exception ex)
	//		{
	//			r = new JsonResult(this.HandleExceptionOnThisLayer(ex));
	//		}
	//	}
	//	return r;
	//}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="value">
	/// {
	///  "username": "f4cio1@gmail.com",
	///  "password": "12345"
	/// }
	/// </param>
	/// <returns></returns>
	// POST api/<HomeController>
	[HttpPost]
	[Route("Login")]
	public async Task<ServerResponseForUI<User>> Login([FromBody] LoginRequest request)
	{
		ServerResponseForUI<User> r;

		using(_c.LogBeginScope($"Executing api call Login on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--
				ServerResponse<User> serverResponse = await _serviceForUsers.ValidateLoginCredentials(request);
				if(serverResponse.IsSuccess)
				{
					User userFromDb = await _serviceForUsers.GetByEMail(request.Username);
					if(!userFromDb.IsRegistered)
					{
						serverResponse = await _serviceForUsers.Register(userFromDb.Id);
						if(!serverResponse.IsSuccess)
						{
							r = new ServerResponseForUI<User>(_c, serverResponse.IsSuccess, serverResponse.Message, serverResponse.LogId, null, serverResponse.Errors, serverResponse.MessageColor, serverResponse.HttpStatusCode);
						}
						else
						{
							userFromDb = serverResponse.Data;
						}
					}
					serverResponse = await this.CreateCookieAuthentication(userFromDb);
				}
				if(serverResponse.Data as User != null)
				{
					(serverResponse.Data as User).PasswordHash = "(removed for safety)";
				}
				r = new ServerResponseForUI<User>(_c, serverResponse);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<User>(ex);
			}
		}

		return r;
	}

	private async Task<ServerResponseForUI<User>> CreateCookieAuthentication(User user)
	{
		ServerResponseForUI<User> r = null;

		var claims = new List<Claim>
					{
						new Claim(ClaimTypes.Name, user.EMail),
						//new Claim(ClaimTypes.Role, "User"), // Example role
					};
		var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

		var authProperties = new AuthenticationProperties
		{
			//if expiration is set here renew on every request will not happen so setting expiration in services.AddAuthentication instead
			//IsPersistent = true,
			//ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(settings.LoginExpirationPeriodInMinutes) // Dynamically set expiration
		};

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(claimsIdentity),
			authProperties);

		_c.CurrentlyLoggedInUser = user;
		_c.LoginHappenedInThisRequest = true;

		await _serviceForUsers.UpdateMomentOfLastLogin(user.Id, DateTime.Now);

		string message = $"User logged in. user.Id={user.Id}, user.EMail={user.EMail}, Authentication kind=Cookie.";
		long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Logged_In, user.Id, message);
		_c.Log(message + $"For more details see log with id={logId}");
		r = new ServerResponseForUI<User>(_c, true, "Ok", logId, user, null, null, System.Net.HttpStatusCode.OK);
		return r;
	}

	//[HttpGet(Name = "GetWeatherForecast")]
	//public async Task<IActionResult> Get()
	//{
	//	JsonResult r;

	//	using(_c.LogBeginScope($"Executing api call IssueByScript on url:{this.CurrentRequestUri}... "))
	//	{
	//		try
	//		{
	//			//---api method body--
	//			WeatherForecast[] w = Enumerable.Range(1, 5).Select(index => new WeatherForecast
	//			{
	//				Date = DateTime.Now.AddDays(index),
	//				TemperatureC = Random.Shared.Next(-20, 55),
	//				Summary = _c.Settings.Description// Summaries[Random.Shared.Next(Summaries.Length)]
	//			}).ToArray();
	//			r = new JsonResult(w);
	//			//--------------------
	//		}
	//		catch(Exception ex)
	//		{
	//			r = new JsonResult(this.HandleExceptionOnThisLayer(ex));
	//		}
	//	}
	//	return r;
	//}

	/// <summary>
	/// 
	/// {
	///   "firstName": "Nele",
	///   "lastName": "Nelic",
	///   "eMail": "t@f4cio.com",
	///   "subscribeMe": true,
	///   "newPassword": "pomorandza",
	///   "newPasswordRepeated": "pomorandza",
	///   "iAgreeToTerms": true
	/// }
	/// 
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	[HttpPost]
	[Route("StartRegistrationProcess")]
	public async Task<ServerResponseForUI<User>> StartRegistrationProcess(RegisterRequest request)
	{
		ServerResponseForUI<User> r;

		using(_c.LogBeginScope($"Executing api call StartRegistrationProcess on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--	
				ServerResponse<User> rInner = await _serviceForUsers.StartRegistrationProcess(request);
				r = new ServerResponseForUI<User>(_c, rInner);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<User>(ex);
			}
		}

		return r;
	}

	[HttpGet]
	[Route("FinishRegistrationProcessAndReturnUserAndJwt")]
	public async Task<ServerResponseForUI<UserAndJwt>> FinishRegistrationProcessAndReturnUserAndJwt(string token)
	{
		ServerResponseForUI<UserAndJwt> r;

		using(_c.LogBeginScope($"Executing api call FinishRegistrationProcessAndReturnUserAndJwt on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--
				ServerResponse<User> serverResponse = await _serviceForUsers.FinishRegistrationProcess(token);
				if(!serverResponse.IsSuccess)
				{
					r = new ServerResponseForUI<UserAndJwt>(_c, serverResponse.IsSuccess, serverResponse.Message, serverResponse.LogId, null, serverResponse.Errors, serverResponse.MessageColor, serverResponse.HttpStatusCode);
				}
				else
				{
					r = await this.CreateJwtAuthentication(serverResponse.Data);
				}
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<UserAndJwt>(ex);
			}
		}

		return r;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="value">
	/// {
	///  "username": "f4cio1@gmail.com",
	///  "password": "12345",
	/// } 
	/// </param>
	/// <returns></returns>
	// POST api/<HomeController>
	[HttpPost]
	[Route("LoginAndReturnUserAndJwt")]
	public async Task<ServerResponseForUI<UserAndJwt>> LoginAndReturnUserAndJwt([FromBody] LoginRequest request)
	{
		ServerResponseForUI<UserAndJwt> r;

		using(_c.LogBeginScope($"Executing api call Login on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--
				ServerResponse<User> serverResponse = await _serviceForUsers.ValidateLoginCredentials(request);
				if(!serverResponse.IsSuccess)
				{
					r = new ServerResponseForUI<UserAndJwt>(_c, serverResponse.IsSuccess, serverResponse.Message, serverResponse.LogId, null, serverResponse.Errors, serverResponse.MessageColor, serverResponse.HttpStatusCode);
				}
				else
				{
					User userFromDb = await _serviceForUsers.GetByEMail(request.Username);
					if(!userFromDb.IsRegistered)
					{
						serverResponse = await _serviceForUsers.Register(userFromDb.Id);
						if(!serverResponse.IsSuccess)
						{
							r = new ServerResponseForUI<UserAndJwt>(_c, serverResponse.IsSuccess, serverResponse.Message, serverResponse.LogId, null, serverResponse.Errors, serverResponse.MessageColor, serverResponse.HttpStatusCode);
						}
						else
						{
							userFromDb = serverResponse.Data;
						}
					}
					r = await this.CreateJwtAuthentication(userFromDb);
				}

				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<UserAndJwt>(ex);
			}
		}

		return r;
	}

	private async Task<ServerResponseForUI<UserAndJwt>> CreateJwtAuthentication(User user)
	{
		ServerResponseForUI<UserAndJwt> r;

		Settings settings = _serviceForSettings.GetCachedOrFromDbForUserDefault().Result;
		var tokenHandler = new JwtSecurityTokenHandler();
		DateTime momentOfExpirationAsUtc = DateTime.UtcNow.AddMinutes(settings.LoginExpirationPeriodInMinutes);
		byte[] key = Encoding.ASCII.GetBytes(settings.LoginSecretKeyForJwt);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.Name, user.EMail),
				// additional claims
			}),
			Expires = momentOfExpirationAsUtc,
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
		string tokenAsString = tokenHandler.WriteToken(token);

		await _serviceForJwts.Create(user.Id, tokenAsString, momentOfExpirationAsUtc);

		_c.CurrentlyLoggedInUser = user;
		_c.LoginHappenedInThisRequest = true;

		await _serviceForUsers.UpdateMomentOfLastLogin(user.Id, DateTime.Now);

		string message = $"User logged in. user.Id={user.Id}, user.EMail={user.EMail}, Authentication kind=Jwt, Token={tokenAsString.Bubble(10, "...")}, MomentOfExpirationAsUtc={momentOfExpirationAsUtc.ToDDateAndTimeAs_MM_DD_YYYY__HH_MM_SS()}.";
		long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Logged_In, user.Id, message);
		_c.Log(message + $"For more details see log with id={logId}");
		var userAndJwt = new UserAndJwt(user, tokenAsString, momentOfExpirationAsUtc);
		userAndJwt.User.PasswordHash = "(removed for safety)";
		r = new ServerResponseForUI<UserAndJwt>(_c, true, "Ok", logId, userAndJwt, null, null, System.Net.HttpStatusCode.OK);

		return r;
	}

	[HttpGet]
	[Route("GetCurrentlyLoggedInUser")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
	public async Task<ServerResponseForUI<User>> GetCurrentlyLoggedInUser()
	{
		ServerResponseForUI<User> r;

		using(_c.LogBeginScope($"Executing api call GetByEMail on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--				
				if(_c.CurrentlyLoggedInUser == null)
				{
					r = new ServerResponseForUI<User>(_c, false, "No user is currently logged in.", null, null, null, null, System.Net.HttpStatusCode.NotFound);
				}
				else
				{
					_c.CurrentlyLoggedInUser.PasswordHash = "(removed for safety)";
					r = new ServerResponseForUI<User>(_c, true, "Ok", null, _c.CurrentlyLoggedInUser, null, null, System.Net.HttpStatusCode.OK);
				}
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<User>(ex);
			}
		}

		return r;
	}

	[HttpGet]
	[Route("Logout")]
	public async Task<ServerResponseForUI<bool>> Logout()
	{
		ServerResponseForUI<bool> r;

		using(_c.LogBeginScope($"Executing api call Logout on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--								
				if(_c.CurrentlyLoggedInUser == null)
				{
					await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
					_c.CurrentlyLoggedInUser = null;

					string message = $"User logged out performed even _c.CurrentlyLoggedInUser was null.";
					long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Logged_Out, (string?)null, message);
					_c.Log(message + $"For more details see log with id={logId}.");
					r = new ServerResponseForUI<bool>(_c, true, "Ok", logId, false, null, null, System.Net.HttpStatusCode.OK);
				}
				else
				{
					User user = _c.CurrentlyLoggedInUser;
					string? authenticationType = HttpContext?.User?.Identity?.AuthenticationType;

					switch(authenticationType)
					{
						case "Cookies":
							await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
							break;
						case "Bearer":
							string authorizationHeader = HttpContext.Request.Headers["Authorization"];
							if(!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
							{
								string token = authorizationHeader.Substring("Bearer ".Length).Trim();
								await _serviceForJwts.Deactivate(token, JwtReasonForDeactivation.UserLogedOutBeforeExpiration, true);
							}
							break;
						default:
							throw new Error($"Authentication type {authenticationType} not supported.");
					}

					_c.CurrentlyLoggedInUser = null;

					string message = $"User loggout performed. user.Id={user.Id}, user.EMail={user.EMail}, authenticationType={authenticationType}.";
					long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Logged_Out, user.Id, message);
					_c.Log(message + $"For more details see log with id={logId}");
					r = new ServerResponseForUI<bool>(_c, true, "Ok", logId, true, null, null, System.Net.HttpStatusCode.OK);
				}
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<bool>(ex);
			}
		}

		return r;
	}

	[HttpGet]
	[Route("CreatePasswordResetRequest")]
	public async Task<ServerResponseForUI<string>> CreatePasswordResetRequest(string email)
	{
		ServerResponseForUI<string> r;

		using(_c.LogBeginScope($"Executing api call CreatePasswordResetRequest on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--	
				ServerResponse<string> rInner = await _serviceForUsers.CreatePasswordResetRequest(email);
				r = new ServerResponseForUI<string>(_c, rInner);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<string>(ex);
			}
		}

		return r;
	}

	[HttpGet]
	[Route("VerifyPasswordResetToken")]
	public async Task<ServerResponseForUI<bool>> VerifyPasswordResetToken(string token)
	{
		ServerResponseForUI<bool> r;

		using(_c.LogBeginScope($"Executing api call VerifyPasswordResetToken on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--	
				ServerResponse<bool> rInner = await _serviceForUsers.VerifyPasswordResetToken(token);
				r = new ServerResponseForUI<bool>(_c, rInner);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<bool>(ex);
			}
		}

		return r;
	}

	[HttpPost]
	[Route("ChangePasswordUsingToken")]
	public async Task<ServerResponseForUI<User>> ChangePasswordUsingToken(ChangePasswordUsingTokenRequest request)
	{
		ServerResponseForUI<User> r;

		using(_c.LogBeginScope($"Executing api call ChangePasswordUsingToken on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--	
				ServerResponse<User> rInner = await _serviceForUsers.ChangePasswordUsingToken(request);
				r = new ServerResponseForUI<User>(_c, rInner);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<User>(ex);
			}
		}

		return r;
	}

	[HttpPost]
	[Route("ChangePassword")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
	public async Task<ServerResponseForUI<User>> ChangePassword(ChangePasswordRequest request)
	{
		ServerResponseForUI<User> r;

		using(_c.LogBeginScope($"Executing api call ChangePassword on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--	
				ServerResponse<User> rInner = await _serviceForUsers.ChangePassword(request);
				r = new ServerResponseForUI<User>(_c, rInner);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<User>(ex);
			}
		}

		return r;
	}

	// PUT api/UsersController>/5
	[HttpPut("EditDetails")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
	public async Task<ServerResponseForUI<User>> EditDetails(User request)
	{
		ServerResponseForUI<User> r;

		using(_c.LogBeginScope($"Executing api call EditDetails on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--	
				ServerResponse<User> rInner = await _serviceForUsers.EditDetails(request);
				r = new ServerResponseForUI<User>(_c, rInner);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<User>(ex);
			}
		}

		return r;
	}

	/// <summary>
	/// {
	///   "id": 0,
	///   "userId": 0,
	///   "userEMail": "f4cio1@gmail.com",
	///   "momentOfCreation": "2023-11-19T22:41:02.103Z",
	///   "momentOfLastUpdate": "2023-11-19T22:41:02.103Z",
	///   "rawData": "Hello, this is test...",
	///   "isRead": true,
	///   "isVisible": true,
	///   "note": "",
	///   "threadId": 0,
	///   "previousMessageId": 0,
	///   "nextMessageId": 0
	/// }
	/// 
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	[HttpPost]
	[Route("PostUserMessage")]
	//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
	public async Task<ServerResponseForUI<string>> PostUserMessage(UserMessage request)
	{
		ServerResponseForUI<string> r;

		using(_c.LogBeginScope($"Executing api call PostUserMessage on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--	
				ServerResponse<string> rInner = await _serviceForUsers.PostUserMessage(request);
				r = new ServerResponseForUI<string>(_c, rInner);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<string>(ex);
			}
		}

		return r;
	}
}