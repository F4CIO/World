import { Injectable } from '@angular/core';
import { Router, CanActivate } from '@angular/router';
import * as moment from 'moment';

import { AuthenticationService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private router: Router,
        private notificationService: NotificationService,
        private authService: AuthenticationService) { }

    canActivate() {
        const user = this.authService.getCurrentlyLoggedInUser();
        const jwtExpirationMomentAsUtc = this.authService.getCurrentJwtExpitationMomentAsUtc();

        if (user && jwtExpirationMomentAsUtc) {

            if (moment.utc() < moment(jwtExpirationMomentAsUtc)) {
                return true;
            } else {
                this.notificationService.openSnackBar('Your session has expired');
                this.router.navigate(['generator']);
                return false;
            }
        }

        this.router.navigate(['generator']);
        return false;
    }
}
