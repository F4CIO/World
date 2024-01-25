using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CBB_LoggingCustomTraceLog = CraftSynth.BuildingBlocks.Logging.CustomTraceLog;
using CBB_LoggingCustomTraceLogExtensions = CraftSynth.BuildingBlocks.Logging.CustomTraceLogExtensions;
using CBB_CommonMisc = CraftSynth.BuildingBlocks.Common.Misc;
using CBB_CommonExtenderClass = CraftSynth.BuildingBlocks.Common.ExtenderClass;

namespace CraftSynth.BuildingBlocks.IO
{
	public class FtpPath
	{
		#region Private Members
		#endregion

		#region Properties	
		public string Scheme;
		public string Authority;
		public int? Port;
		public List<string> Subfolders;
		public string FileName;

		public string username;
		public string password;
		#endregion

		#region Public Methods

		/// <summary>
		/// Returns for example: ftp://ftp.mysite.com/subfolder1/subfolder2/file1.txt 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string r = string.Empty;

			r = r +this.Scheme + "://";

			r = r + Authority;

			if (this.Port != null)
			{
				r = r + ":" + this.Port;
			}

			foreach (string subfolder in this.Subfolders)
			{
				r = r + "/" + subfolder;
			}

			if (this.FileName != null)
			{
				r = r + "/" + FileName;
			}

			return r;
		}

		#endregion

		#region Constructors And Initialization
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ftpPath">
		/// Valid examples: 
		/// ftp.mysite.com
		/// ftp://ftp.mysite.com
		/// ftp://ftp.mysite.com:21
		/// ftp://ftp.mysite.com/file1.txt
		/// ftp://ftp.mysite.com\file1.txt
		/// ftp://ftp.mysite.com/subfolder1/subfolder2
		/// ftp://ftp.mysite.com/subfolder1/subfolder2/file1.txt
		/// ftp.mysite.com/subfolder1/subfolder2/file1.txt
		/// </param>
		public static bool IsValid(string ftpPath, bool isFilePath = true, CBB_LoggingCustomTraceLog log = null)
		{
			bool r;
			
			CBB_LoggingCustomTraceLogExtensions.AddLine(log, string.Format("Checking ftpPath '{0}'...", ftpPath));
			try
			{
				new FtpPath(ftpPath, isFilePath);
				CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Ok.", false);
				r = true;
			}
			catch (Exception e)
			{
				CBB_CommonMisc.GetDeepestException(e);
				CBB_LoggingCustomTraceLogExtensions.AddLine(log, "ftpPath Invalid. Example of valid path: ftp://ftp.mysite.com/subfolder1/subfolder2/file1.txt");
				CBB_LoggingCustomTraceLogExtensions.AddLine(log, e.Message);
				r = false;
			}
			
			return r;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="ftpPath">
		/// Valid examples: 
		/// ftp.mysite.com
		/// ftp://ftp.mysite.com
		/// ftp://ftp.mysite.com:21
		/// ftp://ftp.mysite.com/file1.txt
		/// ftp://ftp.mysite.com\file1.txt
		/// ftp://ftp.mysite.com/subfolder1/subfolder2
		/// ftp://ftp.mysite.com/subfolder1/subfolder2/file1.txt
		/// ftp.mysite.com/subfolder1/subfolder2/file1.txt
		/// </param>
		public FtpPath(string ftpPath, bool isFilePath = true, string username = null, string password = null)
		{
			ftpPath = ftpPath.Trim().Replace('\\', '/').Trim('/');
			
			if (ftpPath.ToLower().StartsWith("ftp://"))
			{
				this.Scheme = "ftp";
				ftpPath = ftpPath.Remove(0, "ftp://".Length);
			}else if (ftpPath.ToLower().StartsWith("sftp://"))
			{
				this.Scheme = "sftp";
				ftpPath = ftpPath.Remove(0, "sftp://".Length);
			}
			else
			{
				this.Scheme = "ftp";
			}
			ftpPath = ftpPath.Trim('/');

			string authorityAndPort = ftpPath.Split('/')[0];
			if (authorityAndPort.Contains(':'))
			{
				this.Authority = authorityAndPort.Split(':')[0];
				this.Port = int.Parse(authorityAndPort.Split(':')[1]);
			}
			else
			{
				this.Authority = authorityAndPort;
				this.Port = null;
			}
			ftpPath = ftpPath.Remove(0, authorityAndPort.Length);
			ftpPath = ftpPath.Trim('/');

			string subfoldersAndFileName = ftpPath;
			if (!subfoldersAndFileName.Contains('/'))
			{
				if (isFilePath)
				{
					this.Subfolders = new List<string>();
					this.FileName = subfoldersAndFileName;
				}
				else
				{
					this.Subfolders = new List<string>() {subfoldersAndFileName};
					this.FileName = null;
				}
			}
			else
			{
				if (isFilePath)
				{
					this.Subfolders = subfoldersAndFileName.Split('/').ToList();
					this.Subfolders.RemoveAt(this.Subfolders.Count-1);
					this.FileName = subfoldersAndFileName.Split('/').Last();
				}
				else
				{
					this.Subfolders = subfoldersAndFileName.Split('/').ToList();
					this.FileName = null;
				}
			}

			this.username = username;
			this.password = password;
		}
		#endregion

		#region Deinitialization And Destructors
		#endregion

		#region Event Handlers
		#endregion

		#region Private Methods
		#endregion

		#region Helpers
		#endregion
	}
}
