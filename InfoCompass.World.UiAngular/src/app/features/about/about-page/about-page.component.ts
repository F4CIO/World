import { Component } from '@angular/core';
import { NotificationService } from 'src/app/core/services/notification.service';
import build from 'src/build';

@Component({
  selector: 'app-about-page',
  templateUrl: './about-page.component.html',
  styleUrls: ['./about-page.component.css']
})
export class AboutPageComponent {
  buildTime: string = '';

  constructor(private notificationService: NotificationService) { 
    
      this.buildTime ='Build time:'+ (build?.timestamp || '?');
  }

  showSnackBar(message:string) {
    this.notificationService.openSnackBar(message);
  }
}
