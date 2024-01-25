export class GenerateInitialBookRequest {
  FirstName: string;
  LastName: string;
  BookTitle: string;
  ChaptersCount: number;
  BookLanguage: string;
  ExportTypesCsv: string;
  IAgreeToTerms?: boolean | null;
  SubscribeMe?: boolean | null;
  EMail: string;

  constructor() {
    this.FirstName = '';
    this.LastName = '';
    this.BookTitle = '';
    this.ChaptersCount = 0;
    this.BookLanguage = '';
    this.ExportTypesCsv = '';
    this.IAgreeToTerms = null;
    this.SubscribeMe = null;
    this.EMail = '';
  }
}