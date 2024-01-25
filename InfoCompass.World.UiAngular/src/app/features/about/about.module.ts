import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AboutRoutingModule } from './about-routing.module';
import { AboutPageComponent } from './about-page/about-page.component';
import { SharedModule } from '../../shared/shared.module';
import { ContactUsComponent } from './contact-us/contact-us.component';
import { FaqComponent } from './faq/faq.component';

@NgModule({
  declarations: [AboutPageComponent, ContactUsComponent, FaqComponent],
  imports: [
    CommonModule,
    SharedModule,
    AboutRoutingModule,
  ]
})
export class AboutModule { }
