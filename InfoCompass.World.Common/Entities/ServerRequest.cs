namespace MyCompany.World.Common.Entities;

public class ServerRequest
{
	public int? CurrentlyLoggedInUserId { get; set; }
	public int Value { get; set; }

	/// <summary>
	/// Tells what is contained in Parameter1. Optional. Mostly for debugging.
	/// </summary>
	public string Parameter1Name { get; set; }
	public string Parameter1 { get; set; }

	/// <summary>
	/// Tells what is contained in Parameter1. Optional. Mostly for debugging.
	/// </summary>
	public string Parameter2Name { get; set; }
	public string Parameter2 { get; set; }

	/// <summary>
	/// Used sometimes to prevent any browser caching.
	/// </summary>
	public string RandomValue { get; set; }
}