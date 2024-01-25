using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

namespace CraftSynth.BuildingBlocks.IO
{
	public enum ExcelFormatVersion
	{
		xls,
		xlsx
	}
    public class Excel
    {
		private const string xlsConnectionStringFormat = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}; Extended Properties=Excel 8.0;";
		private const string xlsxConnectionStringFormat = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;";
	    /// <summary>
		/// Source: http://stackoverflow.com/questions/15828/reading-excel-files-from-c-sharp
		/// http://stackoverflow.com/questions/15793442/how-to-read-data-from-excel-file-using-c-sharp?lq=1
	    /// </summary>
	    /// <param name="filePath"></param>
	    /// <param name="workSheetName"></param>
	    /// <param name="formatVersion"></param>
	    /// <returns></returns>
		public static DataTable ReadFile(string filePath, string workSheetName = null, ExcelFormatVersion? formatVersion = null)
	    {
			DataTable r = new DataTable();
		    
			string connectionString = null;

		    if (formatVersion == null)
		    {
			    if (Path.GetExtension(filePath).ToLower() == ".xls")
			    {
				    formatVersion = ExcelFormatVersion.xls;
			    }
			    else if (Path.GetExtension(filePath).ToLower() == ".xlsx")
			    {
				    formatVersion = ExcelFormatVersion.xlsx;
			    }
			    else
			    {
				    throw new Exception("Can not determine excel format version. File extension must be xls or xlsx. FilePath: " + filePath);
			    }
		    }
	
			if (formatVersion.Value== ExcelFormatVersion.xls)
			{
				connectionString = string.Format(xlsConnectionStringFormat, filePath);
			}
			else if (formatVersion.Value== ExcelFormatVersion.xlsx)
			{
				connectionString = string.Format(xlsxConnectionStringFormat, filePath);
			}

			//TODO: port to DotNet6
			throw new NotImplementedException(); 
			//if (workSheetName == null)
   //         {
   //             using (OleDbConnection conn = new OleDbConnection(connectionString))
   //             {
   //                 conn.Open();
   //                 var dtSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
   //                 workSheetName = dtSchema.Rows[0].Field<string>("TABLE_NAME").Trim('\'').TrimEnd('$');
   //             }
   //         }

   //         workSheetName = workSheetName ?? "Sheet1";
			//var adapter = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}$]", workSheetName), connectionString);
			//var ds = new DataSet();
			//adapter.Fill(ds, "t1");
			//r = ds.Tables["t1"];
		
		 //   return r;
	    }
    }
}
