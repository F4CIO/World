using System.Net;
using System.Runtime.Serialization;

namespace InfoCompass.World.Common.Entities;

public class Errors:List<Error>
{
	public bool IsEmpty
	{
		get { return this.Count == 0; }
	}

	public void Add(string key, string friendlyMessage, Exception innerException = null)
	{
		this.Add(new Error(key, friendlyMessage, innerException));
	}

	public Errors OnlyOnesToShowOnUI
	{
		get
		{
			var r = new Errors();
			r.AddRange(this.Where(e => !string.IsNullOrWhiteSpace(e.FriendlyMessage) && e.SkipLoggingIntoDbAndMail == true));
			return r;
		}
	}

	public override string ToString()
	{
		return this.ToSingleString("null", "0", "|");
	}
}

//[Serializable] - throws NullReferenceException
public class Error:Exception
{
	/// <summary>
	/// Usualy used to keep name of input field on UI where validation did't pass. 
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Usually used to keep status code that Web API web service can catch and build proper response if this error propagates up in the layers.
	/// </summary>
	public HttpStatusCode? HttpStatusCode { get; set; }

	/// <summary>
	/// Tf exception is not handled/catched explicitly in code this message will be displayed to the user.
	/// If this property is null then generic/default error message is displayed.
	/// </summary>
	public string FriendlyMessage { get; set; }

	/// <summary>
	/// Attach additional data here like trace log and values of local variables where exception occured. 
	/// To build trace string use CustomTraceLog class.
	/// </summary>
	public string LongMessage { get; set; }

	/// <summary>
	/// If used with friendly message user will be notified about error but nothing will be logged in our system. To be used for example if user didn't fill in required field or for other error of informative nature.
	/// </summary>
	public bool SkipLoggingIntoDbAndMail { get; set; }

	/// <summary>
	/// Id of db log entry where this error was logged. Usually error is logged on business logic layer so id can be used upper at UI layer to inform user where to find more details.
	/// However if SkipLoggingIntoDbAndMail is true this parameter will probably be always null.
	/// </summary>
	public long? LogId { get; set; }

	public Error(string message)
		: base(message)
	{

	}

	/// <summary>
	/// Use this constructor to create error that will be showed to user on UI and that should not be logged into system.
	/// </summary>
	/// <param name="key">Usualy name of input field where validation did't pass.</param>
	/// <param name="friendlyMessage">Message that should be displayed on screen to user.</param>
	/// <param name="innerException">Can be used to show technical details of an error if user insist.</param>
	public Error(string key, string friendlyMessage, Exception innerException = null) : base(friendlyMessage, innerException)
	{
		this.FriendlyMessage = friendlyMessage;
		this.LongMessage = null;
		this.SkipLoggingIntoDbAndMail = true;
		this.Key = key;
	}

	/// <summary>
	/// Use this constructor under the REST WebAPI layer to create error that will be catched and properly handled when error propagates up to that layer.
	/// </summary>
	/// <param name="httpStatusCode">Proper status code to be returned by REST WebAPI.</param>
	/// <param name="Message">Message about error.</param>
	/// <param name="innerException">Can be used to show technical details of an error if user insist.</param>
	public Error(HttpStatusCode httpStatusCode, string message = null, Exception innerException = null) : base(message, innerException)
	{
		this.HttpStatusCode = httpStatusCode;
	}

	/// <summary>
	/// This error represent business logic error that will be shown to user on UI and will not be logged.
	/// Use this constructor under the REST WebAPI layer to create error that will be catched and properly handled when error propagates up to that layer.
	/// </summary>
	/// <param name="httpStatusCode">Proper status code to be returned by REST WebAPI.</param>
	/// <param name="friendlyMessage">Message that should be displayed on screen to user.</param>
	/// <param name="skipLogging">You can explicitly tell whether or not this error should be logged.</param>
	/// <param name="innerException">Can be used to show technical details of an error if user insist.</param>
	public Error(HttpStatusCode httpStatusCode, string friendlyMessage, bool skipLogging, Exception innerException = null) : base(friendlyMessage, innerException)
	{
		this.HttpStatusCode = httpStatusCode;
		this.FriendlyMessage = friendlyMessage;
		this.SkipLoggingIntoDbAndMail = skipLogging;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="message"></param>
	/// <param name="innerException"></param>
	/// <param name="longMessage">
	/// Attach additional data here like trace log and values of local variables where exception occured. 
	/// To build trace string use CustomTraceLog class.
	/// </param>
	/// <param name="friendlyMessage">
	/// Tf exception is not handled/catched explicitly in code this message will be displayed to the user.
	/// If this property is null then generic/default error message is displayed.
	/// </param>
	/// <param name="skipLoggingIntoDbAndMail">
	/// If used with friendly message user will be notified about error but nothing will be logged in our system.
	/// </param>
	/// <param name="key">
	/// Usualy used to keep name of input field where validation did't pass.
	/// </param> 
	public Error(string message, Exception innerException = null, string longMessage = null, string friendlyMessage = null, bool skipLoggingIntoDbAndMail = false, string key = null)
		: base(message, innerException)
	{
		this.FriendlyMessage = friendlyMessage;
		this.LongMessage = longMessage;
		this.SkipLoggingIntoDbAndMail = skipLoggingIntoDbAndMail;
		this.Key = key;
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);

		if(info != null)
		{
			info.AddValue("LongMessage", this.LongMessage ?? string.Empty);
			info.AddValue("FriendlyMessage", this.FriendlyMessage ?? string.Empty);
		}
	}

	public override string ToString()
	{
		Exception de = CraftSynth.BuildingBlocks.Common.Misc.GetDeepestException(this);
		string? exceptionsAsString = CraftSynth.BuildingBlocks.Common.Misc.GetInnerExceptionsAsSingleString(this);

		string kvps = string.Empty;
		if(this.Data != null)
		{
			foreach(object? k in this.Data.Keys)
			{
				kvps += k + "=" + this.Data[k] + "\r\n";
			}
		}

		string s = string.Format("FriendlyMessage:{0}\r\nMessage:{1}\r\nLongMessage:{2}\r\nKeyField:{3}\r\nData{4}\r\nStackTrace:{5}\r\nExceptionsTree:{6}",
			this.FriendlyMessage.ToNonNullNonEmptyString(""),
			de.Message.ToNonNullNonEmptyString(""),
			this.LongMessage.ToNonNullNonEmptyString(""),
			this.Key.ToNonNullNonEmptyString(""),
			kvps,
			de.StackTrace.ToNonNullNonEmptyString(""),
			exceptionsAsString
			);

		return s;
	}
}
