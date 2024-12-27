//using Microsoft.VisualBasic.Devices;
using System.Diagnostics;

namespace MyCompany.World.Common.Entities;

[Serializable]
public class MemoryInfo
{
	public DateTime moment { get; set; }

	public ulong total { get; set; }
	public ulong totalVirtual { get; set; }
	public ulong available { get; set; }
	public long? availableDelta { get; set; }
	public ulong availableVirtual { get; set; }
	public long? availableVirtualDelta { get; set; }

	public string processName { get; set; }
	public string processFileName { get; set; }
	public long consumedByProcessManaged { get; set; }
	public long? consumedByProcessManagedDelta { get; set; }
	public long consumedByProcess { get; set; }
	public long? consumedByProcessDelta { get; set; }
	public long consumedByProcessWorkingSet { get; set; }
	public long? consumedByProcessWorkingSetDelta { get; set; }
	public long consumedByProcessVirtual { get; set; }
	public long? consumedByProcessVirtualDelta { get; set; }

	public string sufix { get; set; }

	public MemoryInfo(MemoryInfo previousMemoryInfo = null, string sufix = null)
	{
		this.moment = DateTime.UtcNow;

		//var computerInfo = new ComputerInfo();
		//this.total = computerInfo.TotalPhysicalMemory;
		//this.totalVirtual = computerInfo.TotalVirtualMemory;
		//this.available = computerInfo.AvailablePhysicalMemory;
		//this.availableDelta = previousMemoryInfo==null?(long?)null: (long?)this.available-(long?)previousMemoryInfo.available;
		//this.availableVirtual = computerInfo.AvailableVirtualMemory;
		//this.availableVirtualDelta = previousMemoryInfo == null ? (long?)null : (long?)this.availableVirtual - (long?)previousMemoryInfo.availableVirtual;

		var currentProcess = Process.GetCurrentProcess();
		this.processName = currentProcess.ProcessName;
		this.processFileName = currentProcess.MainModule.FileName;
		this.consumedByProcessManaged = GC.GetTotalMemory(true);
		this.consumedByProcessManagedDelta = previousMemoryInfo == null ? (long?)null : this.consumedByProcessManaged - previousMemoryInfo.consumedByProcessManaged;
		currentProcess.Refresh();
		this.consumedByProcess = currentProcess.PrivateMemorySize64;
		this.consumedByProcessDelta = previousMemoryInfo == null ? (long?)null : this.consumedByProcess - previousMemoryInfo.consumedByProcess;
		this.consumedByProcessWorkingSet = currentProcess.WorkingSet64;
		this.consumedByProcessWorkingSetDelta = previousMemoryInfo == null ? (long?)null : this.consumedByProcessWorkingSet - previousMemoryInfo.consumedByProcessWorkingSet;
		this.consumedByProcessVirtual = currentProcess.VirtualMemorySize64;
		this.consumedByProcessVirtualDelta = previousMemoryInfo == null ? (long?)null : this.consumedByProcessVirtual - previousMemoryInfo.consumedByProcessVirtual;

		this.sufix = sufix;
	}

	public override string ToString()
	{
		return this.ToString();
	}

	public string ToString(string separator = "|")
	{
		return string.Format("{0} available/total:{1} ({2})/{3}|virtual available/total:{4} ({5})/{6}|consumedByProcessTotal:{7} ({8})|consumedByProcessManaged:{9} ({10})|consumedByProcessWorkingSet:{11} ({12})|consumedByProcessVirtual:{13} ({14})|Process: {15} ({16}) {17}".Replace("|", separator),
			this.moment.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS(),

			InsertCommaOnThousands(this.available.ToString()).PadLeft(30, '_'),
			InsureSign(InsertCommaOnThousands(this.availableDelta.ToNonNullString("NA"))).PadLeft(30, '_'),
			InsertCommaOnThousands(this.total.ToString()).PadLeft(30, '_'),

			InsertCommaOnThousands(this.availableVirtual.ToString()).PadLeft(30, '_'),
			InsureSign(InsertCommaOnThousands(this.availableVirtualDelta.ToNonNullString("NA"))).PadLeft(30, '_'),
			InsertCommaOnThousands(this.totalVirtual.ToString()).PadLeft(30, '_'),

			InsertCommaOnThousands(this.consumedByProcess.ToString()).PadLeft(30, '_'),
			InsureSign(InsertCommaOnThousands(this.consumedByProcessDelta.ToNonNullString("NA"))).PadLeft(30, '_'),

			InsertCommaOnThousands(this.consumedByProcessManaged.ToString()).PadLeft(30, '_'),
			InsureSign(InsertCommaOnThousands(this.consumedByProcessManagedDelta.ToNonNullString("NA"))).PadLeft(30, '_'),

			InsertCommaOnThousands(this.consumedByProcessWorkingSet.ToString()).PadLeft(30, '_'),
			InsureSign(InsertCommaOnThousands(this.consumedByProcessWorkingSetDelta.ToNonNullString("NA"))).PadLeft(30, '_'),

			InsertCommaOnThousands(this.consumedByProcessVirtual.ToString()).PadLeft(30, '_'),
			InsureSign(InsertCommaOnThousands(this.consumedByProcessVirtualDelta.ToNonNullString("NA"))).PadLeft(30, '_'),

			this.processName.ToNonNullString(),
			this.processFileName.ToNonNullString(),

			this.sufix.ToNonNullString()
			);
	}

	public string ToXml()
	{
		string r = CraftSynth.BuildingBlocks.IO.Xml.Misc.Serialize<MemoryInfo>(this);
		return r;
	}

	public static MemoryInfo FromXml(string xml)
	{
		MemoryInfo r = CraftSynth.BuildingBlocks.IO.Xml.Misc.Deserialize<MemoryInfo>(xml);
		return r;
	}

	private static string InsureSign(string numberOrNa)
	{
		if(numberOrNa == "NA")
		{
			//change nothing
		}
		else if(numberOrNa.StartsWith("-"))
		{
			//change nothing
		}
		else
		{
			numberOrNa = "+" + numberOrNa;
		}
		return numberOrNa;
	}

	private static string InsertCommaOnThousands(string numberOrNa)
	{
		string r = "";

		int i = 0;
		bool nonDigitEncountered = false;
		foreach(char c in numberOrNa.ToNonNullString().Reverse())
		{
			if(!nonDigitEncountered && "0123456789".Contains(c))
			{
				string comma = "";
				if(i > 0 && i % 3 == 0)
				{
					comma = ",";
				}

				r = c + comma + r;
				i++;
			}
			else
			{
				nonDigitEncountered = true;
				r = c + r;
			}
		}

		return r;
	}
}
