import { HttpClient } from '@angular/common/http';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router, ParamMap } from '@angular/router';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { environment } from 'src/environments/environment';
import { Subscription, Observable } from 'rxjs';
import { ChangePasswordUsingTokenRequest, Client, LoginRequest } from 'src/app/shared/api-client.generated';
import { NGXLogger } from 'ngx-logger';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import * as moment from 'moment';
import { BaseForm } from 'src/app/core/baseForm';

@Component({
  selector: 'app-password-reset',
  templateUrl: './password-reset.component.html',
  styleUrls: ['./password-reset.component.css']
})
export class PasswordResetComponent extends BaseForm implements OnInit {
  private token!: string;
  disableSubmitBecauseInvalidToken: boolean = false;
  requestBody: ChangePasswordUsingTokenRequest = new ChangePasswordUsingTokenRequest;
  hideNewPassword: boolean;
  hideNewPasswordRepeated: boolean;

  constructor(
    private activeRoute: ActivatedRoute,
    titleService: Title,
    router: Router,
    cdr: ChangeDetectorRef,
    logger: NGXLogger,
    spinnerService: SpinnerService,
    authService: AuthenticationService,
    notificationService: NotificationService,
    apiClient: Client
    ) 
  {
    super(router,logger, cdr,spinnerService,titleService,notificationService,authService, apiClient);
    this.pageTitle = 'Password Reset Request';
    this.hideNewPassword = true;
    this.hideNewPasswordRepeated = true;
  }

  protected override childSpecific_NgOnInit(): void {
    this.activeRoute.queryParamMap.subscribe((params: ParamMap) => {
      this.token = params.get('token') + '';
      //this.email = params.get('email') + '';

      if (!this.token){// || !this.email) {
        this.router.navigate(['/auth/login']);
      }

      this.createForm();
      this.verifyToken();
    });
  }

  private createForm() {
    // Initial values (insure you initialize even empty ones so requestBody serialize those fields too and awoid Bad request 400)
    this.requestBody = {
        ...this.requestBody,
        token: this.token,
        newPassword: '',
        newPasswordRepeated: '',
        toJSON:this.requestBody.toJSON,
        init:this.requestBody.init
    };

    //register controls with initial values
    this.form = new UntypedFormGroup({
      NewPassword: new UntypedFormControl(this.requestBody.newPassword, Validators.required),
      NewPasswordRepeated: new UntypedFormControl(this.requestBody.newPasswordRepeated, Validators.required)
    });

    //subsribe syncing from controls to requestBody
    this.form.get('NewPassword')?.valueChanges.subscribe(val => { this.requestBody.newPassword = val; });      
    this.form.get('NewPasswordRepeated')?.valueChanges.subscribe(val => { this.requestBody.newPasswordRepeated = val; });      
  }

  verifyToken(){
    console.log(this.requestBody);
    this.apiClient.verifyPasswordResetToken(this.token).subscribe({
      next:(serverResponseForUi) => {
        console.log(serverResponseForUi);
        if (!serverResponseForUi.isSuccess) {
          this.generalErrorMessage = serverResponseForUi.message || 'Your password reset request is invalid. Please go to Login page > Forgot password to create new one.';         
          this.notificationService.openSnackBar(serverResponseForUi.message || '');
          this.disableSubmitBecauseInvalidToken = true;
          this?.form?.get('NewPassword')?.disable();
          this?.form?.get('NewPasswordRepeated')?.disable();
        } else {
            if (!serverResponseForUi?.data) {
                const internalErrorMessage = 'Data is missing in the server response';
                console.error(internalErrorMessage);
                this.logger.error(internalErrorMessage);
                this.generalErrorMessage = 'Operation failed. Error code: CPTV1';
                this.notificationService.openSnackBar(this.generalErrorMessage);   
            } else 
            {//success                
                //do nothing
            }
        }
      },
      error:(error) => {
        this.generalErrorMessage = error;
        console.error(error);
        this.logger.error(error);
        this.notificationService.openSnackBar(error.error ? error.error : error.toString());
      }
    });
  }

  protected override childSpecific_ApiCall(): Observable<any> {
    // if (this.requestBody.newPassword !== this.requestBody.newPasswordRepeated) {
    //   this.notificationService.openSnackBar('Typed two passwords do not match');
    //   return;
    // }
    console.log(this.requestBody);
    return this.apiClient.changePasswordUsingToken(this.requestBody)
  }

  protected override childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any): boolean {
    this.passwordForInstantLogin = this?.requestBody?.newPassword || '';
    return serverResponseForUi?.data;
  }

  private passwordForInstantLogin='';
  private passwordResetResultMessage = '';

  protected override childSpecific_OnSuccess(serverResponseForUi: any): void {
    //this.authenticationService.setCurrentlyLoggedInUser(serverResponseForUi.data);
    //var message = 'User changed password via token. User email='+serverResponseForUi.data.eMail+', UserId=' + serverResponseForUi.data.id+", Token="+this.token;
    this.passwordResetResultMessage = serverResponseForUi.message;
    this.instantLogin(serverResponseForUi.data.eMail||'', this.passwordForInstantLogin||'');
  }

  // this.authService.passwordReset(this.email, this.token, password, passwordConfirm)
  //   .subscribe(
  //     () => {
  //       this.notificationService.openSnackBar('Your password has been changed.');
  //       this.router.navigate(['/auth/login']);
  //     },
  //     (error: any) => {
  //       this.notificationService.openSnackBar(error.error);
  //       this.loading = false;
  //     }
  //   );

  instantLoginFailedPostLogic()
  {
    this.passwordForInstantLogin = '';     
    this.router.navigate(['/auth/login']).then(() => {    
      this.notificationService.openSnackBar(this.passwordResetResultMessage || '');
    });  
  }

  async instantLogin(eMail: string, pass: string) {
    console.log("Instant login");
  
    let loginRequest: LoginRequest = {
      username: eMail,
      password: pass,
      init: function () {},
      toJSON: function () {
        return Object.assign({}, this); // If calling NSwag-generated requests on forms without same-name fields internal JSON.stringify returns {}. This will ensure proper default serialization instead.
      }
    };
  
    this.apiClient.loginAndReturnUserAndJwt(loginRequest).subscribe(
      {
        next: (serverResponseForUi)=>{
          if (!serverResponseForUi.isSuccess) {
            throw new Error(serverResponseForUi.message);
          } else {
            if (!serverResponseForUi?.data?.jwt || !serverResponseForUi.data.user) {
              console.log('Instant login: data.jwt or data.user is missing in the server response');
              this.logger.error('Instant login: data.jwt or data.user is missing in the server response');
              this.instantLoginFailedPostLogic();
            } else {
              // Success
              console.log('received jwt from server. Logging in at client...');
              console.log(serverResponseForUi);

              this.authenticationService.login(
                serverResponseForUi.data?.user,
                serverResponseForUi.data.jwt,
                serverResponseForUi.data.jwtExpirationMomentAsUtc
              );
       
              this.router.navigate(['/dashboard']);
              this.notificationService.openSnackBar('Password changed!' || '');
            }
          }
        },
        error: (error)=>{
          console.error(error);
          this.logger.error(error);
          this.instantLoginFailedPostLogic();
        }
      });
  }

  getInstanceName(): string {
    return environment.INSTANCE_NAME;
  }

  protected goToHomePage() {
    this.router.navigate(['/']);
  }

  cancel() {
    this.router.navigate(['/auth/login']);
  }
}
