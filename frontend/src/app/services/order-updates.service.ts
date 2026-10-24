import { Injectable } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState
} from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { AdminOrder } from '../models/api.models';
import { environment } from '../../environments/environment';

export type OrderConnectionState =
  | 'connecting'
  | 'live'
  | 'reconnecting'
  | 'disconnected';

@Injectable({ providedIn: 'root' })
export class OrderUpdatesService {
  private readonly connection: HubConnection;
  private readonly orderChangedSubject = new Subject<AdminOrder>();
  private readonly connectionStateSubject =
    new BehaviorSubject<OrderConnectionState>('disconnected');

  readonly orderChanged$ = this.orderChangedSubject.asObservable();
  readonly connectionState$ = this.connectionStateSubject.asObservable();

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl(`${environment.apiBaseUrl}/hubs/orders`)
      .withAutomaticReconnect()
      .build();

    this.connection.on('OrderChanged', (order: AdminOrder) => {
      this.orderChangedSubject.next(order);
    });
    this.connection.onreconnecting(() => {
      this.connectionStateSubject.next('reconnecting');
    });
    this.connection.onreconnected(() => {
      this.connectionStateSubject.next('live');
    });
    this.connection.onclose(() => {
      this.connectionStateSubject.next('disconnected');
    });
  }

  async connect(): Promise<void> {
    if (this.connection.state === HubConnectionState.Connected) {
      this.connectionStateSubject.next('live');
      return;
    }

    this.connectionStateSubject.next('connecting');
    try {
      await this.connection.start();
      this.connectionStateSubject.next('live');
    } catch (error) {
      this.connectionStateSubject.next('disconnected');
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    await this.connection.stop();
  }
}
