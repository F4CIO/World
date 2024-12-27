namespace MyCompany.World.Common.Entities;

public class LogFilter
{
	public DateTime? MomentFrom { get; set; }
	public DateTime? MomentTo { get; set; }
	public string OperationName { get; set; }
	public string Keyword { get; set; }
	public string VisitorHash { get; set; }
	public string LogTitle { get; set; }

	public int Offset { get; set; }
	public int Count { get; set; }
}
