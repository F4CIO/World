using System.Drawing;
using System.Net;
using System.Text.Json;

namespace InfoCompass.World.Common.Entities;

public class ServerResponse<T>
{
	public bool IsSuccess { get; set; }
	public string Message { get; set; }
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
	public long? LogId { get; set; }
	public string? LogDetailsUri { get; set; }
	public string? Tag { get; set; }
	public T? Data { get; set; }

	public Errors? Errors { get; set; }

	public HttpStatusCode? HttpStatusCode { get; set; } //needed?

	public ServerResponse(bool isSuccess, string message = null, long? logId = null, T? data = default(T), Errors? errors = null, KnownColor? messageColor = null, HttpStatusCode? httpStatusCode = null)
	{
		this.IsSuccess = isSuccess;
		this.Message = message;
		this.Data = data;
		this.Errors = errors;
		if(messageColor != null)
		{
			_messageColor = messageColor.Value;
		}

		this.HttpStatusCode = httpStatusCode;
		if(this.HttpStatusCode != null)
		{
			if(this.IsSuccess)
			{
				this.HttpStatusCode = System.Net.HttpStatusCode.OK;
			}
			else
			{
				this.HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
			}
		}

		//if (logId == null && c != null)
		//{
		//    c.Log(ToJson());
		//    var serviceForLogs = c.ServiceProvider.GetService<IServiceForLogs>();
		//    serviceForLogs.Write(LogCategoryAndTitle.UserAction__Api_Called, (int?)c.CurrentlyLoggedInUser?.Id, ToJson());
		//}
		this.LogId = logId;
	}

	public virtual string ToJson()
	{
		string r = JsonSerializer.Serialize(this, typeof(ServerResponse<T>), new JsonSerializerOptions() { WriteIndented = true });
		return r;
	}

	public override string ToString()
	{
		string dataAsString = this.Data == null ? "null" : "...";
		if(this.Data is Errors)
		{
			dataAsString = (this.Data as Errors).ToString();
		}
		else
		{
			try
			{
				dataAsString = this.Data.ToString();
			}
			catch { }
		}
		string r = $"IsSuccess={this.IsSuccess}, Message='{this.Message.ToNonNullString("null")}', Data={dataAsString}, LogId={this.LogId.ToNonNullString("null")}, LogDetailsUri={this.LogDetailsUri.ToNonNullString("null")}, HttpStatusCode={this.HttpStatusCode.ToNonNullString("null")}";
		return r;
	}
}