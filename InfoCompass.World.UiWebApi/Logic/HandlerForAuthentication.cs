//using System.Web.Http.Results;
//using System.Web.Security;
//using TS.Common.Entities;
//using tsdashboard.Models.Technician;
//using WebMatrix.WebData;

namespace MyCompany.World.UiWebApi.Logic;

public class HandlerForAuthentication
{
	//     public static void Login(Technician technician, string username, string googleToken, string customLogSuffix = null, bool measureActivities = true)
	//     {      
	//         HttpContext.Current.Session["UserName"] = username;
	//         HttpContext.Current.Session["GoogleToken"] = googleToken;
	//         HttpContext.Current.Session["MeasureActivities"] = measureActivities;

	//         if (technician == null)
	//         {
	//             HttpContext.Current.Session["Technician"] = null;
	//             HttpContext.Current.Session["CurrentlyLoggedInUser"] = null;
	//         }
	//         else
	//         {
	//             HttpContext.Current.Session["Technician"] = technician;
	//             HttpContext.Current.Session["CurrentlyLoggedInUser"] = TS.BusinessLogic.HandlersFactory.HandlerForUsers.GetById(technician.UserId, null);

	//             if (technician.Email.ToNonNullString().ToLower().Contains("nenad.cucin@autovitalsinc.com"))
	//             {
	//                 measureActivities = false;
	//                 HttpContext.Current.Session["MeasureActivities"] = measureActivities;
	//             }
	//         }

	//         GenerateAuthenticationTicket(technician.Email, false, new TimeSpan(3650, 0, 0, 0));

	//         Services.ActivityMonitor.Remove(technician.UserId);

	////Potential bugxix for https://app.asana.com/0/652911843005107/776643507992399/f
	//         if (WebApiApplication.UsersScheduledToLogOutOnNextRequest.Contains(technician.UserId))
	//         {
	//             WebApiApplication.UsersScheduledToLogOutOnNextRequest.Remove(technician.UserId);
	//             Logging.Logger.Log(LogCategoryAndTitle.InternalEvent__Debug, "UsersScheduledToLogOutOnNextRequest contains userid:" + technician.UserId+". Removed it.");
	//         }
	//         else
	//         {
	//             Logging.Logger.Log(LogCategoryAndTitle.InternalEvent__Debug, "UsersScheduledToLogOutOnNextRequest does not contain userid:" + technician.UserId);
	//         }                        

	//         if (technician!=null && measureActivities && !technician.Email.ToNonNullString().ToLower().Contains("nenad.cucin@autovitalsinc.com")/*disabled for developers*/)
	//         {
	//             TS.BusinessLogic.HandlersFactory.HandlerForUsers.ChangeUserAvailability(technician.UserId, true);//https://app.asana.com/0/528008057040619/1111899041609659/f

	//             TS.BusinessLogic.HandlersFactory.HandlerForMeasuredActivities.Write(MeasuredActivitiesNames.LogIn, technician.UserId, null, null);
	//             TS.BusinessLogic.HandlersFactory.HandlerForBudgetedHoursAndLoginStatuses.ChangeUserLoginStatus(technician.UserId, true);

	//             Logging.Logger.Log(LogCategoryAndTitle.UserAction__User_Logged_In, "EMail:" + username.ToNonNullString("null") + customLogSuffix.ToNonNullString());
	//         }
	//     }

	//     public static void Logout(LogoutType logoutType)
	//     {
	//         if (HandlerForAuthentication.CurrentlyLoggedInUser != null)
	//         {//log this logout event only if user is really logged in. But perform logout in all cases to insure cleanup.
	//             Logging.Logger.Log(LogCategoryAndTitle.UserAction__User_Logged_Out, WebApiApplication.WebSiteRootUrl, LogExtraColumnNames.UserId, HandlerForAuthentication.CurrentlyLoggedInUserId);
	//         }

