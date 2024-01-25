using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace InfoCompass.World.Common.Entities;

public enum LogCategoryAndTitle
{
	[Description("")][DefaultLogLevel(LogLevel.Information)] UserMessage__Other,

	//[Description("User requested url")]                                                                                                  UserAction__Url_Requested,
	////disabling because its just too many:                                                                                               
	////[Description("Log in screen shown")]                                                                                               UserAction__LoginPageShown,
	//[Description("User attempted to log in")]                                                                                            UserAction__User_LoginAttempt,
	//[Description("User log in failed")]                                                                                                  UserAction__User_LoginFailed,
	[Description("User Registration Confirmation Link Sent")][DefaultLogLevel(LogLevel.Information)] UserAction__User_Registration_Confirmation_Link_Sent,
	[Description("User registered")][DefaultLogLevel(LogLevel.Information)] UserAction__User_Registered,
	[Description("User logged In")][DefaultLogLevel(LogLevel.Information)] UserAction__User_Logged_In,
	//[Description("User logged Out (due to inactivity)")]                                                                                 UserAction__User_Log_Out_Scheduled,
	[Description("User logged Out")][DefaultLogLevel(LogLevel.Information)] UserAction__User_Logged_Out,
	//[Description("User inserted or updated")]                                                                                            UserAction__User_InsertOrUpdate,
	[Description("User reset password request created")][DefaultLogLevel(LogLevel.Information)] UserAction__Password_Reset_Request_Created,
	[Description("User password changed")][DefaultLogLevel(LogLevel.Information)] UserAction__Password_Changed,
	[Description("User details changed")][DefaultLogLevel(LogLevel.Information)] UserAction__User_Details_Changed,
	[Description("User posted message")][DefaultLogLevel(LogLevel.Information)] UserAction__User_Posted_Message,
	[Description("Password reset request token verification failed")][DefaultLogLevel(LogLevel.Information)] UserAction__Password_Reset_Request_Token_Verification_Failed,
	//[Description("User activated")]                                                                                                      UserAction__User_Activated,
	//[Description("User deactivated")]                                                                                                    UserAction__User_Deactivated,
	[Description("Jwt deactivated")][DefaultLogLevel(LogLevel.Information)] UserAction__Jwt_Deactivated,

	[Description("API called")][DefaultLogLevel(LogLevel.Information)] UserAction__Api_Called,

	[Description("User started generating initial book")][DefaultLogLevel(LogLevel.Information)] UserAction__Generate_Initial_Job_Launched,
	[Description("User started generating book")][DefaultLogLevel(LogLevel.Information)] UserAction__Generate_Job_Launched,
	[Description("Initial Book Sent")][DefaultLogLevel(LogLevel.Information)] InternalEvent__Initial_Book_Sent,
	[Description("Book Sent")][DefaultLogLevel(LogLevel.Information)] InternalEvent__Book_Sent,

	//[Description("User profile updated")]                                                                                                UserAction__User_Profile_Updated,

	//[Description("Operation  executor executed")]                                                                                        UserAction__Operation_Executor_Executed,
	//[Description("Bulk editor execution log")]                                                                                           UserAction__Bulk_Editor_Execution_Log,
	//[Description("Edit user page shown")]                                                                                                UserAction__EditUserPageShown,

	//[Description("")]                                                                                                                    UserAction__ReportGenerated,

	[Description("")] InternalEvent__Web_Site_Started,
	//[Description("")]                                                                                                                    InternalEvent__Web_Site_Ended,
	//[Description("")]                                                                                                                    InternalEvent__Ticket_AutoAssigned,
	//[Description("EMail submitted to MailGun or mail server")]                                                                           InternalEvent__EMailSent,
	//[Description("")]                                                                                                                    InternalEvent__Progress_Created,
	//[Description("Promoted ticket(s) with their automated no-data pairs from same shop")]                                                InternalEvent__Tickets_Promoted,

	//[Description("Internal log out with stack trace for debugging")]                                                                     InternalEvent__LogOutPerformed,
	//[Description("Internal log out that happens at midnight for all users")]                                                             InternalEvent__LogOutAllUsersScheduled,
	[Description("")][DefaultLogLevel(LogLevel.Information)] InternalEvent__Other,
	[Description("")][DefaultLogLevel(LogLevel.Debug)] InternalEvent__Debug,
	//[Description("New user from Mojo inserted into qtool db")]                                                                           InternalEvent__NewUserInserted,
	//[Description("")]                                                                                                                    InternalEvent__AllCacheDropped,
	//[Description("")]                                                                                                                    InternalEvent__AllExpiredCacheDropped,
	//[Description("")]                                                                                                                    InternalEvent__GetOpenTicketsCountExecuted,
	//[Description("")]                                                                                                                    InternalEvent__YouTrackaWebhook,
	//[Description("")]                                                                                                                    InternalEvent__IPowerWebhook,

	[Description("")][DefaultLogLevel(LogLevel.Error)] Error__Other,
	[Description("")][DefaultLogLevel(LogLevel.Warning)] Error__Warning,
	[Description("")][DefaultLogLevel(LogLevel.Critical)] Error__Critical,
}


public static class LogCategoryAndTitleExtensions
{
	public static string GetDescription(this Enum value)
	{
		FieldInfo fi = value.GetType().GetField(value.ToString());
		var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

		if(attributes != null && attributes.Length > 0)
		{
			return attributes[0].Description;
		}
		else
		{
			return value.ToString();
		}
	}

	public static LogLevel? GetDefaultLogLevel(this Enum value)
	{
		FieldInfo fi = value.GetType().GetField(value.ToString());
		var attributes = (DefaultLogLevelAttribute[])fi.GetCustomAttributes(typeof(DefaultLogLevelAttribute), false);

		if(attributes != null && attributes.Length > 0)
		{
			return attributes[0].Level;
		}
		else
		{
			return null;
		}
	}

	public static LogLevel? Parse(string s, LogLevel? invalidCaseResult = null)
	{
		if(Enum.TryParse<LogLevel>(s, true, out LogLevel parsedLogLevel))
		{
			return parsedLogLevel;
		}
		else
		{
			return invalidCaseResult;
		}
	}
}
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
internal sealed class DefaultLogLevelAttribute:Attribute
{
	public LogLevel Level { get; }

	public DefaultLogLevelAttribute(LogLevel level)
	{
		this.Level = level;
	}
}
