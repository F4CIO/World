using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace InfoCompass.World.Common;

/// <summary>
/// This class is simple container for system-wide properties like currentlyLoggedInUser, log etc. 
/// </summary>
public class ServiceForCOE
{
	readonly IServiceProvider _serviceProvider;
	public readonly IServiceScopeFactory _scopeFactory;
	readonly ILogger _logger;
	Settings _settings;
	Configuration _configuration;
	HttpContext _httpContext;
	User _currentlyLoggedInUser;
	UnitOfWork _parentUnitOfWork;
	bool _isAsync;
	object _tag;
	public delegate User OnReviewCurrentlyLoggedInUserDelegate(ServiceForCOE c, User currentlyLoggedInUser, bool loginHappenedInThisRequest);
	readonly OnReviewCurrentlyLoggedInUserDelegate _onReviewCurrentlyLoggedInUserDelegate;
	public delegate Settings OnInitSettingsDelegate(ServiceForCOE c);
	readonly OnInitSettingsDelegate _onInitSettingsDelegate;

	public IServiceProvider ServiceProvider { get => _serviceProvider; /* set => _serviceProvider = value; */}
	//public ILogger Logger { get => _logger;/* set => _logger = value; */}
	public Settings Settings
	{
		get
		{
			if(_settings == null)
			{
				_settings = _onInitSettingsDelegate.Invoke(this);
			}
			return _settings;
		}
		set => _settings = value;
	}
	public Configuration Configuration { get => _configuration; set => _configuration = value; }
	public HttpContext HttpContext { get => _httpContext; set => _httpContext = value; }

	public bool LoginHappenedInThisRequest = false;
	/// <summary>
	/// Used in threads orgnating outside request for example background jobs
	/// </summary>
	public bool IsBackgroundTask = false;
	public User CurrentlyLoggedInUser
	{
		get
		{
			_currentlyLoggedInUser = _onReviewCurrentlyLoggedInUserDelegate.Invoke(this, _currentlyLoggedInUser, this.LoginHappenedInThisRequest);
			return _currentlyLoggedInUser;
		}
		set => _currentlyLoggedInUser = value;
	}
	public UnitOfWork ParentUnitOfWork { get => _parentUnitOfWork; set => _parentUnitOfWork = value; }
	public bool IsAsync { get => _isAsync; set => _isAsync = value; }
	public object Tag { get => _tag; set => _tag = value; }

	public string visitorHash = "//TODO: finish this";

	public ServiceForCOE()
	{
		_logger = new Logger<ServiceForCOE>(new NullLoggerFactory());
	}

	public ServiceForCOE(IServiceProvider serviceProvider,
						 IServiceScopeFactory scopeFactory,
						 IOptions<Configuration> configuration,
						 Microsoft.Extensions.Logging.ILogger serviceForLoggerOrNullForDefault,
						 OnReviewCurrentlyLoggedInUserDelegate onLogouturrentlyLoggedInUser,
						 OnInitSettingsDelegate onInitSettings,
						 IHttpContextAccessor httpContextAccessor,
						 User currentlyLoggedInUser = null, UnitOfWork parentUnitOfWork = null, bool isAsync = true, object tag = null)
	{
		_serviceProvider = serviceProvider;
		_scopeFactory = scopeFactory;
		_logger = serviceForLoggerOrNullForDefault;
		if(_logger == null)
		{
			_logger = _serviceProvider.GetRequiredService<ILogger<ServiceForCOE>>();
			//logFilePath = CraftSynth.BuildingBlocks.Common.Misc.ApplicationPhysicalExeFilePathWithoutExtension + ".log";
		}
		_onReviewCurrentlyLoggedInUserDelegate = onLogouturrentlyLoggedInUser;
		_onInitSettingsDelegate = onInitSettings;

		//         if(_logger==null){
		//             if (_serviceProvider != null)
		//             {
		//                 throw new Exception("Developer's mistake. ILogger was not resolved by DI");
		//             }
		//             else
		//             {
		//                 _logger = new Logger<ServiceForCOE>(new NullLoggerFactory());//logs nothing but insures we don't get null ref exception
		//             }
		//}
		_configuration = configuration.Value;
		_currentlyLoggedInUser = currentlyLoggedInUser;
		_parentUnitOfWork = parentUnitOfWork;
		_httpContext = httpContextAccessor?.HttpContext;
		_isAsync = isAsync;
		_tag = tag;
	}

