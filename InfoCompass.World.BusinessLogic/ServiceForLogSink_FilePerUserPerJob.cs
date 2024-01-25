
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;

namespace InfoCompass.World.BusinessLogic;

public class ServiceForLogSink_FilePerUserPerJob:ILogEventSink, IDisposable
{
	private readonly IServiceScopeFactory _scopeFactory;
	Settings _settings;
	Configuration _configuration;
	public Settings Settings
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
	public Configuration Configuration { get => _configuration; set => _configuration = value; }


	private readonly BlockingCollection<LogEvent> _logEventQueue = new BlockingCollection<LogEvent>();
	private readonly Task _processingTask;

	public ServiceForLogSink_FilePerUserPerJob(IServiceScopeFactory scopeFactory, IOptions<Configuration> configuration)
	{
		_scopeFactory = scopeFactory;
		_configuration = configuration.Value;
		//_onInitSettingsDelegate = onInitSettings;

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
				Console.Out.WriteLine($"MyLogSink throw exception wile processing log queue. {e.Message} Stack trace: {e.StackTrace}");
			}
		}
	}

	private List<string> _previousLogScopes = new List<string>();
	private async Task ProcessLogEventAsync(LogEvent logEvent)
	{
		if((int)logEvent.Level >= (int)this.Settings.LogLevelForLogSink_FilePerUserPerJob)
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
					if(_previousLogScopes.Count - 1 >= i && _previousLogScopes[i] == currentLogScopes[i])
					{
						logScopesCsvToShow = logScopesCsvToShow + "  ";//this produces identation
					}
					else
					{
						logScopesCsvToShow = logScopesCsvToShow + ((logScopesCsvToShow == "" || logScopesCsvToShow.EndsWith(' ')) ? "" : ">") + logScope;
					}
					i++;
				}
				_previousLogScopes = currentLogScopes;
			}
			catch { }

			string actionId = GetPropertyValue<string?>(logEvent, "ActionId");
			string actionName = GetPropertyValue<string?>(logEvent, "ActionName");
			string requestId = GetPropertyValue<string?>(logEvent, "RequestId");
			string requestPath = GetPropertyValue<string?>(logEvent, "RequestPath");
			string connectionId = GetPropertyValue<string?>(logEvent, "ConnectionId");

			string logMessage = logEvent.RenderMessage();

			Exception? deepestException = logEvent.Exception == null ? null : CraftSynth.BuildingBlocks.Common.Misc.GetDeepestException(logEvent.Exception);
			string? exceptionsAsString = logEvent.Exception == null ? null : CraftSynth.BuildingBlocks.Common.Misc.GetInnerExceptionsAsSingleString(logEvent.Exception);


			string message = "\r\n";
			message = message.AppendIfValueToCheckIsNotNull("", logTimestamp);
			message = message.AppendIfValueToCheckIsNotNull(" | LogLevel=", logLevel?.PadRight(Enum.GetNames(typeof(LogLevel)).Max(name => name.Length)));
			message = message.AppendIfValueToCheckIsNotNull(" | UserId=", userId?.ToString()?.PadRight(5));
			message = message.AppendIfValueToCheckIsNotNull(" | JobId=", jobId?.ToString()?.PadRight(5));
			message = message.AppendIfValueToCheckIsNotNull(" | Category=", logCategory?.PadRight(Enum.GetNames(typeof(LogCategoryAndTitle)).Max(name => name.GetSubstringBefore("__").Length)));
			message = message.AppendIfValueToCheckIsNotNull(" | Title=", logTitle?.PadRight(Enum.GetNames(typeof(LogCategoryAndTitle)).Max(name => name.GetSubstringAfter("__").Length)));
			//message = message.AppendIfValueToCheckIsNotNull(" | SourceContext=", logSourceContext);
			message = message.AppendIfValueToCheckIsNotNull(" | ", logScopesCsvToShow);
			message = message.AppendIfValueToCheckIsNotNullOrWhiteSpace((message.EndsWith(' ') ? "" : ">"), logMessage);
			message = message.AppendIfValueToCheckIsNotNull(">Exception=", deepestException?.Message);
			message = message.AppendIfValueToCheckIsNotNull(">ExceptionTree=", exceptionsAsString);

			message = message.Replace("\\r\\n", "\r\n");

			await Console.Out.WriteLineAsync(message);
			await Console.Out.FlushAsync();

			string filePath = FileSystem.InsureProperDirectorySeparatorChar(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath + Path.PathSeparator + "log.txt");
			if(userId != null && jobId != null)
			{
				filePath = CraftSynth.BuildingBlocks.IO.FileSystem.PathCombine(
							Settings.DataFolderPathAbsolute(_configuration.DataFolderPathAbsolute),
							"user_" + userId.ToString(),
							"job_" + jobId,
							"log.txt"//job.BookSettings.SuggestedFileName
							);
			}
			else if(userId != null && jobId == null)
			{
				filePath = CraftSynth.BuildingBlocks.IO.FileSystem.PathCombine(
							Settings.DataFolderPathAbsolute(_configuration.DataFolderPathAbsolute),
							"user_" + userId.ToString(),
							"log.txt"//job.BookSettings.SuggestedFileName
							);
			}
			else if(userId == null && jobId == null)
			{
				filePath = CraftSynth.BuildingBlocks.IO.FileSystem.PathCombine(
							Settings.DataFolderPathAbsolute(_configuration.DataFolderPathAbsolute),
							"log.txt"//job.BookSettings.SuggestedFileName
							);
			}
			FileSystem.CreateFolderIfItDoesNotExist(Path.GetDirectoryName(filePath));
			await File.AppendAllTextAsync(filePath, message);
		}
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
				r = (T?)Convert.ChangeType(userIdPropertyStructuredValue.Properties[0].Value.ToString().Trim('"'), typeof(T?));
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
