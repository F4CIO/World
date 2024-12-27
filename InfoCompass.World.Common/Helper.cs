using System.Net;

namespace MyCompany.World.Common;

public class Helper
{
	/// <summary>
	/// If you are about to call https url or url that redirects to https use this to support it by expecting ssl and skipping ssl certificate validation. 
	/// TODO: move to CraftSynth.BuildingBlocks.IO.Http.Misc.RequestUri(...) method once that project is upgraded to .net 4.5 where there is a support for Tls11 and Tls12
	/// </summary>
	public static void SupportHttpsRequestsAndSkipCertificateValidation()
	{
		//https://stackoverflow.com/questions/10822509/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;//support SSL/TLS
		ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };// Skip validation of SSL/TLS certificate
	}

	public static string DefaultToHttp(string url)
	{
		url = url.ToNonNullString().Trim().ToLower();
		url = url.Replace("https://", "http:");
		url = url.Replace("http://", "http:");

		url = url.Replace("http:", "http://");
		return url;
	}

	public static string CreateHashFromPassword(string password)
	{
		//when writing mda pass to file it makes crc error sometimes so we convert it to A-Z string
		byte[] userPasswordHashAsBytes = CraftSynth.BuildingBlocks.Encryption.GetHashUsingMD5Algorithm(password).ToBytes();
		string r = CraftSynth.BuildingBlocks.Common.ExtenderClass.ToAlphaOnlyHex(userPasswordHashAsBytes);
		return r;
	}
}
