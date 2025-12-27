import { Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { TeamListComponent } from './components/team-list/team-list.component';
import { CreateTeamComponent } from './components/create-team/create-team.component';
import { TeamDetailComponent } from './components/team-detail/team-detail.component';
import { JoinTeamComponent } from './components/join-team/join-team.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/teams',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'teams',
    component: TeamListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'teams/create',
    component: CreateTeamComponent,
    canActivate: [authGuard]
  },
  {
    path: 'teams/:id',
    component: TeamDetailComponent,
    canActivate: [authGuard]
  },
  {
    path: 'join/:token',
    component: JoinTeamComponent,
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: '/teams'
  }
];
