namespace InfoCompass.World.Common.Entities;

public class Languages
{
	/// <summary>
	/// https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
	/// </summary>
	/// <returns></returns>
	public static Dictionary<string, string> GetAllLanguages()
	{
		var r = new Dictionary<string, string>
		{
			{ "af", "Afrikaans" },
			{ "am", "Amharic" },
			{ "ar", "Arabic" },
			{ "bg", "Bulgarian" },
			{ "bn", "Bengali" },
			{ "cs", "Czech" },
			{ "cy", "Welsh" },
			{ "da", "Danish" },
			{ "de", "German" },
			{ "el", "Greek" },
			{ "en", "English" },
			{ "es", "Spanish" },
			{ "es-ES", "Spanish (Spain)" },
			{ "es-MX", "Spanish (Mexico)" },
			{ "et", "Estonian" },
			{ "eu", "Basque" },
			{ "fa", "Persian" },
			{ "fi", "Finnish" },
			{ "fr", "French" },
			{ "ga", "Irish" },
			{ "gu", "Gujarati" },
			{ "he", "Hebrew" },
			{ "hi", "Hindi" },
			{ "hr", "Croatian" },
			{ "hu", "Hungarian" },
			{ "id", "Indonesian" },
			{ "it", "Italian" },
			{ "ja", "Japanese" },
			{ "ka", "Georgian" },
			{ "kn", "Kannada" },
			{ "ko", "Korean" },
			{ "ku", "Kurdish" },
			{ "ky", "Kyrgyz" },
			{ "lt", "Lithuanian" },
			{ "lv", "Latvian" },
			{ "ml", "Malayalam" },
			{ "ms", "Malay" },
			{ "mt", "Maltese" },
			{ "ne", "Nepali" },
			{ "nl", "Dutch" },
			{ "no", "Norwegian" },
			{ "or", "Oriya" },
			{ "pa", "Punjabi" },
			{ "pl", "Polish" },
			{ "pt", "Portuguese" },
			{ "pt-BR", "Portuguese (Brazil)" },
			{ "pt-PT", "Portuguese (Portugal)" },
			{ "ro", "Romanian" },
			{ "ru", "Russian" },
			{ "sd", "Sindhi" },
			{ "sk", "Slovak" },
			{ "sl", "Slovenian" },
			{ "so", "Somali" },
			{ "sr", "Serbian" },
			{ "sv", "Swedish" },
			{ "sw", "Swahili" },
			{ "ta", "Tamil" },
			{ "te", "Telugu" },
			{ "th", "Thai" },
			{ "tl", "Tagalog" },
			{ "tr", "Turkish" },
			{ "uk", "Ukrainian" },
			{ "uz", "Uzbek" },
			{ "vi", "Vietnamese" },
			{ "zh", "Chinese" },
			{ "zh-CN", "Chinese (Simplified)" },
			{ "zh-HK", "Chinese (Hong Kong)" },
			{ "zh-TW", "Chinese (Traditional)" },
			{ "zu", "Zulu" }
		};
		return r;
	}

	public static string GetLanguageName(string languageCode, string nullLanguageCodeCaseResult, string notFoundCaseResult)
	{
		string r;
		Dictionary<string, string> allLanguages = GetAllLanguages();
		if(languageCode == null)
		{
			r = nullLanguageCodeCaseResult;
		}
		else
		{
			if(languageCode.Contains('-'))
			{
				languageCode = languageCode.Split('-')[0].ToLower() + "-" + languageCode.Split('-')[1].ToUpper();
			}
			else
			{
				languageCode = languageCode.ToLower();
			}

			if(!allLanguages.ContainsKey(languageCode))
			{
				r = notFoundCaseResult;
			}
			else
			{
				r = allLanguages[languageCode];
			}
		}
		return r;
	}
}