	public override string ToString()
	{
		string currentlyLoggedInUserData = _currentlyLoggedInUser == null ? "null" : $"{_currentlyLoggedInUser.Id} / {_currentlyLoggedInUser.EMail}";
		string r = $"ContextOfExecution: serviceProvider={(_serviceProvider == null ? "null" : "present")} configuration={(_configuration == null ? "null" : "present")}, log={(_logger == null ? "null" : "present")}, httpContext={(_httpContext == null ? "null" : "present")}, currentlyLoggedInUser={currentlyLoggedInUserData}, parentUnitOfWork={(_parentUnitOfWork == null ? "null" : "present")},isAsync={_isAsync},";
		return r;
	}

	#region _logger exposure to outside world
	public IDisposable? LogBeginScope(string logScopeName)
	{
		IDisposable? r = this._logger.BeginScope(logScopeName);
		this.Log("");
		return r;
	}

	public void Log(Exception exception,
						string message = null,
						LogLevel? logLevel = null,
						LogExtraColumnNames? extraColumn1Name = null,
						int? extraColumn1Value = null,
						LogExtraColumnNames? extraColumn2Name = null,
						int? extraColumn2Value = null
						)
	{
		this.Log(LogCategoryAndTitle.Error__Other,
			message,
			exception,
			extraColumn1Name,
			extraColumn1Value,
			extraColumn2Name,
			extraColumn2Value,
			logLevel);
	}

	public void Log(string message,
						LogExtraColumnNames? extraColumn1Name = null,
						int? extraColumn1Value = null,
						LogExtraColumnNames? extraColumn2Name = null,
						int? extraColumn2Value = null,
						LogLevel? logLevel = null
						)
	{
		this.Log(LogCategoryAndTitle.UserMessage__Other,
			message,
			null,
			extraColumn1Name,
			extraColumn1Value,
			extraColumn2Name,
			extraColumn2Value,
			logLevel);
	}

	public void Log(LogCategoryAndTitle logCategoryAndTitle,
						string message = null,
						Exception exception = null,
						LogExtraColumnNames? extraColumn1Name = null,
						int? extraColumn1Value = null,
						LogExtraColumnNames? extraColumn2Name = null,
						int? extraColumn2Value = null,
						LogLevel? logLevel = null
						)
	{
		logLevel = logLevel ?? logCategoryAndTitle.GetDefaultLogLevel() ?? LogLevel.Information;

		var logEntry = new LogEntry();
		//logEntry.Id // Id (Primary key)
		logEntry.Moment = DateTime.Now;
		logEntry.LoggedInUserId = _currentlyLoggedInUser?.Id;
		logEntry.Category = logCategoryAndTitle.ToString().GetSubstringBefore("__");
		logEntry.Title = logCategoryAndTitle.ToString().GetSubstringAfter("__");
		logEntry.Details = (message ?? logCategoryAndTitle.GetDescription()) + exception?.Message ?? "";
		logEntry.ExtraColumn1Name = extraColumn1Name?.ToString();
		logEntry.ExtraColumn1Value = extraColumn1Value;
		logEntry.ExtraColumn2Name = extraColumn2Name?.ToString();
		logEntry.ExtraColumn2Value = extraColumn2Value;
		//logEntry.VisitorHash { get; set; } // VisitorHash (length: 32)
		//logEntry.MemoryInfo { get; set; } // MemoryInfo (length: 1000)

		_logger.Log(logLevel.Value, new EventId((int)logCategoryAndTitle, logCategoryAndTitle.ToString()), logEntry, exception, (s, e) => $"{logEntry.Details}");
	}

	public string LogToString()
	{
		return _logger.ToString();
	}
	#endregion
}

//  public static class ServiceForCOEExtension
//  {
//      public static ServiceForCOE ConstructIfNull(this ServiceForCOE c)
//      {
//          if (c == null)
//          {
//              c = new ServiceForCOE();
//          }
//          return c;
//      }


//public static IServiceProvider ServiceProvider(this ServiceForCOE c)
//{
//	return c.ConstructIfNull()._serviceProvider;
//}
//public static void ServiceProvider_Set(this ServiceForCOE c, IServiceProvider newValue)
//{
//	c.ConstructIfNull()._serviceProvider = newValue;
//}
//public static void ServiceProvider_SetIfNull(this ServiceForCOE c, IServiceProvider newValue)
//{
//	if (c.ConstructIfNull()._serviceProvider == null)
//	{
//		c.ConstructIfNull()._serviceProvider = newValue;
//	}
//}

