import { HttpClient } from '@angular/common/http';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router, ParamMap } from '@angular/router';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { environment } from 'src/environments/environment';
import { Subscription, Observable } from 'rxjs';
import { UserMessage, Client, LoginRequest, User } from 'src/app/shared/api-client.generated';
import { NGXLogger } from 'ngx-logger';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import * as moment from 'moment';
import { BaseForm } from 'src/app/core/baseForm';

@Component({
  selector: 'app-contact-us',
  templateUrl: './contact-us.component.html',
  styleUrls: ['./contact-us.component.css']
})
export class ContactUsComponent extends BaseForm implements OnInit {
  requestBody: UserMessage = new UserMessage;
  requestSucceeded: boolean = false;
  textAreaMinRows = 10;
  textAreaMaxRows = 20;

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
    this.pageTitle = 'Contact Us';
  }

  protected override childSpecific_NgOnInit(): void {
    console.log('ContactUsComponent ngOnInit');
    // Initial values (insure you initialize even empty ones so requestBody serialize those fields too and awoid Bad request 400)
    this.requestBody = {
        ...this.requestBody,
        id: 0,
        userId: this.authenticationService.getCurrentlyLoggedInUser()?.id || this.authenticationService.getCurrentVisitor()?.id || 0,
        userEMail: this.authenticationService.getCurrentlyLoggedInUser()?.eMail || this.authenticationService.getCurrentVisitor()?.eMail || '',
        momentOfCreation: undefined, 
        momentOfLastUpdate : undefined, 
        message: '',
        isRead: false,
        isVisible: false,
        note: '',
        threadId: 0,
        previousMessageId: 0,
        nextMessageId: 0,
        toJSON:this.requestBody.toJSON,
        init:this.requestBody.init
    };

    //register controls with initial values
    this.form = new UntypedFormGroup({    
      Message: new UntypedFormControl(this.requestBody.message),//, Validators.required),      
      UserEMail: new UntypedFormControl(this.requestBody.userEMail),  
    });

    //subsribe syncing from controls to requestBody         
    this.form.get('Message')?.valueChanges.subscribe(val => { this.requestBody.message = val; });  
    this.form.get('UserEMail')?.valueChanges.subscribe(val => { this.requestBody.userEMail = val; });    
  }

  protected override childSpecific_ApiCall(): Observable<any> {
    this.requestBody.userId = this.authenticationService.getCurrentlyLoggedInUser()?.id || this.authenticationService.getCurrentVisitor()?.id || 0,
    console.log('Posting usermessage:');
    console.log(this.requestBody);
    return this.apiClient.postUserMessage(this.requestBody)
  }

  protected override childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any): boolean {
    return serverResponseForUi?.data;
  }

  protected override childSpecific_OnSuccess(serverResponseForUi: any): void {
    
    var message = 'User posted message successfully. server response='+serverResponseForUi.data;
    console.log(message);
    this.logger.error(message);

    this.requestSucceeded = true;
    this.notificationService.openSnackBar('Sent!');
  }

  getInstanceName(): string {
    return environment.INSTANCE_NAME;
  }

  cancel() {
    this.router.navigate(['/dashboard']);
  }
     
  protected goToHomePage() {
    this.router.navigate(['/']);
  }

  protected goToLoginPage() {
    this.router.navigate(['/auth/login']);
  }
}
