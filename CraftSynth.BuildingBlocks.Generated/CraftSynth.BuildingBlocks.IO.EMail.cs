using System.Net;
//TODO: port to DotNet6
//using System.Web.Configuration;//
//using System.Net.Configuration;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CraftSynth.BuildingBlocks.Logging;
using CBB_CommonExtenderClass = CraftSynth.BuildingBlocks.Common.ExtenderClass;
using CBB_Logging = CraftSynth.BuildingBlocks.Logging;

namespace CraftSynth.BuildingBlocks.IO;

public class EMail
{

	//TODO: port to DotNet6

	//      /// <summary>
	//      /// Sends email by using smtp configuration from:
	//      /// file at: HttpContext.Current.Request.ApplicationPath or ConfigurationManager.OpenExeConfiguration("/web.config") or ConfigurationManager.OpenExeConfiguration(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, "web.config")
	//      /// from tag: system.net/mailSettings
	//      /// </summary>
	//      /// <param name="mail"></param>
	//      public static void SendMailUsingMailSettingsSectionGroupFromConfig(MailMessage mail, HttpContext httpContext = null, object customTraceLog = null, bool useSsl = false, bool ignoreCertificateErrors = false)
	//{
	//          Configuration config = null;
	//          if (httpContext != null)
	//          {
	//              config = WebConfigurationManager.OpenWebConfiguration(httpContext.Request.ApplicationPath);
	//          }
	//          else
	//          {
	//              try
	//              {
	//                  config = ConfigurationManager.OpenExeConfiguration("/web.config");
	//              }
	//              catch
	//              {
	//                  try
	//                  {
	//                      config = ConfigurationManager.OpenExeConfiguration(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, "web.config"));
	//                  }
	//                  catch
	//                  {
	//                      throw;
	//                      //try
	//                      //{
	//                      //	config = ConfigurationManager.OpenExeConfiguration(Path.Combine(HttpContext.Current.Request.ApplicationPath, "web.config"));
	//                      //}
	//                      //catch
	//                      //{
	//                      //try
	//                      //{
	//                      //	config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/web.config");
	//                      //}
	//                      //catch
	//                      //{
	//                      //	try
	//                      //	{
	//                      //		config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(Path.Combine(CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath, "web.config"));
	//                      //	}
	//                      //	catch
	//                      //	{
	//                      //		try
	//                      //		{
	//                      //			config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(Path.Combine(HttpContext.Current.Request.ApplicationPath, "web.config"));
	//                      //		}
	//                      //		catch
	//                      //		{
	//                      //			throw;
	//                      //		}
	//                      //	}
	//                      //}
	//                      //}
	//                  }
	//              }
	//          }

	//	MailSettingsSectionGroup s = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
	//          SendMail(s.Smtp.Network.Host, s.Smtp.Network.Port, s.Smtp.Network.UserName, s.Smtp.Network.Password, mail, customTraceLog, useSsl, ignoreCertificateErrors, s.Smtp.Network.DefaultCredentials, s.Smtp.DeliveryMethod);
	//}

	/// <summary>
	/// Depreciated. Used for backward compatibility.
	/// </summary>
	/// <param name="hostname"></param>
	/// <param name="port"></param>
	/// <param name="username"></param>
	/// <param name="password"></param>
	/// <param name="mail"></param>
	/// <param name="customTraceLog"></param>
	public static void SendMailViaFlowSmtpServer(string hostname, int port, bool useSsl, string username, string password, MailMessage mail, object customTraceLog = null)
	{
		SendMail(hostname, port, username, password, mail, customTraceLog, useSsl, true, null);
	}

	/// <summary>
	/// Use this when sending via office365 as it performs some pre-checks and use proper settings for ssl.
	/// </summary>
	public static void SendMailViaOffice365(string hostname, int port, string username, string password, MailMessage mail, object customTraceLog = null)
	{
		if(mail.From == null)
		{
			mail.From = new MailAddress(username);
		}

		if(string.Compare(mail.From.Address, username, StringComparison.OrdinalIgnoreCase) != 0)
		{
			throw new Exception(string.Format("Office365 will not allow mail relay with different Office365 username and mail's From field. Username: {0}, From: {1}", username, mail.From.Address));
		}

		SendMail(hostname, port, username, password, mail, customTraceLog, true, false, null);
	}

