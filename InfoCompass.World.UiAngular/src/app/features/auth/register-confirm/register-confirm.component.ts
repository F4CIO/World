import { ActivatedRoute, Router, ParamMap } from '@angular/router';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { Observable } from 'rxjs';
import { Client } from 'src/app/shared/api-client.generated';
import { NGXLogger } from 'ngx-logger';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import { BaseForm } from 'src/app/core/baseForm';
import { UntypedFormGroup } from '@angular/forms';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-register-confirm',
  templateUrl: './register-confirm.component.html',
  styleUrls: ['./register-confirm.component.css']
})
export class RegisterConfirmComponent extends BaseForm implements OnInit {
  private token!: string;
  protected executed: boolean = false;

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
    this.pageTitle = 'Register Confirm';
  }

  protected override childSpecific_NgOnInit(): void {
    this.activeRoute.queryParamMap.subscribe((params: ParamMap) => {
      this.token = params.get('token') + '';

      if (!this.token){
        this.router.navigate(['/auth/login']);
      }

      this.form = new UntypedFormGroup({
      });

      this.submitForm();
    });
  }

  protected override childSpecific_ApiCall(): Observable<any> {
    console.log('Finishing registration with token: '+this?.token || '');
    return this.apiClient.finishRegistrationProcessAndReturnUserAndJwt(this.token);
  }

  protected override childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any): boolean {
    return serverResponseForUi?.data?.jwt;
  }

  protected override childSpecific_OnSuccess(serverResponseForUi: any): void {        
    console.log('received jwt from server. Logging in at client...');
    console.log(serverResponseForUi);

    this.executed = true;

    this.authenticationService.login(
      serverResponseForUi.data?.user,
      serverResponseForUi.data.jwt,
      serverResponseForUi.data.jwtExpirationMomentAsUtc
    );

    this.router.navigate(['/dashboard']);
    this.notificationService.openSnackBar('Welcome!' || '');
  }

  protected override childSpecific_OnError(serverResponseForUi: any): void {
    this.executed = true;
  }

  getInstanceName(): string {
    return environment.INSTANCE_NAME;
  }

  protected goToHomePage() {
    this.router.navigate(['/']);
  }

  goToRegisterPage() {
    this.router.navigate(['/auth/register']);
  }

  goToLoginPage() {
    this.router.navigate(['/auth/login']);
  }
}
