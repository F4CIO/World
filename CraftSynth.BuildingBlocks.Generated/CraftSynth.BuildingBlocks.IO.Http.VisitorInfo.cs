using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
//using System.Web.UI;
//using System.Web.SessionState;
using System.Net;

using CraftSynth.BuildingBlocks.Common;
using Microsoft.AspNetCore.Http;

namespace CraftSynth.BuildingBlocks.IO.Http
{
	public class VisitorInfo
	{
		public int VisitorId;
		public string VisitorHash;
		public string Hostname;
		public string IPv4;
		public System.Nullable<long> IPv4N;
		public string IPv6;
		public string ProxyIPv4;
		public System.Nullable<long> ProxyIPv4N;
		public string ProxyIPv6;
		public string BrowserInfo;
		public string PlatformInfo;
		public string UserAgentInfo;
		public string HttpReferrer;
		public string LogonUser;
		public string Language;

		public VisitorInfo(HttpRequest c)
		{			
			Dictionary<VisitorInfoHelper.HttpField, string> info = VisitorInfoHelper.CollectVisitorInfo(c);			

			long ipv4n = VisitorInfoHelper.IPv4ToInt(info[VisitorInfoHelper.HttpField.IP]);
			long proxyv4n = VisitorInfoHelper.IPv4ToInt(info[VisitorInfoHelper.HttpField.ProxyIP]);

			this.Hostname = info[VisitorInfoHelper.HttpField.Hostname];

			this.IPv4 = ipv4n >= 0 ? info[VisitorInfoHelper.HttpField.IP] : String.Empty;
			this.IPv4N = ipv4n;
			this.IPv6 = ipv4n >= 0 ? String.Empty : info[VisitorInfoHelper.HttpField.IP];

			this.ProxyIPv4 = proxyv4n >= 0 ? info[VisitorInfoHelper.HttpField.ProxyIP] : String.Empty;
			this.ProxyIPv4N = proxyv4n;
			this.ProxyIPv6 = proxyv4n >= 0 ? String.Empty : info[VisitorInfoHelper.HttpField.ProxyIP];

			this.BrowserInfo = info[VisitorInfoHelper.HttpField.BrowserInfo];
			this.PlatformInfo = info[VisitorInfoHelper.HttpField.Platform];
			this.UserAgentInfo = info[VisitorInfoHelper.HttpField.UserAgent];
			this.HttpReferrer = info[VisitorInfoHelper.HttpField.Referer];
			this.LogonUser = info[VisitorInfoHelper.HttpField.LogonUser];
			this.Language = info[VisitorInfoHelper.HttpField.Language];
			
			this.VisitorHash = this.BuildVisitorHash();
		}

		//public VisitorInfo(HttpRequestBase c)
		//{
		//	Dictionary<Trace.HttpField, string> info = Trace.CollectVisitorInfo(c);
		
		

		//	long ipv4n = Trace.IPv4ToInt(info[Trace.HttpField.IP]);
		//	long proxyv4n = Trace.IPv4ToInt(info[Trace.HttpField.ProxyIP]);

		//	this.Hostname = Truncate(info[Trace.HttpField.Hostname], 100);

		//	this.IPv4 = Truncate(ipv4n >= 0 ? info[Trace.HttpField.IP] : String.Empty, 16);
		//	this.IPv4N = ipv4n;
		//	this.IPv6 = Truncate(ipv4n >= 0 ? String.Empty : info[Trace.HttpField.IP], 50);

		//	this.ProxyIPv4 = Truncate(proxyv4n >= 0 ? info[Trace.HttpField.ProxyIP] : String.Empty, 16);
		//	this.ProxyIPv4N = proxyv4n;
		//	this.ProxyIPv6 = Truncate(proxyv4n >= 0 ? String.Empty : info[Trace.HttpField.ProxyIP], 50);

		//	this.BrowserInfo = Truncate(info[Trace.HttpField.BrowserInfo], 50);
		//	this.PlatformInfo = Truncate(info[Trace.HttpField.Platform], 50);
		//	this.UserAgentInfo = Truncate(info[Trace.HttpField.UserAgent], 255);
		//	this.HttpReferer = Truncate(info[Trace.HttpField.Referer], 255);
		//	this.LogonUser = Truncate(info[Trace.HttpField.LogonUser], 50);
		//	this.Language = Truncate(info[Trace.HttpField.Language], 50);
		
		//	this.VisitorHash = this.BuildVisitorHash();
		//}

		private static string Truncate(string s, int max)
		{
			int length = s != null ? s.Length : 0;

			if (max > 0 && length > max) return s.Substring(0, max);

			return s;
		}

		public override string ToString()
		{
			return this.ToString("\r\n", "=", 13);
		}

