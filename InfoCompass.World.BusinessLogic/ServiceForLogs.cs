using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace MyCompany.World.BusinessLogic;


public interface IServiceForLogs //: GenericHandlerInterface<YouTrend.Common.Entities.Log>
{
	Task Create();

	Task Write(LogEntry log);

	Task<long> Write(string category, string title, int logLevel, long? currentlyLoggedInUserId, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null);

	Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, long? currentlyLoggedInUserId, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null);

	Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, User currentlyLoggedInUserOrNUll, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null);

	Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, string mojoEMailOrNUll, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null);

	Task<long> Write(Exception exception, string currentlyLoggedInUserEMailOrNull, string details = null);

	Task<long> Write(Exception exception, string currentlyLoggedInUserEMailOrNull, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null);

	Task<long> Write(Exception exception, User currentlyLoggedInUserOrNull, string details, LogExtraColumnNames? logExtraColumnName, long? logExtraColumnValue, LogExtraColumnNames? logExtraColumn2Name, long? logExtraColumn2Value, string visitorHash = null);

	//new Task<LogEntry> GetById(long id);
	Task<LogEntry> GetById(long id);

	string GetUserFriendlyTitleIfExists(string mergedLogTitle);

	LogEntries ExtractUserFriendlyLogsIfAnyOtherwiseReturnAll();
	LogEntries ExtractUserFriendlyLogs();
}

public class ServiceForLogs: /*GenericHandler<YouTrend.Common.Entities.Log>,*/ IServiceForLogs
{
	ServiceForCOE _c;
	World.DataAccessContracts.IServiceForLogs _serviceForLogs;
	World.DataAccessContracts.IServiceForUsers _serviceForUsers;

	public ServiceForLogs(ServiceForCOE c, World.DataAccessContracts.IServiceForLogs serviceForLogs, World.DataAccessContracts.IServiceForUsers serviceForUsers)
	{
		_c = c;
		_serviceForLogs = serviceForLogs;
		_serviceForUsers = serviceForUsers;
	}
	public async Task Create()
	{
		await _serviceForLogs.Create();
	}



	public async Task Write(LogEntry log)
	{
		if
		(
			(
				_c.Settings.LogEventsMailNotifications_LogCategoryAndTitleForWhichToMailNotifications.Any(lct => lct == "*")
				||
				_c.Settings.LogEventsMailNotifications_LogCategoryAndTitleForWhichToMailNotifications.Any(lct => lct.ToLower() == log.LogCategoryAndTitleAsString.ToLower())
			)
			&&
			(
				!_c.Settings.LogEventsMailNotifications_LogCategoryAndTitleForWhichNotToMailNotifications.Exists(lct => lct == "*")
				&&
				!_c.Settings.LogEventsMailNotifications_LogCategoryAndTitleForWhichNotToMailNotifications.Exists(lct => lct.ToLower() == log.LogCategoryAndTitleAsString.ToLower())
		)
		)
		{

			foreach(string receiverEMail in _c.Settings.LogEventsMailNotifications_Receivers)
			{
				string log_LoggedInUserEMail = null;
				try
				{
					if(log.LoggedInUserId != null)
					{
						User log_loggedInUser = await _serviceForUsers.GetById<User>(log.LoggedInUserId.Value);
						if(log_loggedInUser == null)
						{
							log_LoggedInUserEMail = "User was not found in db by Id";
						}
						else
						{
							log_LoggedInUserEMail = log_loggedInUser.EMail.ToNonNullString("Users email property is null");
						}
					}
				}
				catch(Exception ex) { }

				_c.Log($"Preparing LogEventsMailNotifications email for {receiverEMail}...");
				var mailMessage = new MailMessage(new MailAddress(_c.Settings.MailServerFromEMail, _c.Settings.MailServerFromName), new MailAddress(receiverEMail));
				mailMessage.IsBodyHtml = false;
				mailMessage.Subject = _c.Settings.InstanceName + ": " + (log?.LogCategoryAndTitleOrNullWhereCanNotMatch?.GetDescription()?.ToNonNullNonEmptyString(log.LogCategoryAndTitleAsString) ?? log.LogCategoryAndTitleAsString);
				string link = _c.Settings.GuiUrl;
				mailMessage.Body = log.ToString("\r\n", log_LoggedInUserEMail);

				if(_c.Settings.LogEventsMailNotifications_MailBodyPhrasesForWhichNotToMailNotifications.Any(p => mailMessage.Body.Contains(p)))
				{
					log.ToString("Mail sending aborted due to LogEventsMailNotifications_CsvOfMailBodyPhrasesForWhichNotToMailNotifications");
				}
				else
				{
					_c.Log($"Sending email to {receiverEMail}...");
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
					_c.Log($"Mail sent successfully to {receiverEMail}.");
				}
			}
		}

		using(var uow = UnitOfWork.UseParentIfExistOrNew(_c.ParentUnitOfWork, _serviceForLogs.CreateUnitOfWork().Result))
		{
			_c.ParentUnitOfWork = uow;
			await _serviceForLogs.Insert(log);
			//base.Insert(ref log, c);
		}
	}

