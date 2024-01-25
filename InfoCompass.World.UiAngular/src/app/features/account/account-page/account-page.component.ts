import { Component, OnInit } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-account-page',
  templateUrl: './account-page.component.html',
  styleUrls: ['./account-page.component.css']
})
export class AccountPageComponent implements OnInit {

  constructor(private titleService: Title, private router: Router) { }

  ngOnInit() {
    this.titleService.setTitle(environment.INSTANCE_NAME+" - Account");
  }
  
  onTabChange(event: MatTabChangeEvent) {
    if (event.index === 2) { // Assuming 'Upgrade to Pro' is the second tab
      this.router.navigate(['/account/go-pro']);
    }
  }
}
