import { User } from './../../shared/api-client.generated';
import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { delay, map } from 'rxjs/operators';
import * as jwt_decode from 'jwt-decode';
import * as moment from 'moment';

import { environment } from '../../../environments/environment';
import { of, EMPTY } from 'rxjs';
import { NGXLogger } from 'ngx-logger';

@Injectable({
    providedIn: 'root'
})
export class AuthenticationService {    
    constructor(private http: HttpClient,
        @Inject('LOCALSTORAGE') private localStorage: Storage,
        private logger: NGXLogger) {
    }

    // login(email: string, password: string) {
    //     return of(true)
    //         .pipe(delay(1000),
    //             map((/*response*/) => {
    //                 // set token property
    //                 // const decodedToken = jwt_decode(response['token']);

    //                 // store email and jwt token in local storage to keep user logged in between page refreshes
    //                 this.localStorage.setItem('currentlyLoggedInUser', JSON.stringify({
    //                     token: 'aisdnaksjdn,axmnczm',
    //                     isAdmin: true,
    //                     email: 'john.doe@gmail.com',
    //                     id: '12312323232',
    //                     alias: 'john.doe@gmail.com'.split('@')[0],
    //                     expiration: moment().add(1, 'days').toDate(),
    //                     fullName: 'John Doe'
    //                 }));
    //                 return true;
    //             }));
    // }

    login(user:User, jwt:string, expirationMomentAsUtc:Date | undefined){
        this.setCurrentJwt(jwt, expirationMomentAsUtc || moment.utc().add(1,'years').toDate());
        this.setCurrentlyLoggedInUser(user);
    }

    logout(): void {
        this.localStorage.removeItem('currentVisitor');
        this.localStorage.removeItem('currentlyLoggedInUser');
        this.localStorage.removeItem('currentJwt');
        this.localStorage.removeItem('currentJwtExpitationMomentAsUtc');
    }

    setCurrentVisitor(user:User){
        this.localStorage.setItem("currentVisitor", JSON.stringify(user));
        console.log(JSON.stringify(user));
        const currentVisitorAsJson = this.localStorage.getItem('currentVisitor');
        if(currentVisitorAsJson){
        const t: User = JSON.parse(currentVisitorAsJson);
        console.log(t);
        }else{
            console.log('currentVisitorAsJson is null');
        }
    }

    getCurrentVisitor(): User | null {
        const currentVisitorAsJson = this.localStorage.getItem('currentVisitor');
        console.log(currentVisitorAsJson);
        if (currentVisitorAsJson) {
            try {
                const user: User = JSON.parse(currentVisitorAsJson);
                return user;
            } catch (e) {
                console.error('Error parsing currentVisitor from local storage', e);
            }
        }
        return null;
    }

    setCurrentlyLoggedInUser(user:User){
        this.localStorage.setItem("currentlyLoggedInUser", JSON.stringify(user));
        this.setCurrentVisitor(user);//keep in sync

        var message = 'User logged in. User email='+user.eMail+', UserId=' + user.id;
        console.log(message);
        this.logger.info(message);
    }

    getCurrentlyLoggedInUser(): User | null {
        const currentUserAsJson = this.localStorage.getItem('currentlyLoggedInUser');
        if (currentUserAsJson) {
            try {
                const user: User = JSON.parse(currentUserAsJson);
                return user;
            } catch (e) {
                console.error('Error parsing currentUser from local storage', e);
            }
        }
        return null;
    }

    getCurrentJwt(): any {
        return this.localStorage.getItem('currentJwt');
    }

    getCurrentJwtExpitationMomentAsUtc(): Date | null{
        const currentJwtExpirationMomentAsUtcAsJson = this.localStorage.getItem('currentJwtExpitationMomentAsUtc');
        if (currentJwtExpirationMomentAsUtcAsJson) {
            try {
                const r: Date = JSON.parse(currentJwtExpirationMomentAsUtcAsJson);
                return r;
            } catch (e) {
                console.error('Error parsing currentJwtExpitationMomentAsUtc from local storage', e);
            }
        }
        return null;
    }

    private setCurrentJwt(jwt:string, expirationMomentAsUtc:Date){
        this.localStorage.setItem('currentJwt', jwt);
        this.localStorage.setItem('currentJwtExpitationMomentAsUtc', JSON.stringify(expirationMomentAsUtc));
        
        console.log('jwt set. jwt length:'+jwt.length);
        this.logger.info('jwt set. jwt length:'+jwt.length);
    }

    passwordResetRequest(email: string) {
        return of(true).pipe(delay(1000));
    }

    changePassword(email: string, currentPwd: string, newPwd: string) {
        return of(true).pipe(delay(1000));
    }

    passwordReset(email: string, token: string, password: string, confirmPassword: string): any {
        return of(true).pipe(delay(1000));
    }
}
