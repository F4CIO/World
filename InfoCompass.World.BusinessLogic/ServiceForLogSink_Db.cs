
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;

namespace MyCompany.World.BusinessLogic;

public class ServiceForLogSink_Db:ILogEventSink, IDisposable
{
	private readonly IServiceScopeFactory _scopeFactory;
	Settings _settings;
	Configuration _configuration;
	private IServiceForLogs _serviceForLogs;
	private IServiceForLogs ServiceForLogs
	{
		get
		{
			if(_serviceForLogs == null)
			{
				using IServiceScope scope = _scopeFactory.CreateScope();  // This will use the current scope if available
				_serviceForLogs = scope.ServiceProvider.GetRequiredService<IServiceForLogs>();
			}
			return _serviceForLogs;
		}
		set => _serviceForLogs = value;
	}

	private Settings Settings
	{
		get
		{
			if(_settings == null)
			{
				using IServiceScope scope = _scopeFactory.CreateScope();  // This will use the current scope if available
				IServiceForSettings settingsService = scope.ServiceProvider.GetRequiredService<IServiceForSettings>();
				_settings = settingsService.GetCachedOrFromDbForUserDefault().Result;
			}
			return _settings;
		}
		set => _settings = value;
	}
	private Configuration Configuration { get => _configuration; set => _configuration = value; }


	private readonly BlockingCollection<LogEvent> _logEventQueue = new BlockingCollection<LogEvent>();
	private readonly Task _processingTask;

	public ServiceForLogSink_Db(IServiceScopeFactory scopeFactory, IOptions<Configuration> configuration)
	{
		_scopeFactory = scopeFactory;
		_configuration = configuration.Value;

		// Start a long-running task to process log events
		_processingTask = Task.Factory.StartNew(
			ProcessLogQueue,
			TaskCreationOptions.LongRunning);
	}

	public void Emit(LogEvent logEvent)
	{
		// Post the log event to the queue
		_logEventQueue.Add(logEvent);
	}

	private void ProcessLogQueue()
	{
		foreach(LogEvent logEvent in _logEventQueue.GetConsumingEnumerable())
		{
			try
			{
				this.ProcessLogEventAsync(logEvent).Wait();
			}
			catch(Exception e)
			{
				Console.Out.WriteLine($"ServiceForLogSink_Db throw exception wile processing log queue. {e.Message} Stack trace: {e.StackTrace}");
			}
		}
	}

	private async Task ProcessLogEventAsync(LogEvent logEvent)
	{
		long? logId = null;

		if((int)logEvent.Level >= (int)this.Settings.LogLevelForLogSink_Db)
		{
			string logTimestamp = DateTime.Now.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS();

			string logLevel = logEvent.Level.ToString();

			long? userId = GetPropertyValue<long?>(logEvent, "UserId");

			long? jobId = GetPropertyValue<long?>(logEvent, "JobId");

			string? logCategory = GetPropertyStructuredValue<string?>(logEvent, "EventId")?.GetSubstringBefore("__");

			string? logTitle = GetPropertyStructuredValue<string?>(logEvent, "EventId")?.GetSubstringAfter("__");

			string? logSourceContext = GetPropertyValue<string?>(logEvent, "SourceContext");

			string logScopesCsvToShow = "";
			try
			{
				var currentLogScopes = (List<string>)(logEvent.Properties["Scope"] as Serilog.Events.SequenceValue).Elements.Select(el => (el as ScalarValue)?.Value?.ToString()).ToList();
				int i = 0;
				foreach(string logScope in currentLogScopes)
				{
					logScopesCsvToShow = logScopesCsvToShow + ((logScopesCsvToShow == "" || logScopesCsvToShow.EndsWith(' ')) ? "" : ">") + logScope;
					i++;
				}
			}
			catch { }

			string logMessage = logEvent.RenderMessage();

			Exception? deepestException = logEvent.Exception == null ? null : CraftSynth.BuildingBlocks.Common.Misc.GetDeepestException(logEvent.Exception);
			string? exceptionsAsString = logEvent.Exception == null ? null : CraftSynth.BuildingBlocks.Common.Misc.GetInnerExceptionsAsSingleString(logEvent.Exception);

			string message = "";
			//message = message.AppendIfValueToCheckIsNotNull("", logTimestamp);
			//message = message.AppendIfValueToCheckIsNotNull(" | LogLevel=", logLevel);
			//message = message.AppendIfValueToCheckIsNotNull(" | UserId=", userId);
			//message = message.AppendIfValueToCheckIsNotNull(" | JobId=", jobId);
			//message = message.AppendIfValueToCheckIsNotNull(" | Category=", logCategory);
			//message = message.AppendIfValueToCheckIsNotNull(" | Title=", logTitle);
			//message = message.AppendIfValueToCheckIsNotNull(" | SourceContext=", logSourceContext);
			message = message.AppendIfValueToCheckIsNotNull("", logScopesCsvToShow);
			message = message.AppendIfValueToCheckIsNotNullOrWhiteSpace((message.EndsWith(' ') ? "" : ">"), logMessage);
			message = message.AppendIfValueToCheckIsNotNull(">Exception=", deepestException?.Message);
			message = message.AppendIfValueToCheckIsNotNull(">ExceptionsTree=", exceptionsAsString);

			message = message.Replace("\\r\\n", "\r\n");

			if(userId != null && jobId != null)
			{
				logId = await ServiceForLogs.Write(category: logCategory, title: logTitle, logLevel: (int)logEvent.Level, currentlyLoggedInUserId: userId, details: message, LogExtraColumnNames.UserId, userId, LogExtraColumnNames.JobId, jobId);
			}
			else
			{
				logId = await ServiceForLogs.Write(category: logCategory, title: logTitle, logLevel: (int)logEvent.Level, currentlyLoggedInUserId: userId, details: message, null, null, null, null);
			}
		}

		//here i want to return logId to caller
	}
	private static T GetPropertyValue<T>(LogEvent logEvent, string propertyName)
	{
		var r = default(T);

		try
		{
			if(logEvent.Properties.TryGetValue(propertyName, out LogEventPropertyValue userIdProperty) && userIdProperty is ScalarValue userIdPropertyScalarValue)
			{
				r = (T?)userIdPropertyScalarValue.Value;
			}
		}
		catch { }

		return r;
	}

	private static T GetPropertyStructuredValue<T>(LogEvent logEvent, string propertyName)
	{
		var r = default(T);

		try
		{
			if(logEvent.Properties.TryGetValue(propertyName, out LogEventPropertyValue userIdProperty) && userIdProperty is StructureValue userIdPropertyStructuredValue)
			{
				r = (T?)Convert.ChangeType(userIdPropertyStructuredValue.Properties.Where(p => p.Name == "Name").First().Value.ToString().Trim('"'), typeof(T?));
			}
		}
		catch { }

		return r;
	}

	public void Dispose()
	{
		_logEventQueue.CompleteAdding();
		_processingTask.Wait();
		_logEventQueue.Dispose();
	}
}