//public static Configuration Configuration(this ServiceForCOE c)
//{
//	return c.ConstructIfNull()._configuration;
//}
//public static void Configuration_Set(this ServiceForCOE c, Configuration newValue)
//{
//	c.ConstructIfNull()._configuration = newValue;
//}
//public static void Configuration_SetIfNull(this ServiceForCOE c, Configuration newValue)
//{
//	if (c.ConstructIfNull()._configuration == null)
//	{
//		c.ConstructIfNull()._configuration = newValue;
//	}
//}

//public static ILogger Log(this ServiceForCOE c)
//      {
//          return c.ConstructIfNull()._log;
//      }
//      public static void Log_Set(this ServiceForCOE c, ILogger newValue)
//      {
//          c.ConstructIfNull()._log = newValue;
//      }
//      public static void Log_SetIfNull(this ServiceForCOE c, ILogger newValue)
//      {
//          if (c.ConstructIfNull()._log == null)
//          {
//              c.ConstructIfNull()._log = newValue;
//          }
//      }

//public static Settings Settings(this ServiceForCOE c)
//{
//	return c.ConstructIfNull()._settings;
//}
//public static void Log_Set(this ServiceForCOE c, Settings newValue)
//{
//	c.ConstructIfNull()._settings = newValue;
//}
//public static void Settings_SetIfNull(this ServiceForCOE c, Settings newValue)
//{
//	if (c.ConstructIfNull()._settings == null)
//	{
//		c.ConstructIfNull()._settings = newValue;
//	}
//}

//public static User CurrentlyLoggedInUser(this ServiceForCOE c)
//      {
//          return c.ConstructIfNull()._currentlyLoggedInUser;
//      }
//      public static void CurrentlyLoggedInUser_Set(this ServiceForCOE c, User newValue)
//      {
//          c.ConstructIfNull()._currentlyLoggedInUser = newValue;
//      }
//      public static void CurrentlyLoggedInUser_SetIfNull(this ServiceForCOE c, User newValue)
//      {
//          if (c.ConstructIfNull()._currentlyLoggedInUser == null)
//          {
//              c.ConstructIfNull()._currentlyLoggedInUser = newValue;
//          }
//      }

//      public static UnitOfWork ParentUnitOfWork(this ServiceForCOE c)
//      {
//          return c.ConstructIfNull()._parentUnitOfWork;
//      }
//      public static void ParentUnitOfWork_Set(this ServiceForCOE c, UnitOfWork newValue)
//      {
//          c.ConstructIfNull()._parentUnitOfWork = newValue;
//      }
//      public static void ParentUnitOfWork_SetIfNull(this ServiceForCOE c, UnitOfWork newValue)
//      {
//          if (c.ConstructIfNull()._parentUnitOfWork == null)
//          {
//              c.ConstructIfNull()._parentUnitOfWork = newValue;
//          }
//      }

//      public static Microsoft.AspNetCore.Http.HttpContext HttpContext(this ServiceForCOE c)
//      {
//          return c.ConstructIfNull()._httpContext;
//      }
//      public static void HttpContext_Set(this ServiceForCOE c, Microsoft.AspNetCore.Http.HttpContext newValue)
//      {
//          c.ConstructIfNull()._httpContext = newValue;
//      }
//      public static void HttpContext_SetIfNull(this ServiceForCOE c, Microsoft.AspNetCore.Http.HttpContext newValue)
//      {
//          if (c.ConstructIfNull()._httpContext == null)
//          {
//              c.ConstructIfNull()._httpContext = newValue;
//          }
//      }

//public static bool IsAsync(this ServiceForCOE c)
//{
//	return c.ConstructIfNull()._isAsync;
//}
//public static void IsAsync_Set(this ServiceForCOE c, bool newValue)
//{
//	c.ConstructIfNull()._isAsync = newValue;
//}

//public static object Tag(this ContextOfExecution c)
//      {
//          return c.ConstructIfNull()._tag;
//      }
//      public static void Tag_Set(this ContextOfExecution c, object newValue)
//      {
//          c.ConstructIfNull()._tag = newValue;
//      }
//      public static void Tag_SetIfNull(this ContextOfExecution c, object newValue)
//      {
//          if (c.ConstructIfNull()._tag == null)
//          {
//              c.ConstructIfNull()._tag = newValue;
//          }
//      }
//  }
