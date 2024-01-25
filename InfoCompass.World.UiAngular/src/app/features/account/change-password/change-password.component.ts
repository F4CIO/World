import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseForm } from 'src/app/core/baseForm';
import { ChangePasswordRequest, Client } from 'src/app/shared/api-client.generated';
import { Title } from '@angular/platform-browser';
import { Observable } from 'rxjs';


@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent extends BaseForm implements OnInit {
  private requestBody: ChangePasswordRequest = new ChangePasswordRequest;
  hideCurrentPassword: boolean;
  hideNewPassword: boolean;

  constructor(
    private activeRoute: ActivatedRoute,
    titleService: Title,
    router: Router,
    cdr: ChangeDetectorRef,
    logger: NGXLogger,
    spinnerService: SpinnerService,
    authenticationService: AuthenticationService,
    notificationService: NotificationService,
    apiClient: Client
  ) {
      super(router,logger, cdr,spinnerService,titleService,notificationService,authenticationService, apiClient);
    this.hideCurrentPassword = true;
    this.hideNewPassword = true;
  }

  protected override childSpecific_NgOnInit(): void {
    // Initial values (insure you initialize even empty ones so requestBody serialize those fields too and awoid Bad request 400)
    this.requestBody = {
      ...this.requestBody,
      oldPassword: '',
      newPassword: '',
      newPasswordRepeated: '',
      toJSON:this.requestBody.toJSON,
      init:this.requestBody.init
    };

    //register controls with initial values
    this.form = new UntypedFormGroup({      
      OldPassword: new UntypedFormControl(this.requestBody.oldPassword, Validators.required),
      NewPassword: new UntypedFormControl(this.requestBody.newPassword, Validators.required),
      NewPasswordRepeated: new UntypedFormControl(this.requestBody.newPasswordRepeated, Validators.required)
    });

    //subsribe syncing from controls to requestBody    
    this.form.get('OldPassword')?.valueChanges.subscribe(val => { this.requestBody.oldPassword = val; });  
    this.form.get('NewPassword')?.valueChanges.subscribe(val => { this.requestBody.newPassword = val; });      
    this.form.get('NewPasswordRepeated')?.valueChanges.subscribe(val => { this.requestBody.newPasswordRepeated = val; });    
  }

  protected override childSpecific_ApiCall(): Observable<any> {
    // if (this.requestBody.newPassword !== this.requestBody.newPasswordRepeated) {
    //   this.notificationService.openSnackBar('Typed two passwords do not match');
    //   return;
    // }
    this.requestBody.userId = this.authenticationService.getCurrentlyLoggedInUser()?.id;
    console.log(this.requestBody);
    return this.apiClient.changePassword(this.requestBody)
  }

  protected override childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any): boolean {
    return serverResponseForUi?.data;
  }

  protected override childSpecific_OnSuccess(serverResponseForUi: any): void {
    //this.authenticationService.setCurrentlyLoggedInUser(serverResponseForUi.data);
    var message = 'User changed password. User email='+serverResponseForUi.data.eMail+', UserId=' + serverResponseForUi.data.id;
    console.log(message);
    this.logger.info(message);
    this.notificationService.openSnackBar('Password changed!' || '');
    this.reloadCurrentRoute();
  }

  reloadCurrentRoute() {
    let currentUrl = this.router.url;
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([currentUrl]);
    });
  }
  // changePassword() {
  //   if (this.newPassword !== this.newPasswordConfirm) {
  //     this.notificationService.openSnackBar('New passwords do not match.');
  //     return;
  //   }

  //   const email = this.authService.getCurrentlyLoggedInUser()?.eMail;

  //   if(!email){
  //     this.router.navigate(['auth/login']);
  //   }else{
  //   this.authService.changePassword(email, this.currentPassword, this.newPassword)
  //     .subscribe(
  //       data => {
  //         this.logger.info(`User ${email} changed password.`);
  //         this.form.reset();
  //         this.notificationService.openSnackBar('Your password has been changed.');
  //       },
  //       error => {
  //         this.notificationService.openSnackBar(error.error);
  //       }
  //     );
  //   }
  // }
}
