import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AccountRoutingModule } from './account-routing.module';
import { AccountPageComponent } from './account-page/account-page.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { ProfileEditDetailsComponent } from './profile-edit-details/profile-edit-details.component';
import { ProfileDetailsComponent } from './profile-details/profile-details.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { GoProPageComponent } from './go-pro/go-pro.component';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    AccountRoutingModule
  ],
  declarations: [AccountPageComponent, ChangePasswordComponent, ProfileDetailsComponent, ProfileEditDetailsComponent, GoProPageComponent],
  exports: [AccountPageComponent]
})
export class AccountModule { }
