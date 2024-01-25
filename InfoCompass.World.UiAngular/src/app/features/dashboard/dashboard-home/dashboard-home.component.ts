import { Component, OnInit } from '@angular/core';
import { NotificationService } from 'src/app/core/services/notification.service';
import { Title } from '@angular/platform-browser';
import { NGXLogger } from 'ngx-logger';
import { AuthenticationService } from 'src/app/core/services/auth.service';
import { User } from 'src/app/shared/api-client.generated';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-dashboard-home',
  templateUrl: './dashboard-home.component.html',
  styleUrls: ['./dashboard-home.component.css']
})
export class DashboardHomeComponent implements OnInit {
  currentVisitor:User|null=null;
  currentlyLoggedInUser:User|null=null;

  constructor(private notificationService: NotificationService,
    private authService: AuthenticationService,
    private titleService: Title,
    private logger: NGXLogger) {
  }

  ngOnInit() {
    this.titleService.setTitle(environment.INSTANCE_NAME+" - Dashboard");
    this.logger.log('Dashboard loaded');

    this.currentVisitor = this.authService.getCurrentVisitor();
    this.currentlyLoggedInUser = this.authService.getCurrentlyLoggedInUser();

    setTimeout(() => {
      //this.notificationService.openSnackBar('Welcome!');
    });
  }

  getUserFirstNameAndLastNameOrEMail(){
    const r = (((this?.currentlyLoggedInUser?.firstName||'')+' '+(this?.currentlyLoggedInUser?.lastName||'')).trim())||this.currentlyLoggedInUser?.eMail||'';
    return r;
  }
}
