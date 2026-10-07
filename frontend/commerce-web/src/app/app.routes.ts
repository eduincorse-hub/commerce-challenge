import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { UploadComponent } from './pages/upload/upload.component';
import { ProcessComponent } from './pages/process/process.component';
import { ErrorsComponent } from './pages/errors/errors.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'upload', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'upload', component: UploadComponent, canActivate: [authGuard] },
  { path: 'process', component: ProcessComponent, canActivate: [authGuard] },
  { path: 'errors', component: ErrorsComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: 'upload' }
];