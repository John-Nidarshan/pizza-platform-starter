import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TableInfo } from '../models/api.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TableService {
  private readonly http = inject(HttpClient);

  getTable(tableNumber: number): Observable<TableInfo> {
    return this.http.get<TableInfo>(`${environment.apiBaseUrl}/api/tables/${tableNumber}`);
  }
}
