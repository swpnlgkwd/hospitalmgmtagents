import { Routes } from '@angular/router';
import { Login } from './login/login';
import { ShiftCalendarComponent } from './shift-calendar.component/shift-calendar.component';
import { AuthGuard } from './guards/auth-guard';
import { MainLayout } from './main-layout/main-layout';

// export const routes: Routes = [];
export const routes: Routes = [
  { path: 'login', component: Login },

  {
    path: 'calendar',
    component: MainLayout,
    canActivate: [AuthGuard]
  },

  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' }
];