	static MemoryInfo _lastMemoryInfo = null;

	public async Task<long> Write(string category, string title, int logLevel, long? currentlyLoggedInUserId, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		var log = new LogEntry();
		log.Moment = DateTime.Now;
		//log.Id = long.Parse(log.Moment.ToDateAndTimeAsYYYYMMDDHHMMSS());
		log.Level = logLevel;
		log.LoggedInUserId = currentlyLoggedInUserId;
		log.VisitorHash = "visitorHash1";
		log.Category = category;
		log.Title = title.ToNonNullString().FirstXChars(250, "...");
		log.Details = details.FirstXChars(10000, "[cut by ServiceForLogs]");
		log.ExtraColumn1Name = logExtraColumnName?.ToString();
		log.ExtraColumn1Value = logExtraColumnValue;
		log.ExtraColumn2Name = logExtraColumn2Name?.ToString();
		log.ExtraColumn2Value = logExtraColumn2Value;

		try
		{

			var memoryInfo = new MemoryInfo(_lastMemoryInfo, "[" + CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath.ToNonNullString() + "]");
			log.MemoryInfo = memoryInfo.ToString("\r\n");
			_lastMemoryInfo = memoryInfo;
		}
		catch(Exception e)
		{
			log.MemoryInfo = null;
		}

		await this.Write(log);
		return log.Id;
	}

