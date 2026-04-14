import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/Register/register.component';
import { AuthGuard } from './core/guards/auth.guard';
import { FileFormComponent } from './pages/file/file-form/file-form.component';
import { FileDetailsComponent } from './pages/file/file-details/file-details.component';
import { FileListComponent } from './pages/file/file-list/file-list.component';
import { AppMainComponent } from './app-main.component';

export const routes: Routes = [  
  {
    path: '',
    component: AppMainComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'files/upload', component: FileFormComponent, canActivate: [AuthGuard] },  
      { path: 'file/:id', component: FileDetailsComponent },  

    ]
  },
  {
    path: 'files',
    component: FileListComponent,
    canActivate: [AuthGuard]
  },
];

/*
export const routes: Routes = [  
  {
    path: '',
    component: HomeComponent
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'files',
    component: FileListComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'files/upload',
    component: FileFormComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'file/:id',
    component: FileDetailsComponent    
  }
];
*/
