namespace MyCompany.World.Common.Entities;

public static class PageNumberPositionExtenderClass
{
	public static PageNumberPosition? ToPageNumberPosition(this string s, PageNumberPosition? nullCaseResult = null, PageNumberPosition? invalidValueCaseResult = null)
	{
		if(s == null) { return nullCaseResult; }
		{
			s = s.Trim().ToLower();
			switch(s)
			{
				case "left": return PageNumberPosition.Left;
				case "center": return PageNumberPosition.Center;
				case "right": return PageNumberPosition.Right;
				case "leftonoddrightoneven": return PageNumberPosition.LeftOnOddRightOnEven;
				case "leftonevenrightonodd": return PageNumberPosition.LeftOnEvenRightOnOdd;
				default: return invalidValueCaseResult;
			}
		}
	}

	public static PageNumberPosition ToOneOfThreeStates(this PageNumberPosition? pageNumberPosition, int pageNumber, PageNumberPosition nullCaseResult)
	{
		PageNumberPosition r;

		if(pageNumberPosition == null)
		{
			r = nullCaseResult;
		}
		else
		{
			switch(pageNumberPosition)
			{
				case PageNumberPosition.Center:
					r = PageNumberPosition.Center;
					break;
				case PageNumberPosition.Left:
					r = PageNumberPosition.Left;
					break;
				case PageNumberPosition.Right:
					r = PageNumberPosition.Right;
					break;
				case PageNumberPosition.LeftOnOddRightOnEven:
					r = pageNumber % 2 == 1 ? PageNumberPosition.Left : PageNumberPosition.Right;
					break;
				case PageNumberPosition.LeftOnEvenRightOnOdd:
					r = pageNumber % 2 == 0 ? PageNumberPosition.Left : PageNumberPosition.Right;
					break;
				default: throw new Exception("pageNumberPosition case not implemented.");
			}
		}

		return r;
	}
}

public enum PageNumberPosition
{
	Center = 0,
	Left = 1,
	Right = 2,
	LeftOnOddRightOnEven = 3,
	LeftOnEvenRightOnOdd = 4,
}
