import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AdminOrder,
  CreateOrderRequest,
  CreateOrderResponse
} from '../models/api.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);

  createOrder(request: CreateOrderRequest): Observable<CreateOrderResponse> {
    return this.http.post<CreateOrderResponse>(
      `${environment.apiBaseUrl}/api/orders`,
      request
    );
  }

  getOrders(): Observable<AdminOrder[]> {
    return this.http.get<AdminOrder[]>(`${environment.apiBaseUrl}/api/orders`);
  }

  confirmCounterPayment(orderId: string): Observable<AdminOrder> {
    return this.http.post<AdminOrder>(
      `${environment.apiBaseUrl}/api/orders/${orderId}/confirm-payment`,
      {}
    );
  }

  updateOrderStatus(
    orderId: string,
    status: 'Ready' | 'Completed'
  ): Observable<AdminOrder> {
    return this.http.put<AdminOrder>(
      `${environment.apiBaseUrl}/api/orders/${orderId}/status`,
      { status }
    );
  }
}
