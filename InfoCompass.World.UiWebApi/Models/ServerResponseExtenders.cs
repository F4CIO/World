using Microsoft.AspNetCore.Mvc;

namespace InfoCompass.World.UiWebApi.Models;

public static class ServerResponseExtenders
{
	public static HttpResponseMessage ToHttpResponseMessage<T>(this ServerResponse<T> serverResponse, ControllerBase apiController)
	{
		var r = new HttpResponseMessage(serverResponse.HttpStatusCode.Value);
		string json = System.Text.Json.JsonSerializer.Serialize(serverResponse);
		r.Content = new StringContent(json);

		//apiController.Request.CreateResponse<ServerResponse>(serverResponse.HttpStatusCode, serverResponse, apiController.Configuration.Formatters.JsonFormatter);

		return r;
	}
}