	public async Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, long? currentlyLoggedInUserId, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		long logId = await this.Write(
				LogEntry.SeparateLogCategoryAndLogTitle(logCategoryAndTitle.ToString()).Key,
				LogEntry.SeparateLogCategoryAndLogTitle(logCategoryAndTitle.ToString()).Value,
				(int)logCategoryAndTitle.GetDefaultLogLevel(),
				currentlyLoggedInUserId,
				details,
				logExtraColumnName,
				logExtraColumnValue,
				logExtraColumn2Name,
				logExtraColumn2Value);
		return logId;
	}

	public async Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, User currentlyLoggedInUserOrNUll, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		long logId = await this.Write(logCategoryAndTitle, currentlyLoggedInUserOrNUll == null ? (long?)null : (long)currentlyLoggedInUserOrNUll.Id, details, logExtraColumnName, logExtraColumnValue, logExtraColumn2Name, logExtraColumn2Value);
		return logId;
	}

	public async Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, string mojoEMailOrNUll, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		User currentlyLoggedInUser = (await _serviceForUsers.Get<User>(a => string.Compare(a.EMail, mojoEMailOrNUll, StringComparison.OrdinalIgnoreCase) == 0)).SingleOrDefault();
		long logId = await this.Write(logCategoryAndTitle, currentlyLoggedInUser, details, logExtraColumnName, logExtraColumnValue, logExtraColumn2Name, logExtraColumn2Value);
		return logId;
	}

	public async Task<long> Write(Exception exception, string currentlyLoggedInUserEMailOrNull, string details = null)
	{
		long logId = await this.Write(exception, currentlyLoggedInUserEMailOrNull, details, null, null, null, null);
		return logId;
	}

	public async Task<long> Write(Exception exception, string currentlyLoggedInUserEMailOrNull, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		User currentlyLoggedInUser = (await _serviceForUsers.Get<User>(a => string.Compare(a.EMail, currentlyLoggedInUserEMailOrNull, StringComparison.OrdinalIgnoreCase) == 0)).SingleOrDefault();
		long logId = await this.Write(exception, currentlyLoggedInUser, details, logExtraColumnName, logExtraColumnValue, logExtraColumn2Name, logExtraColumn2Value);
		return logId;
	}

	public async Task<long> Write(Exception exception, User currentlyLoggedInUserOrNull, string details, LogExtraColumnNames? logExtraColumnName, long? logExtraColumnValue, LogExtraColumnNames? logExtraColumn2Name, long? logExtraColumn2Value, string visitorHash = null)
	{
		Exception deepestException = CraftSynth.BuildingBlocks.Common.Misc.GetDeepestException(exception);
		string exceptionsAsString = CraftSynth.BuildingBlocks.Common.Misc.GetInnerExceptionsAsSingleString(exception);
		long logId = await this.Write(
			"Error",
			deepestException.Message,
			(int)LogLevel.Error,
			currentlyLoggedInUserOrNull?.Id,
			details.ToNonNullString().AppendIfNotNullOrWhiteSpace("\r\n") + exceptionsAsString,
			logExtraColumnName,
			logExtraColumnValue,
			logExtraColumn2Name,
			logExtraColumn2Value);
		return logId;
	}

	////example of method that is just passing call to lower layer
	//public new async Task<LogEntry> GetById(long id)
	//{
	//	throw new NotImplementedException();
	//	//return base.GetById(id, c);
	//}

	public async Task<LogEntry> GetById(long id)
	{
		return await _serviceForLogs.GetById<LogEntry>(id);
	}

	//public List<SP_GetMergedLogReturnModel> GetMergedLog(long? userId, DateTime? momentFrom, DateTime? momentTo)
	//{
	//    return YouTrend.DataRepository.HandlersFactory.HandlerForLog.GetMergedLog(userId, momentFrom, momentTo, c);
	//}

	public string GetUserFriendlyTitleIfExists(string mergedLogTitle)
	{
		string r = mergedLogTitle;

		if(r == mergedLogTitle)
		{
			LogCategoryAndTitle? logTitle = CraftSynth.BuildingBlocks.Common.ExtenderClass.GetEnumFromString<LogCategoryAndTitle>("UserAction__" + mergedLogTitle.Replace(' ', '_'));
			if(logTitle != null)
			{
				string d = CraftSynth.BuildingBlocks.Common.ExtenderClass.Description(logTitle);
				if(!d.IsNullOrWhiteSpace())
				{
					r = d;
				}
			}
		}

		return r;
	}

	//public List<Log> Search(LogFilter filter)
	//{
	//    return YouTrend.DataRepository.HandlersFactory.HandlerForLog.Search(filter, c);
	//}

	//public long WriteToQueueEventLog(long? userId, long ticketId, string action, long value, string comment)
	//{        
	//    var log = new QueueEventLog();
	//    log.LogTimestamp = YouTrend.Common.Helper.GetPacificTime();
	//    log.Id = userId;
	//    log.TicketId = ticketId.ToString();
	//    log.Action = action;
	//    log.Value = value.ToString();
	//    log.Comment = comment;   

	//    using (var uow = UnitOfWork.UseParentIfExistOrNew(c, base.CreateUnitOfWork()))
	//    {
	//        YouTrend.DataRepository.HandlersFactory.HandlerForQueueEventLog.Insert(ref log, uow);
	//    }

	//    return log.Id;
	//}



	/// <summary>
	/// If c.Log() has entries of level UserFriendly returns all such entries in list; if not returns all log lines as entries in list.
	/// </summary>
	/// <param name="c"></param>
	/// <returns></returns>
	public LogEntries ExtractUserFriendlyLogsIfAnyOtherwiseReturnAll()
	{
		var r = new LogEntries();
		//if (c.Logger.Tag is LogEntries)
		//{
		//    r = (c.Logger.Tag as LogEntries);
		//    if (r.Items.Any(item => item.Level == (long)LogLevel.Information))
		//    {
		//        r = new LogEntries();
		//        r.Items = (c.Logger.Tag as LogEntries).Items.Where(item => item.Level == (long)LogLevel.Information).ToList();
		//    }
		//}
		//else
		{
			r = new LogEntries();
			foreach(string line in _c.LogToString().ToLines(true, true, new List<string>()))
			{
				r.Items.Add(new LogEntry() { Details = line, Level = (int)LogLevel.Information });
			}
		}

		foreach(LogEntry item in r.Items)
		{
			//remove timestamp and special chars
			item.Details = item.Details.Substring("2022.11.11 12:04:37 (local)           ".Length).Replace("\r", "").Replace("\n", "").Replace("\t", "   ");
		}
		return r;
	}

	/// <summary>
	/// If c.Log() has entries of level UserFriendly returns all such entries in list; if not returns all log lines as entries in list.
	/// </summary>
	/// <param name="c"></param>
	/// <returns></returns>
	public LogEntries ExtractUserFriendlyLogs()
	{
		var r = new LogEntries();
		//if (c.Log().Tag is LogEntries)
		//{
		//    r = (c.Log().Tag as LogEntries);
		//    if (r.Items.Any(item => item.Level == (long)LogLevel.Information))
		//    {
		//        r = new LogEntries();
		//        r.Items = (c.Log().Tag as LogEntries).Items.Where(item => item.Level == (long)LogLevel.Information).ToList();
		//    }
		//    else
		//    {
		//        r = new LogEntries();
		//        r.Items = new List<LogEntry>();
		//    }
		//}

		int minSpacesCount = int.MaxValue;
		foreach(LogEntry item in r.Items)
		{
			//remove timestamp and special chars
			item.Details = item.Details.GetSubstringAfter(")").Replace("\r", "").Replace("\n", "").Replace("\t", "   ");

			int spacesCount = item.Details.Length - item.Details.TrimStart().Length;
			if(spacesCount < minSpacesCount)
			{
				minSpacesCount = spacesCount;
			}
		}

		//remove empty spaces but preserve if any identation
		if(minSpacesCount > 0 && minSpacesCount < long.MaxValue)
		{
			foreach(LogEntry item in r.Items)
			{
				item.Details = item.Details.Substring(minSpacesCount);
			}
		}
		return r;
	}
}

