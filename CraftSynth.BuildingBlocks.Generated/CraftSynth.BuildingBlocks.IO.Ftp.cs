using CBB_IOFtpPath = CraftSynth.BuildingBlocks.IO.FtpPath;
using CBB_LoggingCustomTraceLog = CraftSynth.BuildingBlocks.Logging.CustomTraceLog;

namespace CraftSynth.BuildingBlocks.IO;

public class Ftp
{
	/// <summary>
	/// Splits 'dir\dir2\dir3\' to list
	/// </summary>
	/// <param name="remoteDir"></param>
	/// <returns></returns>
	public static List<string> FtpRemoteDirs(string remoteDir)
	{
		var ftpRemoteDirs = new List<string>();
		//try
		//{
		string[] dirNames = remoteDir.Split('\\');
		int i = 0;
		while(i < dirNames.Length)
		{
			ftpRemoteDirs.Add(dirNames[i]);
			i++;
		}

		//}
		//catch (Exception exception)
		//{
		//    ExceptionHandler.Handle(exception, ExceptionHandlingPolicies.DAL_Wrap_Policy);
		//}
		return ftpRemoteDirs;
	}

	/// <summary>
	/// Checks if file exist on ftp server.
	/// </summary>
	/// <returns></returns>
	public static bool FileExists(CBB_IOFtpPath ftpPath, CBB_LoggingCustomTraceLog log)
	{
		bool exist = false;

		var ftpClient = new FtpClient();
		ftpClient.setDebug(false);
		ftpClient.setRemoteHost(ftpPath.Authority);
		if(ftpPath.Port.HasValue)
		{
			ftpClient.setRemotePort(ftpPath.Port.Value);
		}
		ftpClient.setRemoteUser(ftpPath.username);
		ftpClient.setRemotePass(ftpPath.password);

		ftpClient.login(log);
		foreach(string ftpRemoteDir in ftpPath.Subfolders)
		{
			ftpClient.chdir(ftpRemoteDir, log);
		}
		ftpClient.setBinaryMode(true);
		string[] fileList = ftpClient.getFileList(ftpPath.FileName, log);
		if(fileList != null)
		{
			exist = fileList.Any(f => string.Compare(ftpPath.FileName, f, StringComparison.OrdinalIgnoreCase) == 0);
		}

		ftpClient.close(log);

		return exist;
	}

	/// <summary>
	/// Uploads specified file to remote directory on ftp server.
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="hostName"></param>
	/// <param name="port">Pass null for default</param>
	/// <param name="remoteDir"></param>
	/// <param name="userName"></param>
	/// <param name="password"></param>
	/// <returns></returns>
	public static bool UploadFile(string filePath, string hostName, int? port, string remoteDir, string userName, string password, CBB_LoggingCustomTraceLog log)
	{
		bool ok = false;
		//try
		//{
		var ftpClient = new FtpClient();
		ftpClient.setDebug(false);
		ftpClient.setRemoteHost(hostName);
		if(port.HasValue)
		{
			ftpClient.setRemotePort(port.Value);
		}
		ftpClient.setRemoteUser(userName);
		ftpClient.setRemotePass(password);

		ftpClient.login(log);
		foreach(string ftpRemoteDir in FtpRemoteDirs(remoteDir))
		{
			ftpClient.chdir(ftpRemoteDir, log);
		}
		ftpClient.setBinaryMode(true);
		ftpClient.upload(filePath, true, log);
		ftpClient.close(log);
		ok = true;

		//}
		//catch (Exception exception)
		//{
		//    ExceptionHandler.Handle(exception, ExceptionHandlingPolicies.DAL_Wrap_Policy);
		//}

		return ok;
	}
}
