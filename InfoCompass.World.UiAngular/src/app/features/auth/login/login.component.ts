import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UntypedFormControl, UntypedFormGroup } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { Client, LoginRequest } from 'src/app/shared/api-client.generated';
import { NGXLogger } from 'ngx-logger';
import { SpinnerService } from 'src/app/core/services/spinner.service';
import { BaseForm } from 'src/app/core/baseForm';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})
export class LoginComponent extends BaseForm implements OnInit {  
  requestBody: LoginRequest = new LoginRequest; 

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
      this.pageTitle = 'Login';
    }

    protected override childSpecific_NgOnInit(): void {
        this.authenticationService.logout();

        const savedUserEmail = localStorage.getItem('savedUserEmail');

        // Initial values (insure you initialize even empty ones so requestBody serialize those fields too and awoid Bad request 400)
        this.requestBody = {
            ...this.requestBody,
            username: savedUserEmail??'',
            password: '',
            toJSON:this.requestBody.toJSON,
            init:this.requestBody.init
        };

//         //!!!
// this.requestBody = {
//           ...this.requestBody,
//           username: 'f4cio1@gmail.com',
//           password: 'pomorandza',
//           toJSON:this.requestBody.toJSON,
//           init:this.requestBody.init
//       };

        //register controls with initial values
        this.form = new UntypedFormGroup({
            Username: new UntypedFormControl(this.requestBody.username),//, [Validators.required, Validators.email]),
            Password: new UntypedFormControl(this.requestBody.password),//, Validators.required),
            rememberMe: new UntypedFormControl(savedUserEmail !== null)
        });

        //subsribe syncing from controls to requestBody
        this.form.get('Username')?.valueChanges.subscribe(val => { this.requestBody.username = val; });      
        this.form.get('Password')?.valueChanges.subscribe(val => { this.requestBody.password = val; });    

if(this.requestBody.username=='f4cio1@gmail.com' && this.requestBody.password){
          this.submitForm();
      }
    }

    protected override childSpecific_ApiCall(): Observable<any> {
      console.log(this.requestBody.username);
      return this.apiClient.loginAndReturnUserAndJwt(this.requestBody);
    }

    protected override childSpecific_IsServerResponseDataStructureValid(serverResponseForUi: any): boolean {
      return serverResponseForUi?.data?.jwt && serverResponseForUi?.data?.user;
    }

    protected override childSpecific_OnSuccess(serverResponseForUi: any): void {
      this.authenticationService.login(serverResponseForUi.data?.user, serverResponseForUi.data.jwt, serverResponseForUi.data.jwtExpirationMomentAsUtc);
                      
      if (this.form.get('rerememberMe')) {
        localStorage.setItem('savedUserEmail', serverResponseForUi.data?.eMail||'');
      } else {
        localStorage.removeItem('savedUserEmail');
      }
      
      this.router.navigate(['/dashboard']).then(() => {
        this.notificationService.openSnackBar('Welcome!' || '');
      });
    }  

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
    getInstanceName(): string {
      return environment.INSTANCE_NAME;
    }

    protected resetPassword() {
        this.router.navigate(['/auth/password-reset-request']);
    }

    protected register() {
      this.router.navigate(['/auth/register']);
    }
        
    protected goToHomePage() {
      this.router.navigate(['/']);
    }
}
