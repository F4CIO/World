import { Component, OnInit, ChangeDetectorRef, OnDestroy, AfterViewInit } from '@angular/core';
import { MediaMatcher } from '@angular/cdk/layout';
import { timer } from 'rxjs';
import { Subscription } from 'rxjs';

 import { AuthenticationService } from 'src/app/core/services/auth.service';
import { SpinnerService } from '../../core/services/spinner.service';
import { AuthGuard } from 'src/app/core/guards/auth.guard';
import { User } from '../api-client.generated';
import { environment } from 'src/environments/environment';

@Component({
    selector: 'app-layout',
    templateUrl: './layout.component.html',
    styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit, OnDestroy, AfterViewInit {

    private _mobileQueryListener: () => void;
    mobileQuery: MediaQueryList;
    showSpinner: boolean = false;
    userName: string = "";
    currentVisitor:User|null=null;
    currentlyLoggedInUser:User|null=null;

    private autoLogoutSubscription: Subscription = new Subscription;

    constructor(private changeDetectorRef: ChangeDetectorRef,
        private media: MediaMatcher,
        public spinnerService: SpinnerService,
        private authService: AuthenticationService,
        private authGuard: AuthGuard) {

        this.mobileQuery = this.media.matchMedia('(max-width: 1000px)');
        this._mobileQueryListener = () => changeDetectorRef.detectChanges();
        // tslint:disable-next-line: deprecation
        this.mobileQuery.addListener(this._mobileQueryListener);
    }

    ngOnInit(): void {
        this.currentVisitor = this.authService.getCurrentVisitor();
        this.currentlyLoggedInUser = this.authService.getCurrentlyLoggedInUser();

        if(this.currentlyLoggedInUser){
            this.userName = ((this.currentlyLoggedInUser?.firstName||'')+' '+(this.currentlyLoggedInUser?.lastName||'')).trim();
        }
        else if(this.currentVisitor){
            //this.userName = ((this.currentVisitor?.firstName||'')+' '+(this.currentVisitor?.lastName||'')).trim();
            this.userName = "";
        }
        else{
            this.userName = "";
        }
        

        // Auto log-out subscription
        // const timer$ = timer(2000, 5000);
        // this.autoLogoutSubscription = timer$.subscribe(() => {
        //     this.authGuard.canActivate();
        // });
    }

    ngOnDestroy(): void {
        // tslint:disable-next-line: deprecation
        this.mobileQuery.removeListener(this._mobileQueryListener);
        this.autoLogoutSubscription.unsubscribe();
    }

    ngAfterViewInit(): void {
        this.changeDetectorRef.detectChanges();
    }

    getInstanceName(): string {
        return environment.INSTANCE_NAME;
      }
}
