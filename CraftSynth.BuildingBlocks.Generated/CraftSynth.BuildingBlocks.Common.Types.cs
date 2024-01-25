namespace CraftSynth.BuildingBlocks.Common.Types;

public enum UnspecifiedYesNoEnum
{
	Unspecified = 0,
	Yes = 1,
	No = -1
}

public class StringTripple
{
	public string A { get; set; }
	public string B { get; set; }
	public string C { get; set; }
	public StringTripple(string a, string b, string c)
	{
		this.A = a;
		this.B = b;
		this.C = c;
	}
}

public class Triple<T1, T2, T3>
{
	public T1 A { get; set; }
	public T2 B { get; set; }
	public T3 C { get; set; }

	public Triple(T1 a, T2 b, T3 c)
	{
		this.A = a;
		this.B = b;
		this.C = c;
	}
}

public class Quadruple<T1, T2, T3, T4>
{
	public T1 A { get; set; }
	public T2 B { get; set; }
	public T3 C { get; set; }
	public T4 D { get; set; }

	public Quadruple(T1 a, T2 b, T3 c, T4 d)
	{
		this.A = a;
		this.B = b;
		this.C = c;
		this.D = d;
	}

	public override string ToString()
	{
		return $"{(this.A?.ToString()).ToNonNullString("null")},{(this.B?.ToString()).ToNonNullString("null")},{(this.C?.ToString()).ToNonNullString("null")},{(this.D?.ToString()).ToNonNullString("null")}";
	}
}
