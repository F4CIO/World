using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using ZetaLongPaths;

namespace CraftSynth.BuildingBlocks.IO
{
	public enum FileSizeUnit
	{
		Bytes = 0,
		KiloBytes = 1,
		MegaBytes = 2,
		GigaBytes = 3,
		TeraBytes = 4
	}

	/// <summary>
	/// Example of usage  
	/// measuredSize.FromBytes().ToMBytes(2)                         can result in for example 2048.45 
	/// measuredSize.FromBytes().ToFriendlySingleLetterFileSize(2)   can result in for example "2,04 Gb"
	/// </summary>
	public static class FileSizeExtenders
	{
		public static decimal ConvertFileSize(this decimal fromValue, FileSizeUnit fromUnit, FileSizeUnit toUnit, int? decimals = null)
		{
			decimal r;

			//convert to bytes
			switch (fromUnit)
			{
				case FileSizeUnit.Bytes: r = fromValue; break;
				case FileSizeUnit.KiloBytes: r = fromValue * 1024; break;
				case FileSizeUnit.MegaBytes: r = fromValue * 1024 * 1024; break;
				case FileSizeUnit.GigaBytes: r = fromValue * 1024 * 1024 * 1024; break;
				case FileSizeUnit.TeraBytes: r = fromValue * 1024 * 1024 * 1024 * 1024; break;
				default: throw new Exception("fromUnit FileSizeUnit not recognized");
			}

			//convert to toUnit
			switch (toUnit)
			{
				case FileSizeUnit.Bytes:; break;
				case FileSizeUnit.KiloBytes: r = r / 1024; break;
				case FileSizeUnit.MegaBytes: r = r / 1024 / 1024; break;
				case FileSizeUnit.GigaBytes: r = r / 1024 / 1024 / 1024; break;
				case FileSizeUnit.TeraBytes: r = r / 1024 / 1024 / 1024 * 1024; break;
				default: throw new Exception("toUnit FileSizeUnit not recognized");
			}

			//round to decimals
			if (decimals != null)
			{
				r = Math.Round(r, decimals.Value);
			}

			return r;
		}

		public static decimal FromBytes(this decimal v)
		{
			return v;
		}

		public static decimal FromBytes(this long v)
		{
			return v;
		}

		public static decimal FromKBytes(this decimal v)
		{
			return ConvertFileSize(v, FileSizeUnit.KiloBytes, FileSizeUnit.Bytes);
		}

		public static decimal FromMBytes(this decimal v)
		{
			return ConvertFileSize(v, FileSizeUnit.MegaBytes, FileSizeUnit.Bytes);
		}

		public static decimal FromGBytes(this decimal v)
		{
			return ConvertFileSize(v, FileSizeUnit.GigaBytes, FileSizeUnit.Bytes);
		}

		public static decimal FromTBytes(this decimal v)
		{
			return ConvertFileSize(v, FileSizeUnit.TeraBytes, FileSizeUnit.Bytes);
		}

		public static decimal ToBytes(this decimal bytes, int? decimals = null)
		{
			return ConvertFileSize(bytes, FileSizeUnit.Bytes, FileSizeUnit.Bytes, decimals);
		}

		public static decimal ToKBytes(this decimal bytes, int? decimals = null)
		{
			return ConvertFileSize(bytes, FileSizeUnit.Bytes, FileSizeUnit.KiloBytes, decimals);
		}

		public static decimal ToMBytes(this decimal bytes, int? decimals = null)
		{
			return ConvertFileSize(bytes, FileSizeUnit.Bytes, FileSizeUnit.MegaBytes, decimals);
		}

		public static decimal ToTBytes(this decimal bytes, int? decimals = null)
		{
			return ConvertFileSize(bytes, FileSizeUnit.Bytes, FileSizeUnit.TeraBytes, decimals);
		}

		public static string ToFriendlyTwoLettersFileSize(this decimal bytes, int? decimals = 2, bool alwaysShowDecimals = true, bool putSpaceCharBeforeUnit = true)
		{
			string r = "";
			decimal v = bytes;
			FileSizeUnit u = FileSizeUnit.Bytes;

			decimal peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.KiloBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.MegaBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.GigaBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.TeraBytes;
			}


			if (decimals != null)
			{
				v = Math.Round(v, decimals.Value);
			}

			r += v;

			if (alwaysShowDecimals)
			{
				r = v.ToString("N" + decimals);
			}

			if (putSpaceCharBeforeUnit)
			{
				r += " ";
			}

			switch (u)
			{
				case FileSizeUnit.Bytes: r += "B"; break;
				case FileSizeUnit.KiloBytes: r += "KB"; break;
				case FileSizeUnit.MegaBytes: r += "MB"; break;
				case FileSizeUnit.GigaBytes: r += "GB"; break;
				case FileSizeUnit.TeraBytes: r += "TB"; break;
				default: throw new Exception("u FileSizeUnit not recognized");
			}

			return r;
		}

		public static string ToFriendlyBytesWordWithPrefixLetterFileSize(this decimal bytes, int? decimals = 2, bool alwaysShowDecimals = true, bool putSpaceCharBeforeUnit = true)
		{
			string r = "";
			decimal v = bytes;
			FileSizeUnit u = FileSizeUnit.Bytes;

			decimal peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.KiloBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.MegaBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.GigaBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.TeraBytes;
			}


			if (decimals != null)
			{
				v = Math.Round(v, decimals.Value);
			}

			r += v;

			if (alwaysShowDecimals)
			{
				r = v.ToString("N" + decimals);
			}

			if (putSpaceCharBeforeUnit)
			{
				r += " ";
			}

			switch (u)
			{
				case FileSizeUnit.Bytes: r += "Bytes"; break;
				case FileSizeUnit.KiloBytes: r += "KBytes"; break;
				case FileSizeUnit.MegaBytes: r += "MBytes"; break;
				case FileSizeUnit.GigaBytes: r += "GBytes"; break;
				case FileSizeUnit.TeraBytes: r += "TBytes"; break;
				default: throw new Exception("u FileSizeUnit not recognized");
			}

			return r;
		}

		public static string ToFriendlySingleLetterFileSize(this decimal bytes, int? decimals = 2, bool alwaysShowDecimals = true, bool putSpaceCharBeforeUnit = true)
		{
			string r = "";
			decimal v = bytes;
			FileSizeUnit u = FileSizeUnit.Bytes;

			decimal peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.KiloBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.MegaBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.GigaBytes;
			}

			peek = v / 1024;
			if (peek >= 1)
			{
				v = peek;
				u = FileSizeUnit.TeraBytes;
			}


			if (decimals != null)
			{
				v = Math.Round(v, decimals.Value);
			}

			r += v;

			if (alwaysShowDecimals)
			{
				r = v.ToString("N" + decimals);
			}

			if (putSpaceCharBeforeUnit)
			{
				r += " ";
			}

			switch (u)
			{
				case FileSizeUnit.Bytes: r += "b"; break;
				case FileSizeUnit.KiloBytes: r += "k"; break;
				case FileSizeUnit.MegaBytes: r += "M"; break;
				case FileSizeUnit.GigaBytes: r += "G"; break;
				case FileSizeUnit.TeraBytes: r += "T"; break;
				default: throw new Exception("u FileSizeUnit not recognized");
			}

			return r;
		}
	}
}