public class MockedServiceForLogs: /*GenericHandler<YouTrend.Common.Entities.Log>,*/ IServiceForLogs
{
	public Task Create()
	{
		throw new NotImplementedException();
	}

	public LogEntries ExtractUserFriendlyLogs()
	{
		throw new NotImplementedException();
	}

	public LogEntries ExtractUserFriendlyLogsIfAnyOtherwiseReturnAll()
	{
		throw new NotImplementedException();
	}

	public Task<LogEntry> GetById(long id)
	{
		throw new NotImplementedException();
	}

	public string GetUserFriendlyTitleIfExists(string mergedLogTitle)
	{
		throw new NotImplementedException();
	}

	public Task Write(LogEntry log)
	{
		throw new NotImplementedException();
	}

	public Task<long> Write(string category, string title, int logLevel, long? currentlyLoggedInUserId, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		throw new NotImplementedException();
	}

	public Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, long? currentlyLoggedInUserId, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		throw new NotImplementedException();
	}

	public Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, User currentlyLoggedInUserOrNUll, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		throw new NotImplementedException();
	}

	public Task<long> Write(LogCategoryAndTitle logCategoryAndTitle, string mojoEMailOrNUll, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		throw new NotImplementedException();
	}

	public Task<long> Write(Exception exception, string currentlyLoggedInUserEMailOrNull, string details = null)
	{
		throw new NotImplementedException();
	}

	public Task<long> Write(Exception exception, string currentlyLoggedInUserEMailOrNull, string details = null, LogExtraColumnNames? logExtraColumnName = null, long? logExtraColumnValue = null, LogExtraColumnNames? logExtraColumn2Name = null, long? logExtraColumn2Value = null)
	{
		throw new NotImplementedException();
	}

	public Task<long> Write(Exception exception, User currentlyLoggedInUserOrNull, string details, LogExtraColumnNames? logExtraColumnName, long? logExtraColumnValue, LogExtraColumnNames? logExtraColumn2Name, long? logExtraColumn2Value, string visitorHash = null)
	{
		throw new NotImplementedException();
	}
}