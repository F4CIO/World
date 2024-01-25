import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { NotificationService } from 'src/app/core/services/notification.service';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NGXLogger } from 'ngx-logger';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import { BaseForm } from 'src/app/core/baseForm';
import { Client } from 'src/app/shared/api-client.generated';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-password-reset-request',
  templateUrl: './password-reset-request.component.html',
  styleUrls: ['./password-reset-request.component.css']
})
export class PasswordResetRequestComponent extends BaseForm {  
  private email!: string;

  constructor(
    titleService: Title,
    router: Router,
    cdr: ChangeDetectorRef,
    logger: NGXLogger,
    spinnerService: SpinnerService,
    authService: AuthenticationService,
    notificationService: NotificationService,
    apiClient: Client
    ) {
      super(router,logger, cdr,spinnerService,titleService,notificationService,authService, apiClient);
      this.pageTitle = 'Password Reset Request';
  }

  protected override childSpecific_NgOnInit(): void {
    const savedUserEmail = localStorage.getItem('savedUserEmail');

    // Initial values (insure you initialize even empty ones so requestBody serialize those fields too and awoid Bad request 400)
    this.email = savedUserEmail || '';

    //register controls with initial values
    this.form = new UntypedFormGroup({
      EMail: new UntypedFormControl(this.email, [Validators.required, Validators.email])
    });

    //subsribe syncing from controls to requestBody
    this.form.get('EMail')?.valueChanges.subscribe((val: string) => { this.email = val?.toLowerCase(); });
  }

  protected override childSpecific_ApiCall(): Observable<any>
  {          
    console.log("Sending password reset request for email:"+this.email);
    return this.apiClient.createPasswordResetRequest(this.email)
  }

  protected override childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any):boolean {
    return true;
  }

  protected override childSpecific_OnSuccess(serverResponseForUi: any)
  {      
    this.router.navigate(['/auth/login']).then(() => {
      this.notificationService.openSnackBar(serverResponseForUi.message || '');
    });
  }

    // this.loading = true;
    // this.authService.passwordResetRequest(this.email)
    //   .subscribe(
    //     results => {
    //       this.router.navigate(['/auth/login']);
    //       this.notificationService.openSnackBar('Password verification mail has been sent to your email address.');
    //     },
    //     error => {
    //       this.loading = false;
    //       this.notificationService.openSnackBar(error.error);
    //     }
    //   );

  getInstanceName(): string {
    return environment.INSTANCE_NAME;
  }

  protected goToHomePage() {
    this.router.navigate(['/']);
  }

  protected cancel() {
    this.router.navigate(['/auth/login']);
  }
}
