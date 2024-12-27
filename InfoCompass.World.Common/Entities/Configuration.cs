namespace MyCompany.World.Common.Entities;

public class Configuration
{
	public Environment Environment { get; set; }
	public string DataFolderPath { get; set; }
	public string DataFolderPathAbsolute
	{
		get
		{
			string r = CraftSynth.BuildingBlocks.IO.FileSystem.GetAbsolutePath(this.DataFolderPath);
			return r;
		}
	}
	public string AllowedOriginsCsv { get; set; }
}