	//         if (HandlerForAuthentication.CurrentlyLoggedInUser != null && logoutType==LogoutType.Active)
	//         {//this log item is neccessary for ticket auto-assignment. do not remove it.
	//             Logging.Logger.Log(LogCategoryAndTitle.UserAction__User_Logged_Out_Actively, TS.Common.Helper.DefaultToHttp(WebApiApplication.WebSiteRootUrl), LogExtraColumnNames.UserId, HandlerForAuthentication.CurrentlyLoggedInUserId, null, null, null, false/*important*/);
	//         }

	//         try
	//         {
	//             Logging.Logger.Log(LogCategoryAndTitle.InternalEvent__LogOutPerformed, System.Environment.StackTrace, LogExtraColumnNames.UserId, HandlerForAuthentication.CurrentlyLoggedInUserId);
	//         }
	//         catch
	//         {
	//             //informative logging so don't break execution if it fails
	//         }

	//         Services.ActivityMonitor.Remove(Services.HandlerForAuthentication.CurrentlyLoggedInUserId);

	//         if (Services.HandlerForAuthentication.CurrentlyLoggedInUserId != null && WebApiApplication.UsersScheduledToLogOutOnNextRequest.Contains(Services.HandlerForAuthentication.CurrentlyLoggedInUserId))
	//         {
	//             WebApiApplication.UsersScheduledToLogOutOnNextRequest.Remove(Services.HandlerForAuthentication.CurrentlyLoggedInUserId);
	//         }

	//         if (HandlerForAuthentication.CurrentlyLoggedInUser != null && !HandlerForAuthentication.CurrentlyLoggedInUser.Email.ToNonNullString().ToLower().Contains("nenad.cucin@autovitalsinc.com")/*disabled for developers*/)
	//         {
	//             TS.BusinessLogic.HandlersFactory.HandlerForMeasuredActivities.Write(MeasuredActivitiesNames.LogOut, HandlerForAuthentication.CurrentlyLoggedInUserId, null, null, (int)logoutType);
	//             TS.BusinessLogic.HandlersFactory.HandlerForBudgetedHoursAndLoginStatuses.ChangeUserLoginStatus(HandlerForAuthentication.CurrentlyLoggedInUser.UserId, false);
	//         }
	//         FormsAuthentication.SignOut();
	//         try
	//         {
	//             WebSecurity.Logout();
	//         }
	//         catch (Exception e)
	//         {
	//             //logout was never requested from management so we are not spending too much time on it
	//         }

	//         if (HttpContext.Current != null && HttpContext.Current.Session != null)
	//         {
	//             if (HttpContext.Current.Session["GoogleToken"] != null) HttpContext.Current.Session.RemoveAll();
	//         }            
	//     }

	//     /// <summary>
	//     /// Call this from Application_AcquireRequestState in global.asax and only for normal pages non-ajax requests
	//     /// </summary>
	//     public static void LogoutUserIfNoSessionInCurrentRequest()
	//     {
	//         if (   (HttpContext.Current.Session == null
	//                || HttpContext.Current.Session["Technician"] == null
	//                || HttpContext.Current.Session["CurrentlyLoggedInUser"] == null
	//                || HttpContext.Current.Session["UserName"] == null
	//                || HttpContext.Current.Session["GoogleToken"] == null)
	//              &&
	//                HttpContext.Current.User.Identity.IsAuthenticated               
	//            )
	//         {//if asp.net auth cookie exist but session expired logout user.
	//             Logout(LogoutType.AspNet_Session_Empty);
	//             RedirectToLoginPage(true);
	//         }
	//     }

	public static bool IsAuthenticated()
	{
		bool r = false;
		//    HttpContext.Current!=null 
		//         && HttpContext.Current.User!=null 
		//         && HttpContext.Current.User.Identity!=null
		//         && HttpContext.Current.User.Identity.IsAuthenticated 
		//         && HttpContext.Current.Session!=null
		//         && HttpContext.Current.Session["Technician"] != null
		//         && HttpContext.Current.Session["CurrentlyLoggedInUser"] != null
		//         && HttpContext.Current.Session["UserName"] !=null
		//         && HttpContext.Current.Session["GoogleToken"] != null;
		return r;
	}

