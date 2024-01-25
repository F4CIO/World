namespace InfoCompass.World.Common.Entities;

public enum ExportType
{
	SettingsJson,
	Json,
	Txt,
	Pdf,
	Docx
}

public static class ExportTypeExtensions
{
	public static List<ExportType> ToListOfExportTypesOrNullInCaseOfError(this string csv)
	{
		List<ExportType> r;

		try
		{
			r = new List<ExportType>();
			List<string> exportTypesAsStrings = csv.ParseCSV<string>(new char[] { ',' }, new char[] { ' ' }, false, null, null, null);
			foreach(string exportTypeAsString in exportTypesAsStrings)
			{
				ExportType et = Enum.Parse<ExportType>(exportTypeAsString);
				if(!r.Contains(et))
				{
					r.Add(et);
				}
			}
		}
		catch(Exception) { r = null; }

		return r;
	}
}