		public string ToString(string separator = ",", string keyValueSeparator = "=", int? minLengthForEachKey = null, string naString = "N/A", List<string> fieldsToExclude = null)
		{
			StringBuilder r = new StringBuilder();

			r.Append(BuildLine("VisitorHash", this.VisitorHash   ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("IPV4",        this.IPv4          ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("IPV6",        this.IPv6          ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("ProxyIPv4",   this.ProxyIPv4     ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("ProxyIPv6",   this.ProxyIPv6     ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("Browser",     this.BrowserInfo   ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("Platform",    this.PlatformInfo  ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("Agent",       this.UserAgentInfo ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("Referrer",    this.HttpReferrer  ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("Logon",       this.LogonUser     ,separator,keyValueSeparator,minLengthForEachKey,naString,false,fieldsToExclude));
			r.Append(BuildLine("Language",    this.Language      ,separator,keyValueSeparator,minLengthForEachKey,naString,true,fieldsToExclude));

			return r.ToString();
		}

		private string BuildLine(string key, string value, string separator = ",", string keyValueSeparator = "=", int? minLengthForEachKey = null, string naString = "N/A",bool isLastLine = false, List<string> fieldsToExclude = null)
		{
			bool exclude = fieldsToExclude != null && fieldsToExclude.Contains(key);

			if (exclude)
			{
				return "";
			}
			else
			{
				string line = string.Format("{0}{1}{2}{3}",
					minLengthForEachKey == null ? key : key.PadRight(minLengthForEachKey.Value),
					keyValueSeparator,
					value.ToNonNullString().Length==0?naString:value,
					isLastLine ? "" : separator
					);
				return line;
			}
		}

		public string BuildVisitorHash()
		{
			string r = this.ToString(",", "=", null, "N/A", new List<string>() { "VisitorHash", "Referrer" });
			r = r.GetHashAsHexStringUsingMD5Algorithm();
			return r;
		}
	}

	internal class VisitorInfoHelper
	{
		public enum HttpField
		{
			Hostname,
			IP,
			ProxyIP,
			UserAgent,
			Language,
			Platform,
			BrowserInfo,
			LogonUser,
			Cpu,
			Referer
		}
		//public static Dictionary<HttpField, string> CollectVisitorInfo(HttpRequestBase c)
		//{
		//	Dictionary<HttpField, string> info = new Dictionary<HttpField, string>();

		//	string proxyIp = String.Empty;
		//	string strIp;
		//	strIp = c.ServerVariables["HTTP_X_FORWARDED_FOR"];
		//	if (strIp == null)
		//	{
		//		strIp = c.ServerVariables["REMOTE_ADDR"];
		//	}
		//	else
		//	{
		//		proxyIp = c.ServerVariables["REMOTE_ADDR"];
		//	}

		//	info[HttpField.Hostname] = c.ServerVariables["REMOTE_HOST"];
		//	info[HttpField.LogonUser] = c.ServerVariables["LOGON_USER"];
		//	info[HttpField.Cpu] = c.ServerVariables["HTTP_UA_CPU"];
		//	info[HttpField.Referer] = c.ServerVariables["HTTP_REFERER"];
		//	info[HttpField.ProxyIP] = proxyIp;
		//	info[HttpField.IP] = strIp;
		//	info[HttpField.UserAgent] = c.ServerVariables["HTTP_USER_AGENT"];
		//	info[HttpField.Language] = c.ServerVariables["HTTP_ACCEPT_LANGUAGE"];
		//	info[HttpField.Platform] = c.Browser.Platform;
		//	info[HttpField.BrowserInfo] = c.Browser.Type + " " + c.Browser.Version;
		//	return info;
		//}
		public static Dictionary<HttpField, string> CollectVisitorInfo(HttpRequest c)
		{
			Dictionary<HttpField, string> info = new Dictionary<HttpField, string>();

			string proxyIp = String.Empty;
			string strIp;
			strIp = c.Headers["HTTP_X_FORWARDED_FOR"];
			if (strIp == null)
			{
				strIp = c.Headers["REMOTE_ADDR"];
			}
			else
			{
				proxyIp = c.Headers["REMOTE_ADDR"];
			}

			info[HttpField.Hostname] = c.Headers["REMOTE_HOST"];
			info[HttpField.LogonUser] = c.Headers["LOGON_USER"];
			info[HttpField.Cpu] = c.Headers["HTTP_UA_CPU"];
			info[HttpField.Referer] = c.Headers["HTTP_REFERER"];
			info[HttpField.ProxyIP] = proxyIp;
			info[HttpField.IP] = strIp;
			info[HttpField.UserAgent] = c.Headers["HTTP_USER_AGENT"];
			if (string.IsNullOrEmpty(info[HttpField.UserAgent]))
			{
				info[HttpField.UserAgent] = c.Headers["User-Agent"];
			}
			info[HttpField.Language] = c.Headers["HTTP_ACCEPT_LANGUAGE"];
			info[HttpField.Platform] = "//TODO: port to DotNet6";//c.Browser.Platform;
			info[HttpField.BrowserInfo] = "//TODO: port to DotNet6";// c.Browser.Type + " " + c.Browser.Version;
			return info;
		}
		public static long IPv4ToInt(string addr)
		{
			// careful of sign extension: convert to uint first;
			// unsigned NetworkToHostOrder ought to be provided.
			IPAddress ipAddress = null;
			bool isValidIp = System.Net.IPAddress.TryParse(addr, out ipAddress);
			if (isValidIp)
			{
				switch (ipAddress.AddressFamily)
				{
					case System.Net.Sockets.AddressFamily.InterNetwork:
						return (long)(uint)IPAddress.NetworkToHostOrder((int)ipAddress.Address);
					// we have IPv4                        
					case System.Net.Sockets.AddressFamily.InterNetworkV6:
						return -1;
					// we have IPv6
					default:
						return -2; //fuck off

				}
			}
			return -2;
		}

		public static string ToAddr(long address)
		{
			return IPAddress.Parse(address.ToString()).ToString();
			// This also works:
			// return new IPAddress((uint) IPAddress.HostToNetworkOrder(
			//    (int) address)).ToString();
		}
		//public static string GetTraceInfo(Page p)
		//{
		//	return GetTraceInfo(p, true, true, true, true);
		//}
		//public static string GetTraceInfo(HttpContext p)
		//{
		//	return p != null ? GetTraceInfo(p, true, true, true, true) : null;
		//}
		//public static string GetTraceInfo(HttpContext p, bool request, bool session, bool cookies, bool browser)
		//{
		//	if (p.Request == null) return null;

		//	StringBuilder builder = new StringBuilder();
		//	if (browser)
		//		builder.Append(DBrowserString(p.Request));
		//	if (request)
		//		builder.Append(DRequestString(p.Request));
		//	if (session)
		//		builder.Append(DSessionString(p.Session));
		//	if (cookies)
		//		builder.Append(DCookiesString(p.Request));


		//	return builder.ToString();
		//}
		//public static string GetTraceInfo(Page p, bool request, bool session, bool cookies, bool browser)
		//{
		//	StringBuilder builder = new StringBuilder();
		//	if (browser)
		//		builder.Append(DBrowserString(p.Request));
		//	if (request)
		//		builder.Append(DRequestString(p.Request));
		//	if (session)
		//		builder.Append(DSessionString(p.Session));
		//	if (cookies)
		//		builder.Append(DCookiesString(p.Request));


		//	return builder.ToString();
		//}
		////protected void DSessionUser(bool stop, string userstring)
		////{
		////    if (ConfigurationManager.AppSettings["Debug"] != "true")
		////        return;

		////    if (Session[userstring] == null)
		////        builder.Append("<BR>Session[\"" + userstring + "\"]=null;");
		////    else
		////    {
		////        Person p = (Person)Session[userstring];
		////        builder.Append("<BR>Session[\"" + userstring + "\"] contains:");
		////        builder.Append("<ul><li> User name = " + p.Name + "</li>");
		////        builder.Append("<li> User ntlogin = " + p.NTLogin + "</li>");
		////        builder.Append("<li> User uid = " + p.Uid + "</li>");
		////        builder.Append("<li> Options: <ul>");
		////        builder.Append("<li> Registered = " + p.Registered + "</li>");
		////        builder.Append("<li> Transport = " + p.TransportLocation + "</li>");
		////        builder.Append("<li> Song = " + p.VotedSong + "</li>");
		////        builder.Append("<li> KaraokeSong = " + p.KaraokeSong + "</li>");
		////        builder.Append("<li> KaraokeWish = " + p.KaraokeWish + "</li>");
		////        builder.Append("<li> ArtistName = " + p.ArtistName + "</li>");
		////        builder.Append("<li> Karaoke image = " + p.KaraokeImage + "</li>");
		////        builder.Append("<li> Karaoke image ratio = " + p.KaraokeImageRatio + "</li>");
		////        builder.Append("</ul></li></ul><br />");

		////    }
		////}
		////protected string DSessionUserString(string userkey)
		////{

		////    string retstr = String.Empty;

		////    if (Session[userkey] == null)
		////        retstr = "\nSession[\"" + userkey + "\"]=null;\n";
		////    else
		////    {
		////        Person p = (Person)Session[userkey];
		////        retstr = "\nSession[\"" + userkey + "\"] contains:\n";
		////        builder.Append("User name = " + p.Name + "\n");
		////        builder.Append("User ntlogin = " + p.NTLogin + "\n");
		////        builder.Append("User uid = " + p.Uid + "\n");
		////        builder.Append("Options:\n");
		////        builder.Append("\tRegistered = " + p.Registered + "\n");
		////        builder.Append("\tTransport = " + p.TransportLocation + "\n");
		////        builder.Append("\tSong = " + p.VotedSong + "\n");
		////        builder.Append("\tKaraokeSong = " + p.KaraokeSong + "\n");
		////        builder.Append("\tKaraokeWish = " + p.KaraokeWish + "\n");
		////        builder.Append("\tArtistName = " + p.ArtistName + "\n");
		////        builder.Append("\tColor = " + p.FavouriteColor + "\n");
		////        builder.Append("\tImage = " + p.KaraokeImage + "\n");
		////        builder.Append("\tRatio = " + p.KaraokeImageRatio + "\n");
		////    }
		////    return retstr;
		////}
		////protected void DSession(bool stop)
		////{
		////    if (ConfigurationManager.AppSettings["Debug"] != "true")
		////        return;

		////    builder.Append("<BR>DEBUG:<B>Session :</B><BR>");
		////    builder.Append("<UL><LI> SessionID " + session.SessionID + "</LI>");
		////    builder.Append("<LI> Session Timeout " + session.Timeout + "</LI>");
		////    builder.Append("<LI> Session Mode " + session.Mode + "</LI>");
		////    builder.Append("<LI> Session Locale " + session.LCID + "</LI>");
		////    builder.Append("<LI> Session is New " + session.IsNewSession + "</LI>");
		////    builder.Append("<LI> Session is cookiless " + session.IsCookieless + "</LI>");
		////    builder.Append("<LI> Session Key Count " + session.Count + "<UL>");
		////    foreach (string key in session.Keys)
		////    {
		////        if (Session[key] != null)
		////            builder.Append("<LI> Key " + key + " = -" + Session[key].ToString() + "-</LI>");
		////        else
		////            builder.Append("<LI> Key " + key + " = -null-</LI>");
		////    }
		////    builder.Append("</UL></LI></UL><BR>");


		////    if (stop) Response.End();
		////}
		//public static string DExceptionString(Exception e)
		//{
		//	StringBuilder builder = new StringBuilder();

		//	builder.Append("\n" + e.Message + "\n");
		//	builder.Append(e.StackTrace + "\n\n");
		//	builder.Append("InnerException:\n" + e.InnerException + "\n\n");
		//	builder.Append("Source:\n" + e.Source + "\n\n");

		//	return builder.ToString();
		//}

		//public static string DSessionString(HttpSessionState session)
		//{
		//	if (session == null) return null;

		//	StringBuilder builder = new StringBuilder();

		//	builder.Append("\nSession's contents:\n");
		//	builder.Append("SessionID " + session.SessionID + "\n");
		//	builder.Append(" Session Timeout " + session.Timeout + "\n");
		//	builder.Append(" Session Mode " + session.Mode + "\n");
		//	builder.Append(" Session Locale " + session.LCID + "\n");
		//	builder.Append(" Session is New " + session.IsNewSession + "\n");
		//	builder.Append(" Session is cookiless " + session.IsCookieless + "\n");
		//	builder.Append(" Session Key Count " + session.Count + "\n");
		//	foreach (string key in session.Keys)
		//	{
		//		if (session[key] != null)
		//			builder.Append(" Key " + key + " = -" + session[key].ToString() + "-\n");
		//		else
		//			builder.Append(" Key " + key + " = -null-\n");
		//	}
		//	builder.Append("\n");


		//	return builder.ToString();
		//}
		////protected string DRequest(Page page)
		////{
		////    StringBuilder builder = new StringBuilder();
		////    builder.Append("<BR><B>Request :</B><BR>");
		////    builder.Append("<UL><LI> Application Path " + page.request.ApplicationPath + "</LI>");
		////    builder.Append("<LI> Application Relative To Current Execution Path " + page.request.AppRelativeCurrentExecutionFilePath + "</LI>");
		////    builder.Append("<LI> Current Execution File Path " + page.request.CurrentExecutionFilePath + "</LI>");
		////    builder.Append("<LI> Physical Path: " + page.request.PhysicalPath + "</LI>");
		////    builder.Append("<LI> Physical App Path: " + page.request.PhysicalApplicationPath + "</LI>");
		////    builder.Append("<LI> Path: " + page.request.Path + "</LI>");
		////    builder.Append("<LI> Path Info: " + page.request.PathInfo + "</LI>");
		////    builder.Append("<LI> RawURL: " + page.request.RawUrl + "</LI>");
		////    builder.Append("<LI> Query string: " + page.request.QueryString + "</LI>");
		////    builder.Append("<LI> Encoding " + page.request.ContentEncoding.EncodingName + "</LI>");
		////    builder.Append("<LI> Content Length " + page.request.ContentLength + "</LI>");
		////    builder.Append("<LI> Content Type " + page.request.ContentType + "</LI>");
		////    builder.Append("<LI> Virtual Path of current request " + page.request.FilePath + "</LI>");
		////    builder.Append("<LI> Files :<UL>");
		////    foreach (string key in page.request.Files.Keys)
		////    {
		////        if (page.request.Files[key] != null)
		////            builder.Append("<LI> " + key + " = -" + page.request.Files[key].FileName + "-</LI>");
		////        else
		////            builder.Append("<LI> " + key + " = -null-</LI>");

		////    }
		////    builder.Append("</UL></LI><LI> Form :<UL>");
		////    foreach (string key in page.request.Form.Keys)
		////    {
		////        if (page.request.Form[key] != null)
		////            builder.Append("<LI> " + key + " = -" + page.request.Form[key] + "-</LI>");
		////        else
		////            builder.Append("<LI> " + key + " = -null-</LI>");

		////    }
		////    builder.Append("</UL></LI><LI> Headers :<UL>");
		////    foreach (string key in page.request.Headers.Keys)
		////    {
		////        if (page.request.Headers[key] != null)
		////            builder.Append("<LI> " + key + " = -" + page.request.Headers[key] + "-</LI>");
		////        else
		////            builder.Append("<LI> " + key + " = -null-</LI>");

		////    }
		////    builder.Append("<LI> Http Method: " + page.request.HttpMethod + "</LI>");
		////    builder.Append("<LI> IS Authenticated: " + page.request.IsAuthenticated + "</LI>");
		////    builder.Append("<LI> IS Local: " + page.request.IsLocal + "</LI>");
		////    builder.Append("<LI> Logon User:<UL>");
		////    builder.Append("<LI> Authentication type: " + page.request.LogonUserIdentity.AuthenticationType + "</LI>");
		////    builder.Append("<LI> Annonimus: " + page.request.LogonUserIdentity.IsAnonymous + "</LI>");
		////    builder.Append("<LI> Authenticated: " + page.request.LogonUserIdentity.IsAuthenticated + "</LI>");
		////    builder.Append("<LI> Guest: " + page.request.LogonUserIdentity.IsGuest + "</LI>");
		////    builder.Append("<LI> Is System: " + page.request.LogonUserIdentity.IsSystem + "</LI>");
		////    builder.Append("<LI> Name: " + page.request.LogonUserIdentity.Name + "</LI>");
		////    builder.Append("</UL></LI></UL>");

		////    return builder.ToString();

		////}
		//public static XElement XRequest(HttpRequestBase request)
		//{
		//	XElement files = new XElement("files");
		//	XElement form = new XElement("form");
		//	XElement headers = new XElement("headers");

		//	if (request.Files != null && request.Files.Keys != null && request.Files.Keys.Count > 0)
		//	{
		//		foreach (string key in request.Files.Keys)
		//		{
		//			HttpPostedFileBase f = request.Files[key];
		//			if (f != null)
		//			{
		//				files.Add(new XElement("file",
		//					new XElement("name", f.FileName),
		//					new XElement("type", f.ContentType),
		//					new XElement("bytes", f.ContentLength))
		//					);
		//			}
		//		}
		//	}

		//	if (request.Form != null && request.Form.Keys != null && request.Form.Keys.Count > 0)
		//	{
		//		foreach (string key in request.Form.Keys)
		//		{
		//			form.Add(new XElement("parameter",
		//					key + " = " + request.Form[key]));
		//		}
		//	}

		//	if (request.Headers != null && request.Headers.Keys != null && request.Headers.Keys.Count > 0)
		//	{
		//		foreach (string key in request.Headers.Keys)
		//		{
		//			if (key.ToUpper() != "cookie")
		//				headers.Add(new XElement("parameter",
		//						key + " = " + request.Headers[key]));
		//		}
		//	}

		//	XElement xr = new XElement("request",
		//		new XElement("rawurl", request.RawUrl),
		//		files,
		//		form,
		//		headers
		//		);


		//	foreach (string key in request.Params.AllKeys)
		//		xr.Add(new XElement("param", "Request[" + key + "] = " + request[key]));

		//	return xr;
		//}
		//public static XElement XRequest(HttpRequest request)
		//{
		//	XElement files = new XElement("files");
		//	XElement form = new XElement("form");
		//	XElement headers = new XElement("headers");

		//	if (request.Files != null && request.Files.Keys != null && request.Files.Keys.Count > 0)
		//	{
		//		foreach (string key in request.Files.Keys)
		//		{
		//			HttpPostedFile f = request.Files[key];
		//			if (f != null)
		//			{
		//				files.Add(new XElement("file",
		//					new XElement("name", f.FileName),
		//					new XElement("type", f.ContentType),
		//					new XElement("bytes", f.ContentLength))
		//					);
		//			}
		//		}
		//	}

		//	if (request.Form != null && request.Form.Keys != null && request.Form.Keys.Count > 0)
		//	{
		//		foreach (string key in request.Form.Keys)
		//		{
		//			form.Add(new XElement("parameter",
		//					key + " = " + request.Form[key]));
		//		}
		//	}

		//	if (request.Headers != null && request.Headers.Keys != null && request.Headers.Keys.Count > 0)
		//	{
		//		foreach (string key in request.Headers.Keys)
		//		{
		//			if (key.ToUpper() != "cookie")
		//				headers.Add(new XElement("parameter",
		//						key + " = " + request.Headers[key]));
		//		}
		//	}

		//	XElement xr = new XElement("request",
		//		new XElement("rawurl", request.RawUrl),
		//		files,
		//		form,
		//		headers
		//		);


		//	foreach (string key in request.Params.AllKeys)
		//		xr.Add(new XElement("param", "Request[" + key + "] = " + request[key]));

		//	return xr;
		//}
		//public static string DRequestString(HttpRequest request)
		//{
		//	if (request == null) return null;

		//	StringBuilder builder = new StringBuilder();


		//	builder.Append("Request :\n");
		//	//builder.Append(" Application Path " + request.ApplicationPath + "\n");
		//	//builder.Append(" Application Relative To Current Execution Path " + request.AppRelativeCurrentExecutionFilePath + "\n");

		//	//builder.Append(" Physical Path: " + request.PhysicalPath + "\n");
		//	//builder.Append(" Physical App Path: " + request.PhysicalApplicationPath + "\n");
		//	//builder.Append(" Path: " + request.Path + "\n");
		//	//builder.Append(" Path Info: " + request.PathInfo + "\n");
		//	builder.Append(" RawURL: " + request.RawUrl + "\n");
		//	builder.Append(" Query string: " + request.QueryString + "\n");
		//	builder.Append(" Current Execution File Path " + request.CurrentExecutionFilePath + "\n");
		//	builder.Append(" Encoding " + request.ContentEncoding.EncodingName + "\n");
		//	builder.Append(" Content Length " + request.ContentLength + "\n");
		//	builder.Append(" Content Type " + request.ContentType + "\n");
		//	//builder.Append(" Virtual Path of current request " + request.FilePath + "\n");
		//	builder.Append(" Files:\n");
		//	foreach (string key in request.Files.Keys)
		//	{
		//		if (request.Files[key] != null)
		//			builder.Append("\t" + key + " = -" + request.Files[key].FileName + "-\n");
		//		else
		//			builder.Append("\t" + key + " = -null-\n");

		//	}
		//	builder.Append("Form:\n");
		//	foreach (string key in request.Form.Keys)
		//	{
		//		if (request.Form[key] != null)
		//			builder.Append("\t" + key + " = -" + request.Form[key] + "-\n");
		//		else
		//			builder.Append("\t" + key + " = -null-\n");

		//	}
		//	builder.Append("Headers:\n");
		//	foreach (string key in request.Headers.Keys)
		//	{
		//		if (request.Headers[key] != null)
		//			builder.Append("\t" + key + " = -" + request.Headers[key] + "-\n");
		//		else
		//			builder.Append("\t" + key + " = -null-\n");

		//	}
		//	builder.Append(" Http Method: " + request.HttpMethod + "\n");
		//	builder.Append(" IS request Authenticated: " + request.IsAuthenticated + "\n");
		//	//builder.Append(" IS Local: " + request.IsLocal + "\n");
		//	builder.Append(" Logon User:\n");
		//	builder.Append("\tAuthentication type: " + request.LogonUserIdentity.AuthenticationType + "\n");
		//	//builder.Append("\tIs Anonimus: " + request.LogonUserIdentity.IsAnonymous + "\n");
		//	builder.Append("\tAuthenticated: " + request.LogonUserIdentity.IsAuthenticated + "\n");
		//	//builder.Append("\tGuest: " + request.LogonUserIdentity.IsGuest + "\n");
		//	//builder.Append("\tIs System: " + request.LogonUserIdentity.IsSystem + "\n");
		//	builder.Append("\tName: " + request.LogonUserIdentity.Name + "\n");
		//	builder.Append("\n");


		//	return builder.ToString();
		//}
		////protected void DBrowser(bool stop)
		////{
		////    if (ConfigurationManager.AppSettings["Debug"] != "true")
		////        return;

		////    builder.Append("<BR><B>Browser Info :</B><BR>");
		////    builder.Append("<UL><LI> ActiveX " + request.Browser.ActiveXControls + "</LI>");
		////    builder.Append("<LI> Background Sounds " + request.Browser.BackgroundSounds + "</LI>");
		////    builder.Append("<LI> Beta version " + request.Browser.Beta + "</LI>");
		////    builder.Append("<LI> User-Agent Header " + request.Browser.Browser + "</LI>");
		////    builder.Append("<LI> Can Send mail " + request.Browser.CanSendMail + "</LI>");
		////    builder.Append("<LI> Capabilities :<UL>");
		////    foreach (string key in request.Browser.Capabilities.Keys)
		////    {
		////        if (request.Browser.Capabilities[key] != null)
		////            builder.Append("<LI> " + key + " = -" + request.Browser.Capabilities[key] + "-</LI>");
		////        else
		////            builder.Append("<LI> " + key + " = -null-</LI>");

		////    }
		////    builder.Append("</UL></LI><LI> Cookies support " + request.Browser.Cookies + "</LI>");
		////    builder.Append("</UL><BR>");


		////    if (stop) Response.End();
		////}
		//public static string DBrowserString(HttpRequest request)
		//{
		//	StringBuilder builder = new StringBuilder();

		//	builder.Append("Browser Info :\n");
		//	builder.Append(" User-Agent Header " + request.Browser.Browser + "\n");
		//	builder.Append("Cookies support " + request.Browser.Cookies + "\n");
		//	builder.Append(" Beta version " + request.Browser.Beta + "\n");
		//	builder.Append("ActiveX " + request.Browser.ActiveXControls + "\n");
		//	//builder.Append(" Background Sounds " + request.Browser.BackgroundSounds + "\n");

		//	//builder.Append(" Can Send mail " + request.Browser.CanSendMail + "\n");
		//	//builder.Append(" Capabilities :\n");
		//	//foreach (string key in request.Browser.Capabilities.Keys)
		//	//{
		//	//    if (request.Browser.Capabilities[key] != null)
		//	//        builder.Append("\t" + key + " = -" + request.Browser.Capabilities[key] + "-\n");
		//	//    else
		//	//        builder.Append("\t" + key + " = -null-\n");

		//	//}
		//	builder.Append("\n");


		//	return builder.ToString();
		//}

		//public static XElement XBrowser(HttpRequestBase request)
		//{

		//	return new XElement("browser",
		//				new XElement("info", request.Browser.Browser),
		//				new XElement("ismobile", request.Browser.IsMobileDevice),
		//				new XElement("js", request.Browser.EcmaScriptVersion),
		//				new XElement("cookies", request.Browser.Cookies),
		//				new XElement("activex", request.Browser.ActiveXControls)
		//				);

		//}

		//public static XElement XBrowser(HttpRequest request)
		//{

		//	return new XElement("browser",
		//				new XElement("info", request.Browser.Browser),
		//				new XElement("ismobile", request.Browser.IsMobileDevice),
		//				new XElement("js", request.Browser.EcmaScriptVersion),
		//				new XElement("cookies", request.Browser.Cookies),
		//				new XElement("activex", request.Browser.ActiveXControls)
		//				);

		//}
		//public static XElement XException(Exception x)
		//{
		//	if (x == null)
		//		return new XElement("Exception", null);

		//	return new XElement("Exception",
		//				new XElement("Message", x.Message.Replace("'", "\"")),
		//				new XElement("StackTrace", x.StackTrace));
		//}
		//public static XElement XCookies(HttpRequestBase request)
		//{
		//	XElement cookieCollection = new XElement("collection");
		//	try
		//	{
		//		if (request.Cookies.Count > 0)
		//			for (int i = 0; i < request.Cookies.Count; i++)
		//			{
		//				XElement keys = new XElement("keys");
		//				if (request.Cookies[i].HasKeys &&
		//					request.Cookies[i].Values != null &&
		//					request.Cookies[i].Values.Keys != null &&
		//					request.Cookies[i].Values.Keys.Count > 0)
		//				{
		//					foreach (string key in request.Cookies[i].Values.Keys)
		//					{
		//						if (request.Cookies[i][key] != null)
		//							keys.Add(new XElement("key", key + " = " + request.Cookies[i][key]));
		//						else
		//							keys.Add(new XElement("key", key + " = null"));
		//					}
		//				}
		//				cookieCollection.Add(new XElement("cookie",
		//					new XElement("name", request.Cookies[i].Name),
		//					new XElement("domain", request.Cookies[i].Domain),
		//					new XElement("expires", request.Cookies[i].Expires.ToString()),
		//					new XElement("clientSideAccessible", request.Cookies[i].HttpOnly),
		//					new XElement("path", request.Cookies[i].Path),
		//					new XElement("secure", request.Cookies[i].Secure),
		//					new XElement("secure", request.Cookies[i].Secure),
		//					keys
		//					));
		//			}//foreach cookie
		//	}
		//	catch (Exception e)
		//	{
		//		cookieCollection.Add(new XElement("EXCEPTION", e != null ? e.Message : "null"),
		//			new XElement("STACKTRACE", e != null ? e.StackTrace : "null"));
		//	}


		//	XElement cookies = new XElement("cookies",
		//					new XElement("count", request.Cookies.Count),
		//					cookieCollection
		//					);

		//	return cookies;
		//}

		//public static XElement XCookies(HttpRequest request)
		//{
		//	XElement cookieCollection = new XElement("collection");

		//	try
		//	{
		//		if (request.Cookies.Count > 0)
		//			for (int i = 0; i < request.Cookies.Count; i++)
		//			{
		//				XElement keys = new XElement("keys");
		//				if (request.Cookies[i].HasKeys &&
		//					request.Cookies[i].Values != null &&
		//					request.Cookies[i].Values.Keys != null &&
		//					request.Cookies[i].Values.Keys.Count > 0)
		//				{
		//					foreach (string key in request.Cookies[i].Values.Keys)
		//					{
		//						if (request.Cookies[i][key] != null)
		//							keys.Add(new XElement("key", key + " = " + request.Cookies[i][key]));
		//						else
		//							keys.Add(new XElement("key", key + " = null"));
		//					}
		//				}
		//				cookieCollection.Add(new XElement("cookie",
		//					new XElement("name", request.Cookies[i].Name),
		//					new XElement("domain", request.Cookies[i].Domain),
		//					new XElement("expires", request.Cookies[i].Expires.ToString()),
		//					new XElement("clientSideAccessible", request.Cookies[i].HttpOnly),
		//					new XElement("path", request.Cookies[i].Path),
		//					new XElement("secure", request.Cookies[i].Secure),
		//					new XElement("secure", request.Cookies[i].Secure),
		//					keys
		//					));
		//			}//foreach cookie
		//	}
		//	catch (Exception e)
		//	{
		//		cookieCollection.Add(new XElement("EXCEPTION", e != null ? e.Message : "null"),
		//			new XElement("STACKTRACE", e != null ? e.StackTrace : "null"));
		//	}


		//	XElement cookies = new XElement("cookies",
		//					new XElement("count", request.Cookies.Count),
		//					cookieCollection
		//					);

		//	return cookies;
		//}

		//public static string DCookiesString(HttpRequest request)
		//{
		//	StringBuilder builder = new StringBuilder();
		//	builder.Append("\nCookies Info :\n\n");
		//	builder.Append("\t Cookies count " + request.Cookies.Count + " : \n");
		//	for (int i = 0; i < request.Cookies.Count; i++)
		//	{
		//		builder.Append("\t");
		//		builder.Append(DCookie(request, i));
		//		builder.Append("\n");

		//	}
		//	builder.Append("\n\n");

		//	return builder.ToString();
		//}
		//public static string DCookie(HttpRequest request, int inx)
		//{
		//	StringBuilder builder = new StringBuilder();
		//	if (request.Cookies[inx] == null)
		//	{
		//		builder.Append("\tNull cookie.\n");
		//	}
		//	else
		//	{

		//		builder.Append("\nCookie name: -" + request.Cookies[inx].Name + "- \n");
		//		builder.Append("\tDomain: " + request.Cookies[inx].Domain + "\n");
		//		builder.Append("\t Expires: " + request.Cookies[inx].Expires.ToShortDateString() + "\n");
		//		builder.Append("\t HasKeys: " + request.Cookies[inx].HasKeys + "\n");
		//		builder.Append("\t ClientSideAccessible: " + request.Cookies[inx].HttpOnly + "\n");
		//		builder.Append("\t Path: " + request.Cookies[inx].Path + "\n");
		//		builder.Append("\t Secure: " + request.Cookies[inx].Secure + "\n");
		//		builder.Append("\t Value: " + request.Cookies[inx].Value + "\n");
		//		builder.Append("\t Keys :\n");
		//		foreach (string key in request.Cookies[inx].Values.Keys)
		//		{
		//			if (request.Cookies[inx][key] != null)
		//				builder.Append("\t\t " + key + " = -" + request.Cookies[inx][key] + "-\n");
		//			else
		//				builder.Append("\t\t " + key + " = -null-\n");

		//		}
		//		builder.Append("\n\n");

		//	}
		//	return builder.ToString();
		//}
		////protected void DApplication(bool stop)
		////{
		////    if (ConfigurationManager.AppSettings["Debug"] != "true")
		////        return;

		////    builder.Append("<BR><B>Application state info: </B><BR>");
		////    builder.Append("<UL><LI> Number Of objects: " + Application.Count + "</LI>");
		////    builder.Append("<LI> Objects :<UL>");
		////    foreach (string key in Application.Keys)
		////    {
		////        if (Application[key] != null)
		////            builder.Append("<LI> Application[" + key + "] = -" + Application[key] + "-</LI>");
		////        else
		////            builder.Append("<LI> Application[" + key + "] = -null-</LI>");

		////    }
		////    builder.Append("</UL></LI></UL>");

		////    if (stop) Response.End();
		////}
	}
}