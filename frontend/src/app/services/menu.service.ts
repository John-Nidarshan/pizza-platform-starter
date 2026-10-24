import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MenuItem } from '../models/api.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly http = inject(HttpClient);

  getMenu(): Observable<MenuItem[]> {
    return this.http.get<MenuItem[]>(`${environment.apiBaseUrl}/api/menu`);
  }
}
