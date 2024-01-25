import { HttpClient } from '@angular/common/http';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router, ParamMap } from '@angular/router';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { environment } from 'src/environments/environment';
import { Subscription, Observable } from 'rxjs';
import { ChangePasswordUsingTokenRequest, Client, LoginRequest, RegisterRequest, User } from 'src/app/shared/api-client.generated';
import { NGXLogger } from 'ngx-logger';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import * as moment from 'moment';
import { BaseForm } from 'src/app/core/baseForm';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent extends BaseForm implements OnInit {
  requestBody: RegisterRequest = new RegisterRequest;
  hideNewPassword: boolean;
  hideNewPasswordRepeated: boolean;
  requestSucceeded: boolean = false;

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
    this.pageTitle = 'Register New Account';
    this.hideNewPassword = true;
    this.hideNewPasswordRepeated = true;
  }

  protected override childSpecific_NgOnInit(): void {
    console.log('RegisterComponent ngOnInit');
    // Initial values (insure you initialize even empty ones so requestBody serialize those fields too and awoid Bad request 400)
    this.requestBody = {
        ...this.requestBody,
        firstName: "",
        lastName: "",
        eMail: "",
        subscribeMe: false, 
        newPassword: '', 
        newPasswordRepeated: '',
        iAgreeToTerms: false,
        toJSON:this.requestBody.toJSON,
        init:this.requestBody.init
    };

    //register controls with initial values
    this.form = new UntypedFormGroup({    
      FirstName: new UntypedFormControl(this.requestBody.firstName),//, Validators.required),      
      LastName: new UntypedFormControl(this.requestBody.lastName),  
      EMail: new UntypedFormControl(this.requestBody.eMail),    
      SubscribeMe: new UntypedFormControl(this.requestBody.subscribeMe),  
      NewPassword: new UntypedFormControl(this.requestBody.newPassword),//, Validators.required),
      NewPasswordRepeated: new UntypedFormControl(this.requestBody.newPasswordRepeated),//, Validators.required)
      IAgreeToTerms: new UntypedFormControl(this.requestBody.iAgreeToTerms),       
    });

    //subsribe syncing from controls to requestBody         
    this.form.get('FirstName')?.valueChanges.subscribe(val => { this.requestBody.firstName = val; });  
    this.form.get('LastName')?.valueChanges.subscribe(val => { this.requestBody.lastName = val; });  
    this.form.get('EMail')?.valueChanges.subscribe(val => { this.requestBody.eMail = val; });      
    this.form.get('SubscribeMe')?.valueChanges.subscribe(val => { this.requestBody.subscribeMe = val; });    
    this.form.get('NewPassword')?.valueChanges.subscribe(val => { this.requestBody.newPassword = val; });      
    this.form.get('NewPasswordRepeated')?.valueChanges.subscribe(val => { this.requestBody.newPasswordRepeated = val; });   
    this.form.get('IAgreeToTerms')?.valueChanges.subscribe(val => { this.requestBody.iAgreeToTerms = val; }); 
  }

  protected override childSpecific_ApiCall(): Observable<any> {
    // if (this.requestBody.newPassword !== this.requestBody.newPasswordRepeated) {
    //   this.notificationService.openSnackBar('Typed two passwords do not match');
    //   return;
    // }
    console.log('Starting registration process with:');
    console.log(this.requestBody);
    return this.apiClient.startRegistrationProcess(this.requestBody)
  }

  protected override childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any): boolean {
    return serverResponseForUi?.data as User != null;
  }

  protected override childSpecific_OnSuccess(serverResponseForUi: any): void {
    this.authenticationService.setCurrentVisitor(serverResponseForUi.data);
    
    var message = 'User started registration process. User email='+serverResponseForUi.data.eMail+', UserId=' + serverResponseForUi.data.id;
    console.log(message);
    this.logger.error(message);

    this.requestSucceeded = true;
    this.notificationService.openSnackBar('Success!');
    // this.router.navigate(['/auth/login']).then(() => {    
    //   this.notificationService.openSnackBar(serverResponseForUi?.data.message || 'Check your inbox and confirm registration!');
    // });  
  }

  getInstanceName(): string {
    return environment.INSTANCE_NAME;
  }

  cancel() {
    this.router.navigate(['/auth/login']);
  }
     
  protected goToHomePage() {
    this.router.navigate(['/']);
  }

  protected goToLoginPage() {
    this.router.navigate(['/auth/login']);
  }
}
