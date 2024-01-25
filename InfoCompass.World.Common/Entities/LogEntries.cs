namespace InfoCompass.World.Common.Entities;

[Serializable]
public class LogEntries
{
	public List<LogEntry> Items { get; set; }

	public LogEntries()
	{
		this.Items = new List<LogEntry>();
	}
}
