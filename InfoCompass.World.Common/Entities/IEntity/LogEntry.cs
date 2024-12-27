namespace MyCompany.World.Common.Entities;

[Serializable]
public class LogEntry:IEntity
{
	public long Id { get; set; } // Id (Primary key)
	public System.DateTime Moment { get; set; } // Moment
	public long? LoggedInUserId { get; set; } // LoggedInUserId
	public int Level { get; set; } // Level (length: 32)
	public string Category { get; set; } // Category (length: 250)
	public string Title { get; set; } // Title (length: 250)
	public string Details { get; set; } // Details
	public string ExtraColumn1Name { get; set; } // ExtraColumn1Name (length: 250)
	public long? ExtraColumn1Value { get; set; } // ExtraColumn1Value
	public string ExtraColumn2Name { get; set; } // ExtraColumn2Name (length: 250)
	public long? ExtraColumn2Value { get; set; } // ExtraColumn2Value
	public string VisitorHash { get; set; } // VisitorHash (length: 32)
	public string MemoryInfo { get; set; } // MemoryInfo (length: 1000)

	public LogEntry()
	{
	}

	public string LogCategoryAndTitleAsString
	{
		get
		{
			string r = this.Category.ToNonNullString() + "__" + this.Title.ToNonNullString().Replace(' ', '_');
			return r;
		}
	}

	public LogCategoryAndTitle? LogCategoryAndTitleOrNullWhereCanNotMatch
	{
		get
		{
			LogCategoryAndTitle? r = null;
			try
			{
				string rAsString = this.Category.ToNonNullString() + "__" + this.Title.ToNonNullString().Replace(' ', '_');
				r = LogCategoryAndTitle.Parse<LogCategoryAndTitle>(rAsString);
			}
			catch(Exception ex)
			{
				r = null;
			}
			return r;
		}
	}


	public static KeyValuePair<string, string> SeparateLogCategoryAndLogTitle(string logCategoryAndTitle)
	{
		try
		{
			return new KeyValuePair<string, string>(logCategoryAndTitle.Replace("__", "|").Split('|')[0], logCategoryAndTitle.Replace("__", "|").Split('|')[1].Replace('_', ' '));
		}
		catch
		{
			throw new Error("Can not detect SeparateLogCategoryAndLogTitle form: " + logCategoryAndTitle.ToNonNullString("null"));
		}
	}

	public override string ToString()
	{
		return this.ToString(",");
	}

	public string ToString(string separator = ",", string loggedInUserEMail = null)
	{
		var sb = new StringBuilder();

		sb.Append($"Id: {this.Id}");
		sb.Append($"{separator}Moment: {this.Moment.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS()}");
		sb.Append($"{separator}LoggedInUserId: {this.LoggedInUserId.ToNonNullString("null")}");
		if(loggedInUserEMail != null)
		{
			sb.Append($"{separator}LoggedInUserEMail: {loggedInUserEMail.ToNonNullString("null")}");
		}
		sb.Append($"{separator}Level: {this.Level}");
		sb.Append($"{separator}Category: {this.Category.ToNonNullString("null")}");
		sb.Append($"{separator}Title: {this.Title.ToNonNullString("null")}");
		sb.Append($"{separator}Details: {this.Details.ToNonNullString("null")}");
		sb.Append($"{separator}ExtraColumn1Name: {this.ExtraColumn1Name.ToNonNullString("null")}");
		sb.Append($"{separator}ExtraColumn1Value: {this.ExtraColumn1Value.ToNonNullString("null")}");
		sb.Append($"{separator}ExtraColumn2Name: {this.ExtraColumn2Name.ToNonNullString("null")}");
		sb.Append($"{separator}ExtraColumn2Value: {this.ExtraColumn2Value.ToNonNullString("null")}");
		sb.Append($"{separator}VisitorHash: {this.VisitorHash.ToNonNullString("null")}");
		sb.Append($"{separator}MemoryInfo: {this.MemoryInfo.ToNonNullString("null")}");

		return sb.ToString();
	}
}
