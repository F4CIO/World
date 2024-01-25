import { ChangeDetectorRef, Component, Injectable, OnInit, enableProdMode } from '@angular/core';
import { Router } from '@angular/router';
import { UntypedFormControl, Validators, UntypedFormGroup } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { environment } from 'src/environments/environment';
import { Subscription, Observable } from 'rxjs';
import { Client, LoginRequest } from 'src/app/shared/api-client.generated';
import { NGXLogger } from 'ngx-logger';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import * as moment from 'moment';

@Injectable()
export abstract class BaseForm implements OnInit {
    protected form!: UntypedFormGroup;
    protected loading!: boolean;    
    protected API_BASE_URL:string = environment.API_BASE_URL;
    protected disableSubmit!: boolean;
    protected generalErrorMessage:string = '';  
    protected subscriptions: Subscription[] = [];    
    //protected requestBody: any;
    protected pageTitle='PageTitle';

    constructor(protected router: Router,
        protected logger: NGXLogger,
        protected cdr: ChangeDetectorRef,
        protected spinnerService: SpinnerService,
        private titleService: Title,
        protected notificationService: NotificationService,
        protected authenticationService: AuthenticationService,
        protected apiClient: Client
        ) {
    }

    public ngOnInit() {
      this.logger.log('Loading page '+this.pageTitle+' ...');      
      this.titleService.setTitle(environment.INSTANCE_NAME+" - "+this.pageTitle);

      this.childSpecific_NgOnInit();

      this.subscriptions.push(this.form.statusChanges.subscribe(status => {
        if (status === 'VALID') {
          this.generalErrorMessage = '';
          this.disableSubmit = false;
        }else{
          this.disableSubmit = true;
        }
      }));          
      this.subscriptions.push(this.spinnerService.visibility.subscribe((value) => { this.disableSubmit = value; }));

      this.logger.log(this.pageTitle+' page loaded.');
    }

    protected abstract childSpecific_NgOnInit():void;

    public ngOnDestroy() {
      if(this.subscriptions){
        this.subscriptions.forEach(s=>s.unsubscribe());
      }
    }

    protected submitForm() {      
        this.disableSubmit = true;
        this.generalErrorMessage = '';
        this.loading = true;

        //console.log(this.requestBody);
        
        this.childSpecific_ApiCall()
          .subscribe(
            (serverResponseForUi) => {
              console.log(serverResponseForUi);
              if (!serverResponseForUi.isSuccess) {
                //this.generalErrorMessage = serverResponseForUi.message || 'Please fill in form properly.';
                if (serverResponseForUi.errors) {
                  console.log(serverResponseForUi.errors);
                  for (var err of serverResponseForUi.errors) {
                    const control = this.form.get(err.key || '');
                    if (control) {
                      control.setErrors({ controlSpecificError: err.friendlyMessage });
                      control.markAsTouched();
                    }
                  }
                  this.cdr.detectChanges();
                }
                // this.notificationService.openSnackBar(this.generalErrorMessage);
                // this.disableSubmit = false;
                // this.loading = false;
                this.onError(serverResponseForUi.message || 'Please fill in form properly.');
              } else {
                if (!this.childSpecific_IsServerResponseDataStructureValid(serverResponseForUi)) {
                  this.onError('Operation failed. Data is missing in the server response');
                } else {//success
                  this.onSuccess(serverResponseForUi);                
                }
              }
            },
            (error) => {
              this.onError(error);
            }
          );

        // this.authenticationService
        //     .login(email.toLowerCase(), password)
        //     .subscribe(
        //         data => {
        //             if (rememberMe) {
        //                 localStorage.setItem('savedUserEmail', email);
        //             } else {
        //                 localStorage.removeItem('savedUserEmail');
        //             }
        //             this.router.navigate(['/']);
        //         },
        //         error => {
        //             this.notificationService.openSnackBar(error.error);
        //             this.loading = false;
        //         }
        //     );
    }

    // This this method should be overridden by child classes.
    // For example in child class implementation could be:
    // return apiClient.Login(email, password);
    protected abstract childSpecific_ApiCall(): Observable<any>;

    // This this method should be overridden by child classes.
    // For example implementation could be:
    // return serverResponseForUi?.data?.jwt && serverResponseForUi?.data?.user;
    protected abstract childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any):boolean;

    private onSuccess(serverResponseForUi: any) {
      var message = 'Response from server='+serverResponseForUi.message;
      console.log(message);
      this.logger.info(message);
      this.form.reset();
      this.childSpecific_OnSuccess(serverResponseForUi);
      this.disableSubmit = false;
      this.loading = false;
    }

    // This this method should be overridden by child classes.
    protected abstract childSpecific_OnSuccess(serverResponseForUi: any):void;
  
    private onError(error: any) {
      this.generalErrorMessage = error;
      console.error(error);
      this.logger.error(error);
      this.childSpecific_OnError(error);
      this.notificationService.openSnackBar(error.error ? error.error : error.toString());
      this.disableSubmit = false;
      this.loading = false;
    }

    // This this method could be overridden by child classes.
    protected childSpecific_OnError(error: any):void{      
    }
}
