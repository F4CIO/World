using CraftSynth.BuildingBlocks;
using Errors = MyCompany.World.Common.Entities.Errors;

namespace MyCompany.World.Common;

public static class FieldValidator
{
	public static Errors ValidateField(string s,
		string fieldName,
		string fieldNameUserFriendly,
		FieldType fieldType,
		bool required,
		bool allowSpaceCharacter,
		int min,
		int max,
		Errors errors)
	{
		if(fieldName == null)
		{
			throw new ArgumentNullException(nameof(fieldName));
		}
		if(fieldNameUserFriendly == null)
		{
			throw new ArgumentNullException(nameof(fieldNameUserFriendly));
		}

		errors = errors ?? new Errors();

		if(required && (s == null || s == ""))
		{
			errors.Add(fieldName, $"{fieldNameUserFriendly} is required.");
		}
		else
		{
			if(s != null)
			{
				if(!allowSpaceCharacter && s.Contains(' '))
				{
					if(required && s.Trim() == string.Empty)
					{
						errors.Add(fieldName, $"{fieldNameUserFriendly} is required.");
					}
					else
					{
						errors.Add(fieldName, $"{fieldNameUserFriendly} should not include space characters.");
					}
				}
				else
				{

					if(allowSpaceCharacter && (fieldType == FieldType.EMail || fieldType == FieldType.Url || fieldType == FieldType.Integer))
					{
						s = s.Trim();
					}

					bool minMaxRepresentStringLengthNotValue = fieldType != FieldType.Integer;

					if(minMaxRepresentStringLengthNotValue)
					{
						if(required || s.Length > 0)
						{
							if(s.Length < min)
							{
								errors.Add(fieldName, $"{fieldNameUserFriendly} must have at least {min} characters.");
							}

							if(s.Length > max)
							{
								errors.Add(fieldName, $"{fieldNameUserFriendly} can have up to {max} characters.");
							}
						}
					}

					if(required || s.Length > 0)
					{
						switch(fieldType)
						{
							case FieldType.EMail:
								if(!s.IsEMail())
								{
									errors.Add(fieldName, $"{fieldNameUserFriendly} should be valid EMail.");
								}
								break;
							case FieldType.Url:
								if(!s.IsUri(false))
								{
									errors.Add(fieldName, $"{fieldNameUserFriendly} should be valid Url.");
								}
								break;
							case FieldType.Integer:
								int value;
								bool successfullyParsed = int.TryParse(s, out value);
								if(!successfullyParsed)
								{
									errors.Add(fieldName, $"{fieldNameUserFriendly} should be valid integer number.");
								}
								else
								{
									if(value < min || value > max)
									{
										errors.Add(fieldName, $"{fieldNameUserFriendly} should be valid integer number between {min} and {max}.");
									}
								}
								break;
							case FieldType.AlphaUS:
								if(allowSpaceCharacter)
								{
									s = s.Replace(" ", "");
								}
								if(!s.IsAlphaUS())
								{
									errors.Add(fieldName, $"Please use only US letters (A to Z) for {fieldNameUserFriendly}.");
								}
								break;
							case FieldType.AlphaNumericUS:
								if(allowSpaceCharacter)
								{
									s = s.Replace(" ", "");
								}
								if(!s.IsAlphaNumericUS())
								{
									errors.Add(fieldName, $"Please use only US letters (A to Z) and digits (0 to 9) for {fieldNameUserFriendly}.");
								}
								break;
							case FieldType.AlphaAnyLanguage:
								if(allowSpaceCharacter)
								{
									s = s.Replace(" ", "");
								}
								if(!s.IsAllLetters())
								{
									errors.Add(fieldName, $"Please use only letters for {fieldNameUserFriendly}.");
								}
								break;

							case FieldType.AlphaNumericAnyLanguage:
								if(allowSpaceCharacter)
								{
									s = s.Replace(" ", "");
								}
								if(!s.IsAllLettersOrDigits())
								{
									errors.Add(fieldName, $"Please use only letters and digits for {fieldNameUserFriendly}.");
								}
								break;
							case FieldType.AlphaNumericPlusExtraCharsAnyLanguage:
								if(allowSpaceCharacter)
								{
									s = s.Replace(" ", "");
								}
								if(!s.IsAllLettersDigitsOrExceptions("!#$%&()*+,-./:;<=>?_", true, true))
								{
									errors.Add(fieldName, $"Please use only letters and digits for {fieldNameUserFriendly}.");
								}
								break;
							//case FieldType.AlphaWithPunctuationAnyLanguage:
							//	if(allowSpaceCharacter)
							//	{
							//		s = s.Replace(" ", "");
							//	}
							//	if(!s.IsAllLettersOrPunctuations())
							//	{
							//		errors.Add(fieldName, $"Please use only letters for {fieldNameUserFriendly}.");
							//	}
							//	break;
							//case FieldType.AlphaNumericWithPunctuationAnyLanguage:
							//	if(allowSpaceCharacter)
							//	{
							//		s = s.Replace(" ", "");
							//	}
							//	if(!s.IsAllLettersDigitsOrPunctuations())
							//	{
							//		errors.Add(fieldName, $"Please use only letters and digits for {fieldNameUserFriendly}.");
							//	}
							//	break;
							case FieldType.Other:
								break;
							default: throw new Exception($"FieldType not implemented. FieldType={fieldType}");
						}
					}
				}
			}
		}

		return errors;
	}

	public static Errors ValidateField(this bool? s,
		string fieldName,
		string fieldNameUserFriendly,
		bool required,
		Errors errors)
	{
		if(fieldName == null)
		{
			throw new ArgumentNullException(nameof(fieldName));
		}
		if(fieldNameUserFriendly == null)
		{
			throw new ArgumentNullException(nameof(fieldNameUserFriendly));
		}

		errors = errors ?? new Errors();

		if(required && s == null)
		{
			errors.Add(fieldName, $"{fieldNameUserFriendly} is required.");
		}

		return errors;
	}
}

public enum FieldType
{
	EMail,
	Url,

	Integer,
	AlphaUS,
	AlphaNumericUS,
	AlphaAnyLanguage,
	AlphaNumericAnyLanguage,
	AlphaNumericPlusExtraCharsAnyLanguage,
	//AlphaWithPunctuationAnyLanguage,
	//AlphaNumericWithPunctuationAnyLanguage,

	Other
}
