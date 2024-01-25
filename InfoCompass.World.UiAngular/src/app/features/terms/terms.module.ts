import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TermsRoutingModule } from './terms-routing.module';
import { TermsComponent } from './terms/terms.component';
import { SharedModule } from 'src/app/shared/shared.module';


@NgModule({
  declarations: [TermsComponent],
  imports: [
    CommonModule,
    SharedModule,
    TermsRoutingModule
  ]
})
export class TermsModule { }
