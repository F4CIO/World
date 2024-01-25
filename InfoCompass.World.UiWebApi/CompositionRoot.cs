using System.IdentityModel.Tokens.Jwt;
using InfoCompass.World.BusinessLogic;
using InfoCompass.World.UiWebApi.Logic;
using InfoCompass.World.UiWebApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using static InfoCompass.World.Common.ServiceForCOE;
using Error = InfoCompass.World.Common.Entities.Error;

namespace InfoCompass.World.UiWebApi;

internal class CompositionRoot
{
	public static void Compose(Microsoft.Extensions.Hosting.HostBuilderContext context, IServiceCollection services, bool isTesting)
	{
		if(isTesting)
		{
			services.Configure<Configuration>(options => context.Configuration.GetSection("Main").Bind(options));

			//Log.Logger = new LoggerConfiguration()
			//	.MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Adjust this to filter out noise
			//	.MinimumLevel.Override("System", LogEventLevel.Warning)
			//	.WriteTo.File(CraftSynth.BuildingBlocks.Common.Misc.ApplicationPhysicalExeFilePathWithoutExtension + ".log"
			//	  , outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Scope} > {Message} {Exception} Properties: {Properties}")
			//	.CreateLogger();
			services.AddSingleton<ServiceForLogSink_FilePerUserPerJob>();
			services.AddSingleton<ServiceForLogSink_Db>();
			//Log.Logger = new LoggerConfiguration...will be executed after services are built
			services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddSerilog();
			});

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddScoped<ServiceForCOE>(provider =>
			{
				IServiceScopeFactory serviceScopeFactory = provider.GetService<IServiceScopeFactory>();
				Microsoft.Extensions.Options.IOptions<Configuration>? serviceForConfiguration = provider.GetService<Microsoft.Extensions.Options.IOptions<Configuration>>();
				ILogger<ServiceForCOE>? serviceForLogger = provider.GetService<ILogger<ServiceForCOE>>();
				OnReviewCurrentlyLoggedInUserDelegate onReviewCurrentlyLoggedInUser = (ServiceForCOE c, User currentlyLoggedInUserReadOnly, bool loginHappenedInThisRequest) =>
				{
					User r = null;
					bool authenticated = c.HttpContext?.User?.Identity?.IsAuthenticated == true;

					if(!authenticated && currentlyLoggedInUserReadOnly == null)
					{
						//not authenticated and all ready=>do nothing
						r = currentlyLoggedInUserReadOnly;
					}
					if(authenticated && currentlyLoggedInUserReadOnly != null)
					{
						//authenticated and all ready=>do nothing
						r = currentlyLoggedInUserReadOnly;
					}
					if(authenticated && currentlyLoggedInUserReadOnly == null)
					{//user authenticated but _c.CurrentlyLoggedInUserReadOnly is not yet set =>set _c.CurrentlyLoggedInUserReadOnly
						string? eMail = c.HttpContext.User.Identity.Name;
						//var eMail = _httpContext.User.FindFirst("EMail")?.Value;
						IServiceForUsers _serviceForUsers = c.ServiceProvider.GetRequiredService<IServiceForUsers>();
						IServiceForJwts _serviceForJwts = c.ServiceProvider.GetRequiredService<IServiceForJwts>();
						r = _serviceForUsers.GetByEMail(eMail).Result;
						if(r == null)
						{
							try
							{
								c.Log($"Logged in user email is {eMail} but it was not found in db.Logging him out...");
							}
							catch { }

							try
							{
								//cookie based logout
								c.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
							}
							catch { }

							try
							{
								//jwt based logout
								string authorizationHeader = c.HttpContext.Request.Headers["Authorization"];
								if(!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
								{
									string token = authorizationHeader.Substring("Bearer ".Length).Trim();

									_serviceForJwts.Deactivate(token, JwtReasonForDeactivation.UserLogedOutBeforeExpiration, false).Wait();
								}
							}
							catch { }
						}
					}
					else if(!authenticated && currentlyLoggedInUserReadOnly != null && !loginHappenedInThisRequest)
					{//user not authenticated anymore but _c.CurrentlyLoggedInUserReadOnly is still set =>log logout and nullify _c.CurrentlyLoggedInUserReadOnly
						User user = currentlyLoggedInUserReadOnly;

						IServiceForLogs _serviceForLogs = c.ServiceProvider.GetRequiredService<IServiceForLogs>();
						string message = $"User loggout performed because session/cookie expired/not found. user.Id={user.Id}, user.EMail={user.EMail}.";
						long logId = _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Logged_Out, user.Id, message).Result;
						c.Log(message + $"For more details see log with id={logId}");

						r = null;
					}

					return r;
				};
				OnInitSettingsDelegate onInitSettings = (ServiceForCOE c) =>
				{
					//we must init settings later on someones get to awoid circular call in constructor below
					//we could also put this code in property get but then we would have dll circular dependency
					//Settings settings = provider.GetRequiredService<IServiceForSettings>().GetCachedOrFromDbForUserDefault().Result;

					//now the above line would generally work but not in background tasks like ServiceForLogSink_Db
					//so we need to first get scope and create settings service from it
					using IServiceScope scope = c._scopeFactory.CreateScope();  // This will use the current scope if available
					IServiceForSettings settingsService = scope.ServiceProvider.GetRequiredService<IServiceForSettings>();
					Settings settings = settingsService.GetCachedOrFromDbForUserDefault().Result;

					return settings;
				};
				IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
				User currentlyLoggedInUser = InfoCompass.World.UiWebApi.Logic.HandlerForAuthentication.CurrentlyLoggedInUser;
				var c = new ServiceForCOE(provider, serviceScopeFactory, serviceForConfiguration, serviceForLogger, onReviewCurrentlyLoggedInUser, onInitSettings, httpContextAccessor, currentlyLoggedInUser, null, true, null);
				return c;
			}
			);
			InfoCompass.World.BusinessLogic.CompositionRoot.Compose(context, services, isTesting);
			services.AddScoped<IServiceForBase, ServiceForBase>();
			//services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			//	.AddCookie(options =>
			//	{
			//		options.LoginPath = "/Users/Login/";
			//		options.ExpireTimeSpan = TimeSpan.FromDays(1); // Setting the expiration to 1 day
			//		options.SlidingExpiration = true; // This is optional, enabling it resets the expiration time on each request
			//	});
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddJwtBearer().AddCookie();
			services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				ServiceProvider serviceProvider = services.BuildServiceProvider();
				IServiceForSettings settingsService = serviceProvider.GetRequiredService<IServiceForSettings>();
				Settings settings = settingsService.GetCachedOrFromDbForUserDefault().Result;
				options.ExpireTimeSpan = TimeSpan.FromMinutes(settings.LoginExpirationPeriodInMinutes);
			});
			services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				ServiceProvider serviceProvider = services.BuildServiceProvider();
				IServiceForSettings settingsService = serviceProvider.GetRequiredService<IServiceForSettings>();
				Settings settings = settingsService.GetCachedOrFromDbForUserDefault().Result;
				byte[] key = Encoding.ASCII.GetBytes(settings.LoginSecretKeyForJwt);

				options.RequireHttpsMetadata = false; // In production, set to true
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false
				};
				options.Events = new JwtBearerEvents
				{
					OnChallenge = context =>
					{
						// This will prevent the redirect to login page or whatever default challenge handling
						context.HandleResponse();
						context.Response.StatusCode = 401;
						context.Response.ContentType = "application/json";
						return context.Response.WriteAsync("You are not authorized.");
					},
					OnTokenValidated = async context =>
					{
						IServiceForJwts serviceForJwts = context.HttpContext.RequestServices.GetRequiredService<IServiceForJwts>();
						var token = context.SecurityToken as JwtSecurityToken;
						if(token != null && serviceForJwts.IsPresentAndActive(token.RawData).Result)
						{
							context.Fail("This token is not present and active.");
							return;
						}
					}
				};
			});
		}
		else
		{
			services.Configure<Configuration>(options => context.Configuration.GetSection("Main").Bind(options));

			//Log.Logger = new LoggerConfiguration()
			//	.MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Adjust this to filter out noise
			//	.MinimumLevel.Override("System", LogEventLevel.Warning)
			//	.WriteTo.File(CraftSynth.BuildingBlocks.Common.Misc.ApplicationPhysicalExeFilePathWithoutExtension + ".log"
			//	  , outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Scope} > {Message} {Exception} Properties: {Properties}")
			//	.CreateLogger();
			services.AddSingleton<ServiceForLogSink_FilePerUserPerJob>();
			//Log.Logger = new LoggerConfiguration...will be executed after services are built
			services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.AddSerilog();
			});

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddSingleton<ServiceForLogSink_Db>();
			services.AddScoped<ServiceForCOE>(provider =>
			{
				IServiceScopeFactory serviceScopeFactory = provider.GetService<IServiceScopeFactory>();
				Microsoft.Extensions.Options.IOptions<Configuration>? serviceForConfiguration = provider.GetService<Microsoft.Extensions.Options.IOptions<Configuration>>();
				ILogger<ServiceForCOE>? serviceForLogger = provider.GetService<ILogger<ServiceForCOE>>();
				OnReviewCurrentlyLoggedInUserDelegate onReviewCurrentlyLoggedInUser = (ServiceForCOE c, User currentlyLoggedInUserReadOnly, bool loginHappenedInThisRequest) =>
				{
					User r = null;

					if(c.IsBackgroundTask)
					{//in jobs we dont have authenticated user from request. CurrentlyLoggedInUser was set on job intitalization and its not ever changed
						if(currentlyLoggedInUserReadOnly == null)
						{
							throw new Error("Developer's fault -c.IsBackgroundTask is true but c.CurrentlyLoggedInUser is null.");
						}
						r = currentlyLoggedInUserReadOnly;
					}
					else
					{//in request utilize authenticated user info
						bool authenticated = c.HttpContext?.User?.Identity?.IsAuthenticated == true;

						if(!authenticated && currentlyLoggedInUserReadOnly == null)
						{
							//not authenticated and all ready=>do nothing
							r = currentlyLoggedInUserReadOnly;
						}
						if(authenticated && currentlyLoggedInUserReadOnly != null)
						{
							//authenticated and all ready=>do nothing
							r = currentlyLoggedInUserReadOnly;
						}
						if(authenticated && currentlyLoggedInUserReadOnly == null)
						{//user authenticated but _c.CurrentlyLoggedInUserReadOnly is not yet set =>set _c.CurrentlyLoggedInUserReadOnly
							string? eMail = c.HttpContext.User.Identity.Name;
							//var eMail = _httpContext.User.FindFirst("EMail")?.Value;
							IServiceForUsers _serviceForUsers = c.ServiceProvider.GetRequiredService<IServiceForUsers>();
							IServiceForJwts _serviceForJwts = c.ServiceProvider.GetRequiredService<IServiceForJwts>();
							r = _serviceForUsers.GetByEMail(eMail).Result;
							if(r == null)
							{
								try
								{
									c.Log($"Logged in user email is {eMail} but it was not found in db.Logging him out...");
								}
								catch { }

								try
								{
									//cookie based logout
									c.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
								}
								catch { }

								try
								{
									//jwt based logout
									string authorizationHeader = c.HttpContext.Request.Headers["Authorization"];
									if(!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
									{
										string token = authorizationHeader.Substring("Bearer ".Length).Trim();

										_serviceForJwts.Deactivate(token, JwtReasonForDeactivation.UserLogedOutBeforeExpiration, false).Wait();
									}
								}
								catch { }
							}
						}
						else if(!authenticated && currentlyLoggedInUserReadOnly != null && !loginHappenedInThisRequest)
						{//user not authenticated anymore but _c.CurrentlyLoggedInUserReadOnly is still set =>log logout and nullify _c.CurrentlyLoggedInUserReadOnly
							User user = currentlyLoggedInUserReadOnly;

							IServiceForLogs _serviceForLogs = c.ServiceProvider.GetRequiredService<IServiceForLogs>();
							string message = $"User loggout performed because session/cookie expired/not found. user.Id={user.Id}, user.EMail={user.EMail}.";
							long logId = _serviceForLogs.Write(LogCategoryAndTitle.UserAction__User_Logged_Out, user.Id, message).Result;
							c.Log(message + $"For more details see log with id={logId}");

							r = null;
						}
					}

					return r;
				};
				OnInitSettingsDelegate onInitSettings = (ServiceForCOE c) =>
				{
					//we must init settings later on someones get to awoid circular call in constructor below
					//we could also put this code in property get but then we would have dll circular dependency
					//Settings settings = provider.GetRequiredService<IServiceForSettings>().GetCachedOrFromDbForUserDefault().Result;

					//now the above line would generally work but not in background tasks like ServiceForLogSink_Db
					//so we need to first get scope and create settings service from it
					using IServiceScope scope = c._scopeFactory.CreateScope();  // This will use the current scope if available
					IServiceForSettings settingsService = scope.ServiceProvider.GetRequiredService<IServiceForSettings>();
					Settings settings = settingsService.GetCachedOrFromDbForUserDefault().Result;

					return settings;
				};
				IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
				User currentlyLoggedInUser = InfoCompass.World.UiWebApi.Logic.HandlerForAuthentication.CurrentlyLoggedInUser;
				var c = new ServiceForCOE(provider, serviceScopeFactory, serviceForConfiguration, serviceForLogger, onReviewCurrentlyLoggedInUser, onInitSettings, httpContextAccessor, currentlyLoggedInUser, null, true, null);
				return c;
			}
			);
			InfoCompass.World.BusinessLogic.CompositionRoot.Compose(context, services, isTesting);
			services.AddScoped<IServiceForBase, ServiceForBase>();
			//services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			//	.AddCookie(options =>
			//	{
			//		options.LoginPath = "/Users/Login/";
			//		options.ExpireTimeSpan = TimeSpan.FromDays(1); // Setting the expiration to 1 day
			//		options.SlidingExpiration = true; // This is optional, enabling it resets the expiration time on each request
			//	});			
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer().AddCookie();//!!! default shceme is very important when authenticated users access [AllowAnonymous] mehtods or methods with no [Authorize]. On such methods default scheme will decide whether user is authenticated or not.
			services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				ServiceProvider serviceProvider = services.BuildServiceProvider();
				IServiceForSettings settingsService = serviceProvider.GetRequiredService<IServiceForSettings>();
				Settings settings = settingsService.GetCachedOrFromDbForUserDefault().Result;
				options.ExpireTimeSpan = TimeSpan.FromMinutes(settings.LoginExpirationPeriodInMinutes);

			});
			services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
			{
				ServiceProvider serviceProvider = services.BuildServiceProvider();
				IServiceForSettings settingsService = serviceProvider.GetRequiredService<IServiceForSettings>();
				Settings settings = settingsService.GetCachedOrFromDbForUserDefault().Result;
				byte[] key = Encoding.ASCII.GetBytes(settings.LoginSecretKeyForJwt);

				options.RequireHttpsMetadata = false; // In production, set to true
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false
				};
				options.Events = new JwtBearerEvents
				{
					OnChallenge = async context =>
					{
						// This will prevent the redirect to login page or whatever default challenge handling
						context.HandleResponse();
						context.Response.StatusCode = 401;
						context.Response.ContentType = "application/json; charset=utf-8";
						ServiceForCOE serviceForCOE = context.HttpContext.RequestServices.GetRequiredService<ServiceForCOE>();
						string result = System.Text.Json.JsonSerializer.Serialize(new ServerResponseForUI<string>(serviceForCOE, false, "Not authorized.", null, context?.Request?.Path.Value, null, null, System.Net.HttpStatusCode.Unauthorized));
						int byteData = Encoding.UTF8.GetBytes(result).Length;
						context.Response.ContentLength = byteData;
						await context.Response.WriteAsync(result);
						return;
					},
					OnTokenValidated = async context =>
					{
						// Bypass token validation for OPTIONS (preflight) requests
						if(context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
						{
							return;
						}

						IServiceForJwts serviceForJwts = context.HttpContext.RequestServices.GetRequiredService<IServiceForJwts>();
						var token = context.SecurityToken as JwtSecurityToken;
						if(token == null || !(await serviceForJwts.IsPresentAndActive(token.RawData)))
						{
							context.Fail("This token is not present and active.");
							return;
						}
					},
					OnAuthenticationFailed = context =>
					{
						// Bypass authentication failure handling for OPTIONS (preflight) requests
						if(context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
						{
							return Task.CompletedTask;
						}

						ServiceForCOE c = context.HttpContext.RequestServices.GetRequiredService<ServiceForCOE>();
						c.Log(context.Exception, $"Authentication failed. Error:{context?.Exception?.Message}");
						return Task.CompletedTask;
					}
				};
			});
		}
	}
}
