using InfoCompass.World.UiWebApi.Logic;
using InfoCompass.World.UiWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace InfoCompass.World.Public.Api;


public class BaseController:Microsoft.AspNetCore.Mvc.ControllerBase
{
	[ApiExplorerSettings(IgnoreApi = true)]
	public bool CurrentlyLoggedInUserIsAdmin()
	{
		ServiceForBase? serviceForBase = this.HttpContext.RequestServices.GetService<ServiceForBase>();
		bool r = serviceForBase.CurrentlyLoggedInUserIsAdmin();

		return r;
	}

	[ApiExplorerSettings(IgnoreApi = true)]
	public virtual ActionResult RedirectToHome()
	{
		throw new NotSupportedException();
		//return this.Redirect(Services.HelperForUriMvc.BuildUriToAction_Simple(null, MVC.Queue.Name, MVC.Queue.ActionNames.Technician));
	}

	protected string CurrentRequestUri
	{
		get
		{
			string r;

			var uri = new Uri($"{this.HttpContext.Request.Scheme}://{this.HttpContext.Request.Host}{this.HttpContext.Request.Path}{this.HttpContext.Request.QueryString}");
			r = uri.AbsoluteUri;

			return r;
		}
	}

	protected async Task<ServerResponseForUI<T>> HandleExceptionOnThisLayer<T>(Exception e)
	{
		IServiceForBase? serviceForBase = this.HttpContext.RequestServices.GetService<IServiceForBase>();
		ServerResponseForUI<T> r = await serviceForBase.HandleExceptionOnThisLayer<T>(e);
		return r;
	}
}
