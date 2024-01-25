import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { catchError, delay, map, tap} from 'rxjs/operators';
import * as jwt_decode from 'jwt-decode';
import * as moment from 'moment';

import { environment } from '../../../environments/environment';
import { of, EMPTY, Observable, throwError } from 'rxjs';
import { GenerateInitialBookRequest } from '../models/generate-initial-book-request';
import { ServerResponseForUI } from '../models/server-response-for-ui';
import {ServerErrors} from '../models/server-error';

@Injectable({
    providedIn: 'root'
})
export class BooksService {
    private API_BASE_URL = environment.API_BASE_URL;

    constructor(private http: HttpClient,
        @Inject('LOCALSTORAGE') private localStorage: Storage) {
    }

    public generateInitial(requestBody: GenerateInitialBookRequest): Observable<ServerResponseForUI<number | ServerErrors>> {
        const finalUrl = `${this.API_BASE_URL}/books/GenerateInitial`;
        const requestBodyAsJson = JSON.stringify(requestBody);    
        console.log('Request body as JSON:', requestBodyAsJson);
    
        const httpOptions = {
            headers: new HttpHeaders({
              'Content-Type': 'application/json'
            })
          };

          return this.http.post<ServerResponseForUI<any>>(finalUrl, requestBodyAsJson, httpOptions).pipe(
            map(response => {
                // Manually check the response and handle the Data type accordingly
                if (response.isSuccess) {
                    // If success, cast the data to number
                    response.data = Number(response.data);
                } else {
                    // If not success, try parsing the Data as ServerErrors
                    response.data = response.data as ServerErrors;
                }
                return response as ServerResponseForUI<number | ServerErrors>;
            }),
            tap((response) => {
                console.log('Received JSON:', response);
            }),
            catchError(this.handleError)
        );
    }
    
      private handleError(error: HttpErrorResponse) {
        // Log the error to the console
        console.error('An error occurred:', error.error);
        return throwError(() => new Error('An error occurred; please try again later.'));
      }
}
