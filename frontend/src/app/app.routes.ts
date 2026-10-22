import { Routes } from '@angular/router';
import { AdminComponent } from './admin/admin.component';
import { HomeComponent } from './home.component';

export const routes: Routes = [
  { path: '', component: HomeComponent, title: 'Slice Club — Scan. Order. Enjoy.' },
  { path: 'admin', component: AdminComponent, title: 'Admin | Slice Club' },
  { path: '**', redirectTo: '' }
];