	public static void SendMail(string hostname, int? port, string username, string password, MailMessage mail, object customTraceLog = null, bool useSsl = false, bool ignoreCertificateErrors = false, bool? useDefaultCredentials = null, SmtpDeliveryMethod? deliveryMethod = SmtpDeliveryMethod.Network)
	{
		var log = CBB_Logging.CustomTraceLog.Unbox(customTraceLog);
		string recipients = string.Empty;
		if(log != null)
		{
			var recipientsList = new List<string>();
			foreach(MailAddress mailAddress in mail.To)
			{
				recipientsList.Add(mailAddress.Address);
			}
			recipients = CBB_CommonExtenderClass.ToCSV(recipientsList);
		}

		var c = new SmtpClient();
		c.Host = hostname;
		if(port.HasValue)
		{
			c.Port = port.Value;
		}
		if(useDefaultCredentials != null)
		{
			c.UseDefaultCredentials = useDefaultCredentials.Value;
		}
		c.EnableSsl = useSsl;
		c.Credentials = new NetworkCredential(username, password);
		if(deliveryMethod != null)
		{
			c.DeliveryMethod = deliveryMethod.Value;
		}

		if(ignoreCertificateErrors)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
			{
				return true;
			};
		}

		using(log.LogScope("Sending mail to '" + recipients + "' ..."))
		{
			c.Send(mail);
		}
	}

	public static async Task SendMailAsync(string hostname, int? port, string username, string password, MailMessage mail, object customTraceLog = null, bool useSsl = false, bool ignoreCertificateErrors = false, bool? useDefaultCredentials = null, SmtpDeliveryMethod? deliveryMethod = SmtpDeliveryMethod.Network, object? userToken = null, SendCompletedEventHandler sendCompletedCallback = null)
	{
		var log = CBB_Logging.CustomTraceLog.Unbox(customTraceLog);
		string recipients = string.Empty;
		if(log != null)
		{
			var recipientsList = new List<string>();
			foreach(MailAddress mailAddress in mail.To)
			{
				recipientsList.Add(mailAddress.Address);
			}
			recipients = CBB_CommonExtenderClass.ToCSV(recipientsList);
		}

		var smtpClient = new SmtpClient();
		smtpClient.Host = hostname;
		if(port.HasValue)
		{
			smtpClient.Port = port.Value;
		}
		if(useDefaultCredentials != null)
		{
			smtpClient.UseDefaultCredentials = useDefaultCredentials.Value;
		}
		smtpClient.EnableSsl = useSsl;
		smtpClient.Credentials = new NetworkCredential(username, password);
		if(deliveryMethod != null)
		{
			smtpClient.DeliveryMethod = deliveryMethod.Value;
		}

		if(ignoreCertificateErrors)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
			{
				return true;
			};
		}

		SendCompletedEventHandler eventHandler;
		if(sendCompletedCallback != null)
		{
			eventHandler = sendCompletedCallback;
		}
		else
		{
			eventHandler = new SendCompletedEventHandler((sender, e) =>
			{
				// Default callback implementation, if no callback is provided
				// You can still access the userToken here via e.UserState
				object? token = e.UserState;

				// default callback logic
				if(e.Error != null)
				{
					log?.AddLine($"Error: {e.Error.Message}");
				}
				else if(e.Cancelled)
				{
					log?.AddLine("Send canceled.");
				}
				else
				{
					log?.AddLine($"Mail sent successfully to {recipients}.");
				}
			});
		}
		smtpClient.SendCompleted += eventHandler;

		try
		{
			smtpClient.SendAsync(mail, userToken);
		}
		catch(Exception ex)
		{
			log?.AddLine($"Exception occurred: {ex.Message}");
			throw;
		}
		finally
		{
			smtpClient.SendCompleted -= eventHandler;
			smtpClient.Dispose();
		}
	}
}
