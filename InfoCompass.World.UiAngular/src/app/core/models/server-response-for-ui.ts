// You might want to define an enum for KnownColor if it's a finite set of values.
// This is a simplistic example:
export enum KnownColor {
    Red,
    Green,
    Blue,
    Yellow
    // ... other colors
  }

export class ServerResponseForUI<T> {
    isSuccess: boolean;
    message: string;
    messageColor?: KnownColor | null;
    logId?: number | null;
    logDetailsUri?: string | null;
    tag?: string | null;
    // Use a generic type for the data
    data?: T;
    httpStatusCode?: number | null;
  
    constructor() {
      this.isSuccess = false;
      this.message = '';
      this.messageColor = null;
      this.logId = null;
      this.logDetailsUri = null;
      this.tag = null;
      this.data = undefined;
      this.httpStatusCode = null;
    }
  }