	public static User CurrentlyLoggedInUser
	{
		get
		{
			User r = null;

			if(IsAuthenticated())
			{
				throw new NotImplementedException();
				//r = HttpContext.Current.Session["CurrentlyLoggedInUser"] as User;
			}

			return r;
		}
	}

	public static int? CurrentlyLoggedInUserId
	{
		get
		{
			int? r = null;

			if(IsAuthenticated())
			{
				throw new NotImplementedException();
				//r = (HttpContext.Current.Session["Technician"] as Technician).UserId;
			}

			return r;
		}
	}

	//      public static bool ActivitiesAreMeasuredForCurrentlyLoggedInUser
	//      {
	//          get
	//          {
	//              bool r = false;

	//              try
	//              {
	//                  r = HandlerForAuthentication.CurrentlyLoggedInUser != null && ((bool)HttpContext.Current.Session["MeasureActivities"]) == true;
	//              }
	//              catch (Exception e)
	//              {
	//                  r = false;
	//              }
	//              return r;
	//          }
	//      }

	//private static string AccessKeyOfCurrentlyLoggedInUserOrDefault
	//{
	//	get
	//	{
	//		string r = null;
	//		if (Services.HandlerForAuthentication.CurrentlyLoggedInUser != null)
	//		{
	//			r = Services.HandlerForAuthentication.CurrentlyLoggedInUser.AccessToken;
	//		}
	//		r = r ?? AppSettingsProxy_Generated.Instance.MojoApiKey;
	//		return r;
	//	}
	//}

	//private static void GenerateAuthenticationTicket(string username, bool persist, TimeSpan expireAfter)
	//      {
	//          FormsAuthenticationTicket tkt;
	//          string cookiestr;
	//          HttpCookie ck;
	//          tkt = new FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.Add(expireAfter), persist, "your custom data");
	//          cookiestr = FormsAuthentication.Encrypt(tkt);
	//          ck = new HttpCookie(FormsAuthentication.FormsCookieName, cookiestr);
	//          if (persist)
	//          {
	//              ck.Expires = tkt.Expiration;
	//          }
	//          ck.Path = FormsAuthentication.FormsCookiePath;
	//          HttpContext.Current.Response.Cookies.Add(ck);
	//      }

	//public static void RedirectToLoginPage(bool scheduleReturningToCurrentUrlAfterLogin = false)
	//{
	//    var queryString = new List<KeyValuePair<string, string>>();
	//    if (scheduleReturningToCurrentUrlAfterLogin)
	//    {
	//        queryString.Add(new KeyValuePair<string, string>("ReturnUrl", HttpContext.Current.Request.Url.AbsoluteUri));
	//    }
	//    string uri = Services.HelperForUriMvc.BuildUriToAction(HttpContext.Current, MVC.Account.ActionNames.Login, MVC.Account.Name, false, queryString.ToArray());
	//    HttpContext.Current.Response.Redirect(uri);
	//}

	//public static void RedirectToLoginPageIfNotAuthenticated(bool scheduleReturningToCurrentUrlAfterLogin = false)
	//{
	//    if(!IsAuthenticated())
	//    {
	//        RedirectToLoginPage(scheduleReturningToCurrentUrlAfterLogin);
	//    }
	//}

	public static bool CurrentlyLoggedInUserIsAdmin()
	{
		bool r = false;

		if(CurrentlyLoggedInUser != null)
		{
			throw new NotImplementedException();
			//List<string> admins = System.Configuration.ConfigurationManager.AppSettings["AdminList"].ToNonNullString().ToLower().Split('|').ToList();
			//r = admins.Any(a => a.IsEqualToWhileDisregardingCasing(CurrentlyLoggedInUser.Email));
		}

		return r;
	}
}