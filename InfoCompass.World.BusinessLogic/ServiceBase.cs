using Error = MyCompany.World.Common.Entities.Error;
using Errors = MyCompany.World.Common.Entities.Errors;

namespace MyCompany.World.BusinessLogic;

public class ServiceBase
{
	protected readonly ServiceForCOE _c;
	protected readonly MyCompany.World.BusinessLogic.IServiceForLogs _serviceForLogs;
	protected readonly MyCompany.World.DataAccessContracts.IServiceForUsers _serviceForUsers;

	public ServiceBase(ServiceForCOE c, IServiceForLogs serviceForLogs, MyCompany.World.DataAccessContracts.IServiceForUsers serviceForUsers, long? userIdForBackgroundTask = null)
	{
		_c = c;
		_serviceForUsers = serviceForUsers;
		_serviceForLogs = serviceForLogs;

		if(userIdForBackgroundTask != null)
		{
			_c.IsBackgroundTask = true;
			User user = _serviceForUsers.GetById<User>(userIdForBackgroundTask.Value).Result;
			_c.CurrentlyLoggedInUser = user;
		}
	}

	public Errors RequireCurrentlyLoggedInUser(Errors errors)
	{
		if(_c.CurrentlyLoggedInUser == null)
		{
			errors.Add(null, "User must be logged in for this operation");
		}

		return errors;
	}

	public void RequireCurrentlyLoggedInUser()
	{
		if(_c.CurrentlyLoggedInUser == null)
		{
			throw new Error(System.Net.HttpStatusCode.Unauthorized, "User must be logged in for this operation", false, null);
		}
	}

	protected async Task HandleExceptionOnThisLayer(Exception e, string details, MyCompany.World.Common.Entities.LogExtraColumnNames? logExtraColumnName = null, int? logExtraColumnValue = null, MyCompany.World.Common.Entities.LogExtraColumnNames? logExtraColumn2Name = null, int? logExtraColumn2Value = null, bool propagate = true)
	{
		if(e is Error)
		{
			var error = (Error)e;
			if((e as Error).SkipLoggingIntoDbAndMail == false)
			{
				long errorLogId = await this.WriteLog(e, details, _c.CurrentlyLoggedInUser, logExtraColumnName, logExtraColumnValue, logExtraColumn2Name, logExtraColumn2Value);
				(e as Error).LogId = errorLogId;
				(e as Error).FriendlyMessage = (e as Error).FriendlyMessage ?? $"Error occured during operation. Please try again or send us this error ID: {errorLogId}.";
				_c.Log((e as Error), (e as Error).FriendlyMessage);
			}
		}
		else
		{
			long errorLogId = await this.WriteLog(e, details, _c.CurrentlyLoggedInUser, logExtraColumnName, logExtraColumnValue, logExtraColumn2Name, logExtraColumn2Value);
			e = new Error($"Error occured during operation. Please try again or send us this error ID: {errorLogId}.", e);
			(e as Error).LogId = errorLogId;
			_c.Log((e as Error), (e as Error).Message);
		}

		if(propagate)
		{
			throw e;
		}
	}

	async Task<long> WriteLog(Exception e, string details, User currentlyLoggedInUserOrNull, LogExtraColumnNames? logExtraColumnName, int? logExtraColumnValue, LogExtraColumnNames? logExtraColumn2Name, int? logExtraColumn2Value)
	{
		long errorLogId;

		try
		{
			errorLogId = await _serviceForLogs.Write(e, currentlyLoggedInUserOrNull, details, logExtraColumnName, logExtraColumnValue, logExtraColumn2Name, logExtraColumn2Value);
		}
		catch(Exception ee)
		{
			errorLogId = await _serviceForLogs.Write(e, null, details.ToNonNullString(), null, null);
		}

		return errorLogId;
	}
}
