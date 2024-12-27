namespace MyCompany.World.Common.Entities;

public class Settings:IEntity
{
	public long Id { get; set; }
	public long UserId { get; set; }
	public bool Enable { get; set; }
	public string InstanceName { get; set; }
	public string Version { get; set; }
	public string Description { get; set; }
	public string ContactName { get; set; }
	public string ContactEmail { get; set; }
	public string ContactUrl { get; set; }
	public bool License { get; set; }
	public string LicenseName { get; set; }
	public string LicenseUrl { get; set; }

	public Environment Environment { get; set; }
	public string DataFolderPath { get; set; }
	public string DataFolderPathAbsolute(string dataFolderPath)
	{
		string r = CraftSynth.BuildingBlocks.IO.FileSystem.PathCombine(dataFolderPath, this.DataFolderPath);
		return r;
	}

	public int LogLevelForLogSink_FilePerUserPerJob { get; set; }
	public int LogLevelForLogSink_Db { get; set; }

	public string MailServerHostName { get; set; }
	public int? MailServerPort { get; set; }
	public string MailServerUsername { get; set; }
	public string MailServerPassword { get; set; }
	public bool MailServerIgnoreCertificateErrors { get; set; }
	public string MailServerFromEMail { get; set; }
	public string MailServerFromName { get; set; }

	public string LogEventsMailNotifications_ReceiversCsv { get; set; }
	public List<string> LogEventsMailNotifications_Receivers
	{
		get
		{
			return this.LogEventsMailNotifications_ReceiversCsv.ParseCSV(new char[] { ',' }, new char[] { ' ' }, false, null, null, new List<string>());
		}
	}
	public string LogEventsMailNotifications_CsvOfLogCategoryAndTitleForWhichToMailNotifications { get; set; }
	public List<string> LogEventsMailNotifications_LogCategoryAndTitleForWhichToMailNotifications
	{
		get
		{
			return this.LogEventsMailNotifications_CsvOfLogCategoryAndTitleForWhichToMailNotifications.ParseCSV(new char[] { ',' }, new char[] { ' ' }, false, null, null, new List<string>());
		}
	}
	public string LogEventsMailNotifications_CsvOfLogCategoryAndTitleForWhichNotToMailNotifications { get; set; }
	public List<string> LogEventsMailNotifications_LogCategoryAndTitleForWhichNotToMailNotifications
	{
		get
		{
			return this.LogEventsMailNotifications_CsvOfLogCategoryAndTitleForWhichNotToMailNotifications.ParseCSV(new char[] { ',' }, new char[] { ' ' }, false, null, null, new List<string>());
		}
	}
	public string LogEventsMailNotifications_CsvOfMailBodyPhrasesForWhichNotToMailNotifications { get; set; }
	public List<string> LogEventsMailNotifications_MailBodyPhrasesForWhichNotToMailNotifications
	{
		get
		{
			return this.LogEventsMailNotifications_CsvOfMailBodyPhrasesForWhichNotToMailNotifications.ParseCSV(new char[] { '|' }, new char[] { ' ' }, false, null, null, new List<string>());
		}
	}

	public int LoginExpirationPeriodInMinutes { get; set; }
	public string LoginSecretKeyForJwt { get; set; }

	public string GuiUrl { get; set; }

	public int TimeoutForJobsInMinutes { get; set; }
}
