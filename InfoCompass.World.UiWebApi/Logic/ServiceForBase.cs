using System.Text.Json;
using MyCompany.World.UiWebApi.Models;
using Error = MyCompany.World.Common.Entities.Error;
using Errors = MyCompany.World.Common.Entities.Errors;

namespace MyCompany.World.UiWebApi.Logic;

public interface IServiceForBase
{
	public bool CurrentlyLoggedInUserIsAdmin();
	public Task<ServerResponseForUI<T>> HandleExceptionOnThisLayer<T>(Exception e);
	public string GetCurrentRequestUriRoot();
}

public class ServiceForBase:IServiceForBase
{
	private ServiceForCOE _c;
	private MyCompany.World.BusinessLogic.IServiceForLogs _serviceForLog;
	public ServiceForBase(ServiceForCOE c, MyCompany.World.BusinessLogic.IServiceForLogs serviceForLogs)
	{
		_c = c;
		_serviceForLog = serviceForLogs;
	}

	public bool CurrentlyLoggedInUserIsAdmin()
	{
		throw new NotImplementedException();
	}

	protected string GetCurrentRequestUri()
	{
		string r = null;

		if(_c.HttpContext != null)
		{
			var uri = new Uri($"{_c.HttpContext.Request.Scheme}://{_c.HttpContext.Request.Host}{_c.HttpContext.Request.Path}{_c.HttpContext.Request.QueryString}");
			r = uri.AbsoluteUri;
		}

		return r;
	}

	public string GetCurrentRequestUriRoot()
	{
		string r = null;

		if(_c.HttpContext != null)
		{
			var uri = new Uri($"{_c.HttpContext.Request.Scheme}://{_c.HttpContext.Request.Host}");
			r = uri.AbsoluteUri;
		}

		return r;
	}

	public async Task<ServerResponseForUI<T>> HandleExceptionOnThisLayer<T>(Exception e)
	{
		ServerResponseForUI<T> r;
		try
		{
			Error errorThatHoldsFriendlyLogEntries = null;
			LogEntries? friendlyLogEntries = _serviceForLog.ExtractUserFriendlyLogs();
			if(friendlyLogEntries.Items.Count == 0)
			{
				friendlyLogEntries = null;
			}
			else
			{
				errorThatHoldsFriendlyLogEntries = new Error(friendlyLogEntries.Items.Select(l => l.ToString()).ToSingleString("", "", "\r\n"));
			}

			if(e is Error)
			{
				if((e as Error).SkipLoggingIntoDbAndMail == false)
				{
					long errorLogId = await _serviceForLog.Write(e, _c.CurrentlyLoggedInUser, null, null, null, null, null);
					(e as Error).LogId = errorLogId;
					(e as Error).FriendlyMessage = (e as Error).FriendlyMessage ?? $"Error occured during operation. Please try again or send us this error ID: {errorLogId}.";
				}

				r = new ServerResponseForUI<T>(
						_c,
							false,
							(e as Error).FriendlyMessage ?? (e as Error).Message,
							(e as Error).LogId,
							default(T),
							new Errors() { errorThatHoldsFriendlyLogEntries },
							null,
							(e as Error).HttpStatusCode ?? System.Net.HttpStatusCode.InternalServerError
						);
			}
			else
			{
				long errorLogId = await _serviceForLog.Write(e, _c.CurrentlyLoggedInUser, null, null, null, null, null);
				string details = $"Error occured during operation. Please try again or send us this error ID: {errorLogId}.";

				r = new ServerResponseForUI<T>(
						_c,
							false,
							"Error occured during operation execution. Details:" + details,
							errorLogId,
							default(T),
							new Errors() { errorThatHoldsFriendlyLogEntries },
							null,
							System.Net.HttpStatusCode.InternalServerError
						);
			}

			string rAsJson = JsonSerializer.Serialize(r, typeof(ServerResponseForUI<T>), new JsonSerializerOptions() { WriteIndented = false });
			_c.Log(rAsJson);
		}
		catch(Exception ex)
		{
			r = new ServerResponseForUI<T>(
							_c,
								false,
								"Error occured during operation execution. Details:" + e.Message + " (Also handling of error failed. Details:" + ex.Message + ")",
								null,
								default(T),
								null,
								null,
								System.Net.HttpStatusCode.InternalServerError
							);
		}

		return r;
	}

	//     private static long WriteLog(Exception e, string details, User currentlyLoggedInUserOrNull, LogExtraColumnNames? logExtraColumnName, int? logExtraColumnValue, LogExtraColumnNames? logExtraColumn2Name, int? logExtraColumn2Value, ContextOfExecution c)
	//     {
	//         long errorLogId = -1;

	//         try
	//         {
	//	_serviceForLog.Write(e, currentlyLoggedInUserOrNull, details, logExtraColumnName, logExtraColumnValue, logExtraColumn2Name, logExtraColumn2Value, c);
	//}
	//catch (Exception ee)
	//         {
	//	_serviceForLog.Write(e, null, details.ToNonNullString(), null, null);
	//}

	//return errorLogId;
	//     }
}
