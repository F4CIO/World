import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseForm } from 'src/app/core/baseForm';
import { ChangePasswordRequest, Client, User } from 'src/app/shared/api-client.generated';
import { Title } from '@angular/platform-browser';
import { Observable } from 'rxjs';


@Component({
  selector: 'app-profile-edit-details',
  templateUrl: './profile-edit-details.component.html',
  styleUrls: ['./profile-edit-details.component.css']
})
export class ProfileEditDetailsComponent extends BaseForm implements OnInit {
  private requestBody: User = new User;

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
  }

  protected override childSpecific_NgOnInit(): void {
    // Initial values (insure you initialize even empty ones so requestBody serialize those fields too and awoid Bad request 400)
    this.requestBody = {
      ...this.requestBody,
      id: this.authenticationService.getCurrentlyLoggedInUser()?.id,
      firstName: this.authenticationService.getCurrentlyLoggedInUser()?.firstName,
      lastName: this.authenticationService.getCurrentlyLoggedInUser()?.lastName,
      eMail: this.authenticationService.getCurrentlyLoggedInUser()?.eMail || '',
      note: this.authenticationService.getCurrentlyLoggedInUser()?.note || '',
      toJSON:this.requestBody.toJSON,
      init:this.requestBody.init
    };

    //register controls with initial values
    this.form = new UntypedFormGroup({      
      FirstName: new UntypedFormControl(this.requestBody.firstName, Validators.required),
      LastName: new UntypedFormControl(this.requestBody.lastName, Validators.required)
    });

    //subsribe syncing from controls to requestBody    
    this.form.get('FirstName')?.valueChanges.subscribe(val => { this.requestBody.firstName = val; });  
    this.form.get('LastName')?.valueChanges.subscribe(val => { this.requestBody.lastName = val; });      
  }

  protected override childSpecific_ApiCall(): Observable<any> {    
    console.log(this.requestBody);
    return this.apiClient.editDetails(this.requestBody)
  }

  protected override childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any): boolean {
    return serverResponseForUi?.data?.id != null;
  }

  protected override childSpecific_OnSuccess(serverResponseForUi: any): void {
    //this.authenticationService.setCurrentlyLoggedInUser(serverResponseForUi.data);
    this.requestBody = serverResponseForUi.data;
    this.authenticationService.setCurrentVisitor(serverResponseForUi.data);
    this.authenticationService.setCurrentlyLoggedInUser(serverResponseForUi.data);


    var message = 'User changed details. User email='+serverResponseForUi.data.eMail+', UserId=' + serverResponseForUi.data.id;
    console.log(message);
    this.logger.info(message);
    this.notificationService.openSnackBar(serverResponseForUi.message);
    //window.location.reload();
    this.reloadCurrentRoute();
  }

  reloadCurrentRoute() {
    let currentUrl = this.router.url;
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([currentUrl]);
    });
  }
}
