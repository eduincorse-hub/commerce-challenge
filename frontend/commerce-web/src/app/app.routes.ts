import { Routes } from '@angular/router';
import { UploadComponent } from './pages/upload/upload.component';
import { ProcessComponent } from './pages/process/process.component';
import { ErrorsComponent } from './pages/errors/errors.component';

export const routes: Routes = [
  { path: '', redirectTo: 'upload', pathMatch: 'full' },
  { path: 'upload', component: UploadComponent },
  { path: 'process', component: ProcessComponent },
  { path: 'errors', component: ErrorsComponent }
];