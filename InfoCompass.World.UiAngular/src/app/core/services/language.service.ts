// language.service.ts

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private allLanguages: { [key: string]: string } = {
    'af': 'Afrikaans',
    'am': 'Amharic',
    'ar': 'Arabic',
    'bg': 'Bulgarian',
    'bn': 'Bengali',
    'cs': 'Czech',
    'cy': 'Welsh',
    'da': 'Danish',
    'de': 'German',
    'el': 'Greek',
    'en': 'English',
    'es': 'Spanish',
    'es-ES': 'Spanish (Spain)',
    'es-MX': 'Spanish (Mexico)',
    'et': 'Estonian',
    'eu': 'Basque',
    'fa': 'Persian',
    'fi': 'Finnish',
    'fr': 'French',
    'ga': 'Irish',
    'gu': 'Gujarati',
    'he': 'Hebrew',
    'hi': 'Hindi',
    'hr': 'Croatian',
    'hu': 'Hungarian',
    'id': 'Indonesian',
    'it': 'Italian',
    'ja': 'Japanese',
    'ka': 'Georgian',
    'kn': 'Kannada',
    'ko': 'Korean',
    'ku': 'Kurdish',
    'ky': 'Kyrgyz',
    'lt': 'Lithuanian',
    'lv': 'Latvian',
    'ml': 'Malayalam',
    'ms': 'Malay',
    'mt': 'Maltese',
    'ne': 'Nepali',
    'nl': 'Dutch',
    'no': 'Norwegian',
    'or': 'Oriya',
    'pa': 'Punjabi',
    'pl': 'Polish',
    'pt': 'Portuguese',
    'pt-BR': 'Portuguese (Brazil)',
    'pt-PT': 'Portuguese (Portugal)',
    'ro': 'Romanian',
    'ru': 'Russian',
    'sd': 'Sindhi',
    'sk': 'Slovak',
    'sl': 'Slovenian',
    'so': 'Somali',
    'sr': 'Serbian',
    'sv': 'Swedish',
    'sw': 'Swahili',
    'ta': 'Tamil',
    'te': 'Telugu',
    'th': 'Thai',
    'tl': 'Tagalog',
    'tr': 'Turkish',
    'uk': 'Ukrainian',
    'uz': 'Uzbek',
    'vi': 'Vietnamese',
    'zh': 'Chinese',
    'zh-CN': 'Chinese (Simplified)',
    'zh-HK': 'Chinese (Hong Kong)',
    'zh-TW': 'Chinese (Traditional)',
    'zu': 'Zulu'
  };

  constructor() { }

  public getAllLanguages(): { [key: string]: string } {
    return this.allLanguages;
  }

  public getAllLanguagesOrderedByName(): { [key: string]: string } {
    // Convert the allLanguages object into an array of [key, value] pairs
    const languagesArray = Object.entries(this.allLanguages);

    // Sort the array by the language name (value)
    languagesArray.sort((a, b) => a[1].localeCompare(b[1]));

    // Convert the sorted array back into an object
    const sortedLanguages = languagesArray.reduce((obj, [key, value]) => {
      obj[key] = value;
      return obj;
    }, {} as { [key: string]: string });

    return sortedLanguages;
  }

  public getLanguageName(languageCode: string | null, nullLanguageCodeCaseResult: string, notFoundCaseResult: string): string {
    if (languageCode === null) {
      return nullLanguageCodeCaseResult;
    }

    const standardizedLanguageCode = this.standardizeLanguageCode(languageCode);

    return this.allLanguages[standardizedLanguageCode] || notFoundCaseResult;
  }

  private standardizeLanguageCode(languageCode: string): string {
    if (languageCode.includes('-')) {
      const parts = languageCode.split('-');
      return `${parts[0].toLowerCase()}-${parts[1].toUpperCase()}`;
    } else {
      return languageCode.toLowerCase();
    }
  }
}
