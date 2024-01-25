using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Threading;

using CBB_ExtenderClass = CraftSynth.BuildingBlocks.Common.ExtenderClass;
using CBB_DateAndTime = CraftSynth.BuildingBlocks.Common.DateAndTime;
using CBB_Encryption = CraftSynth.BuildingBlocks.Encryption;
//TODO: port to DotNet6
//using CBB_WindowsNT = CraftSynth.BuildingBlocks.WindowsNT;


namespace CraftSynth.BuildingBlocks.IO.Http
{
	public enum UsagePingerAction
	{
		ProgramInstalled,
		AboutRequested
	}

	public class UsagePinger
    {
		//TODO: port to DotNet6


		//public static bool TryToSendUsagePing(string application, string applicationVersion, string destinationUrl, UsagePingerAction action, int timeoutInMilliseconds = 4000)
		//{
		//	bool r = false;

		//	//string info = string.Format("SyntaxVersion={0}|Application={1}|Version={2}|Action={3}|MachineName={4}|UserDomainName={5}|OSVersion.VersionString={6}|Is64BitOperatingSystem={7}|ProcessorCount={8}//|RAM={9}",
		//	string info = string.Format("SyntaxVersion={0}|Application={1}|Version={2}|Action={3}|MachineName={4}|UserDomainName={5}|OSVersion.VersionString={6}|ProcessorCount={7}",
		//		1,
		//		application,
		//		applicationVersion,
		//		action.ToString(),
		//		System.Environment.MachineName ?? "N/A",
		//		System.Environment.UserDomainName ?? ("N/A"),
		//		System.Environment.OSVersion == null ? "" : System.Environment.OSVersion.VersionString ?? "N/A",
		//		//CBB_WindowsNT.Misc.Is64BitOperatingSystem,
		//		System.Environment.ProcessorCount
		//		//Math.Round(new Microsoft.VisualBasic.Devices.Computer().Info.TotalPhysicalMemory / (double)1024 / 1024 / 1024, 2).ToString() + "Gb"
		//		);
		//	byte[] b = CraftSynth.BuildingBlocks.Encryption.ToBytesFromUnicodeString(info);
		//	//b = CraftSynth.BuildingBlocks.Encryption.EncryptWithRijndaelAlgorithm(b, VERSION_GUID);
		//	info = CBB_ExtenderClass.ToAlphaOnlyHex(b);

		//	//b = t.FromAlphaOnlyHex();
		//	////b = CraftSynth.BuildingBlocks.Encryption.DecryptWithRijndaelAlgorithm(b, VERSION_GUID);
		//	//t = CraftSynth.BuildingBlocks.Encryption.ToUnicodeStringFromBytes(b);

		//	Thread t = new Thread(() =>
		//	{
		//		try
		//		{
		//			string response = CraftSynth.BuildingBlocks.IO.Http.Misc.RequestUri(destinationUrl + "?usagePing=1", null, null, null, null, true, info, 2, null, false, null, false, null, true, null, null, false, true);
		//			r = true;
		//		}
		//		catch (Exception e)
		//		{					
		//		}
		//	});
		//	t.Start();
		//	if (!t.Join(timeoutInMilliseconds))
		//	{
		//		t.Abort();
		//	}

		//	return r;
		//}

		//public static bool HandleUsagePingIfPresent(HttpContext httpContext, string folderPath = null)
		//{
		//	bool isUsagePingPresent = HttpContext.Current.Request.QueryString["usagePing"] != null;

		//	folderPath = folderPath ?? HttpContext.Current.Server.MapPath("~");
		//	try
		//	{
		//		if (isUsagePingPresent)
		//		{					
		//			string usagePingBodyAsAlphaOnlyHex = GetDocumentContents(HttpContext.Current.Request);
		//			byte[] b = CBB_ExtenderClass.FromAlphaOnlyHex(usagePingBodyAsAlphaOnlyHex);
		//			string usagePingBody = CBB_Encryption.ToUnicodeStringFromBytes(b);


		//			//if (usagePingBody.Contains("SyntaxVersion") && usagePingBody.Contains("|Action="))
		//			//{
		//			//add timestamp and VisitorInfo fields
		//			var kvps = ParseUsagePingBody(usagePingBody);
		//			usagePingBody = string.Format("Moment={0}|{1}|{2}",
		//										  CBB_DateAndTime.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS(DateTime.Now),
		//										  usagePingBody,
		//										  new VisitorInfo(HttpContext.Current.Request).ToString("|", "=", null, "N/A")
		//										  );

		//			//add FullVisitorHash field
		//			kvps = ParseUsagePingBody(usagePingBody);
		//			string h = "";
		//			h += kvps["MachineName"];
		//			h += "|" + kvps["UserDomainName"];
		//			h += "|" + kvps["OSVersion.VersionString"];
		//			h += "|" + kvps["Is64BitOperatingSystem"];
		//			h += "|" + kvps["ProcessorCount"];
		//			h += "|" + kvps["VisitorHash"];
		//			h = CBB_Encryption.GetHashAsHexStringUsingMD5Algorithm(h);
		//			kvps.Add("FullVisitorHash", h);
		//			usagePingBody = "";
		//			foreach (var kvp in kvps)
		//			{
		//				usagePingBody += kvp.Key + "=" + kvp.Value + "|";
		//			}
		//			usagePingBody = usagePingBody.TrimEnd('|') + "\r\n";

		//			//append line to file
		//			string filePath = folderPath + @"\UsagePings_" + kvps["Application"] + "_" + kvps["Version"] + ".log";
		//			CraftSynth.BuildingBlocks.IO.FileSystem.CreateFileIfItDoesNotExist(filePath);
		//			CraftSynth.BuildingBlocks.IO.FileSystem.AppendFile(filePath, usagePingBody, CraftSynth.BuildingBlocks.IO.FileSystem.ConcurrencyProtectionMechanism.Mutex);
		//			//}
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		try
		//		{
		//			ex = CraftSynth.BuildingBlocks.Common.Misc.GetDeepestException(ex);
		//			string filePath = folderPath + @"\UsagePingErrors.log";
		//			CraftSynth.BuildingBlocks.IO.FileSystem.CreateFileIfItDoesNotExist(filePath);
		//			CraftSynth.BuildingBlocks.IO.FileSystem.AppendFile(filePath, "\r\n" + CraftSynth.BuildingBlocks.Common.DateAndTime.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS(DateTime.Now) + "\r\n" + ex.Message + "\r\n" + ex.StackTrace, CraftSynth.BuildingBlocks.IO.FileSystem.ConcurrencyProtectionMechanism.Mutex);
		//		}
		//		catch (Exception ex2) { }
		//	}

		//	return isUsagePingPresent;
		//}

		//private static string GetDocumentContents(System.Web.HttpRequest Request)
		//{
		//	string documentContents;
		//	using (Stream receiveStream = Request.InputStream)
		//	{
		//		using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
		//		{
		//			documentContents = readStream.ReadToEnd();
		//		}
		//	}
		//	return documentContents;
		//}

		//private static Dictionary<string, string> ParseUsagePingBody(string usagePingBody)
		//{
		//	Dictionary<string, string> r = new Dictionary<string, string>();
		//	foreach (string kvp in usagePingBody.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
		//	{
		//		r.Add(kvp.Substring(0, kvp.IndexOf('=')), kvp.Substring(kvp.IndexOf('=') + 1));
		//	}
		//	return r;
		//}
	}
}
