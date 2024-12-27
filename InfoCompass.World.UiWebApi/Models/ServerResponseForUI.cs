using System.Drawing;
using System.Net;
using MyCompany.World.BusinessLogic;
using Errors = MyCompany.World.Common.Entities.Errors;

namespace MyCompany.World.UiWebApi.Models;

public class ServerResponseForUI<T>:MyCompany.World.Common.Entities.ServerResponse<T>
{
	private KnownColor? _messageColor;
	public KnownColor? MessageColor
	{
		set
		{
			_messageColor = value;
		}
		get
		{
			if(_messageColor == null)
			{
				return this.IsSuccess ? KnownColor.Green : KnownColor.Red;
			}
			else
			{
				return _messageColor;
			}
		}
	}

	//     public ServerResponse(bool isSuccess, string message = null, long? logId = null, object data = null, KnownColor? messageColor = null, HttpStatusCode? httpStatusCode = null)
	public ServerResponseForUI(ServiceForCOE c, Common.Entities.ServerResponse<T> serverResponse)
						  : base(serverResponse.IsSuccess, serverResponse.Message, serverResponse.LogId, serverResponse.Data, serverResponse.Errors, serverResponse.MessageColor, serverResponse.HttpStatusCode)
	{
		if(this.LogId == null && c != null)
		{
			c.Log(this.ToJson());
			IServiceForLogs? serviceForLogs = c.ServiceProvider.GetService<IServiceForLogs>();
			this.LogId = serviceForLogs.Write(LogCategoryAndTitle.UserAction__Api_Called, (int?)c.CurrentlyLoggedInUser?.Id, this.ToJson()).Result;
		}

		if(this.LogId != null)
		{
			Logic.IServiceForBase? serviceForBase = c.HttpContext.RequestServices.GetService<Logic.IServiceForBase>();
			this.LogDetailsUri = $"{serviceForBase.GetCurrentRequestUriRoot() ?? "/"}LogDetails/{this.LogId}";
			//this.LogDetailsUri = Services.HelperForUriMvc.BuildUriToAction(System.Web.HttpContext.Current, MVC.Manage.ActionNames.LogDetails, MVC.Manage.Name, false, new KeyValuePair<string, string>("id", this.LogId.ToString()));
		}
	}

	public ServerResponseForUI(ServiceForCOE c, bool isSuccess, string message = null, long? logId = null, T? data = default(T), Errors? errors = null, KnownColor? messageColor = null, HttpStatusCode? httpStatusCode = null)
						  : base(isSuccess, message, logId, data, errors, messageColor, httpStatusCode)
	{
		if(messageColor != null)
		{
			_messageColor = messageColor.Value;
		}

		if(this.LogId == null && c != null)
		{
			c.Log(this.ToJson());
			IServiceForLogs? serviceForLogs = c.ServiceProvider.GetService<IServiceForLogs>();
			this.LogId = serviceForLogs.Write(LogCategoryAndTitle.UserAction__Api_Called, (int?)c.CurrentlyLoggedInUser?.Id, this.ToJson()).Result;
		}

		if(this.LogId != null)
		{
			Logic.IServiceForBase? serviceForBase = c.HttpContext.RequestServices.GetService<Logic.IServiceForBase>();
			this.LogDetailsUri = $"{serviceForBase.GetCurrentRequestUriRoot() ?? "/"}LogDetails/{this.LogId}";
			//this.LogDetailsUri = Services.HelperForUriMvc.BuildUriToAction(System.Web.HttpContext.Current, MVC.Manage.ActionNames.LogDetails, MVC.Manage.Name, false, new KeyValuePair<string, string>("id", this.LogId.ToString()));
		}

		this.HttpStatusCode = httpStatusCode;
	}

	public override string ToJson()
	{
		string r = System.Text.Json.JsonSerializer.Serialize(this, typeof(ServerResponseForUI<T>), new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
		return r;
	}
}
