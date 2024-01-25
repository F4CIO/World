export class ServerError {
    // Usually used to keep the name of the input field on UI where validation didn't pass.
    key: string;
  
    // Usually used to keep status code that Web API web service can catch and build a proper response if this error propagates up in the layers.
    // In TypeScript, you can use `number` for HTTP status codes since there's no equivalent to the HttpStatusCode enum.
    httpStatusCode?: number;
  
    // If the exception is not handled/catched explicitly in code, this message will be displayed to the user.
    // If this property is null then a generic/default error message is displayed.
    friendlyMessage?: string;
  
    // Attach additional data here like a trace log and values of local variables where the exception occurred.
    // To build a trace string, you would implement a separate logic similar to CustomTraceLog class.
    longMessage?: string;
  
    // If used with a friendly message, the user will be notified about the error, but nothing will be logged in our system.
    // To be used, for example, if a user didn't fill in a required field or for another error of an informative nature.
    skipLoggingIntoDbAndMail: boolean;
  
    // ID of the DB log entry where this error was logged. Usually, the error is logged on the business logic layer, so the ID can be used upper at the UI layer to inform the user where to find more details.
    // However, if SkipLoggingIntoDbAndMail is true, this parameter will probably always be null.
    logId?: number;
  
    constructor(key: string, httpStatusCode?: number, friendlyMessage?: string, longMessage?: string, skipLoggingIntoDbAndMail: boolean = false, logId?: number) {
      this.key = key;
      this.httpStatusCode = httpStatusCode;
      this.friendlyMessage = friendlyMessage;
      this.longMessage = longMessage;
      this.skipLoggingIntoDbAndMail = skipLoggingIntoDbAndMail;
      this.logId = logId;
    }
  }

  export type ServerErrors = ServerError[];
  