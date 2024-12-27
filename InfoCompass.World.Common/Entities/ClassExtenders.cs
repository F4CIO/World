namespace MyCompany.World.Common;

public static class ClassExtenders
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	public static string Example1(this string s)
	{
		s = s.TrimStart(" ");
		return s;
	}
}
