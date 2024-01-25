import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AuthGuard } from './core/guards/auth.guard';
import { NoGuard } from './core/guards/no.guard';

const appRoutes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule),
    canActivate: [NoGuard]
  },
  // {
  //   path: 'dashboard',
  //   loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule),
  //   canActivate: [NoGuard]
  // }, 
  {
    path: 'library',
    loadChildren: () => import('./features/library/library.module').then(m => m.LibraryModule),
    canActivate: [NoGuard]
  },
  {
    path: 'account',
    loadChildren: () => import('./features/account/account.module').then(m => m.AccountModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'terms',
    loadChildren: () => import('./features/terms/terms.module').then(m => m.TermsModule),
    canActivate: [NoGuard]
  },
  {
    path: 'typography',
    loadChildren: () => import('./features/typography/typography.module').then(m => m.TypographyModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'about',
    loadChildren: () => import('./features/about/about.module').then(m => m.AboutModule),
    canActivate: [NoGuard]
  },
  {
    path: 'dashboard',
    redirectTo: '/',
    pathMatch: 'full'
  },
  {
    path: '',
    loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule),
    canActivate: [NoGuard]
  },
  {
    path: '**',
    redirectTo: '/',
    pathMatch: 'full'
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(appRoutes)
  ],
  exports: [RouterModule],
  providers: []
})
export class AppRoutingModule { }
