import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnDestroy, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { AdminOrder } from '../models/api.models';
import { OrderService } from '../services/order.service';
import {
  OrderConnectionState,
  OrderUpdatesService
} from '../services/order-updates.service';

type OrderFilter = 'all' | 'payment' | 'preparing' | 'ready' | 'completed';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit, OnDestroy {
  private readonly orderService = inject(OrderService);
  private readonly orderUpdates = inject(OrderUpdatesService);
  private readonly destroyRef = inject(DestroyRef);

  orders: AdminOrder[] = [];
  filter: OrderFilter = 'all';
  connectionState: OrderConnectionState = 'disconnected';
  loading = true;
  updatingOrderId: string | null = null;
  errorMessage = '';
  connectionError = '';
  private wasReconnecting = false;
  private orderLoadRequest = 0;
  private readonly realtimeOrders = new Map<string, AdminOrder>();

  ngOnInit(): void {
    this.orderUpdates.orderChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(order => {
        this.realtimeOrders.set(order.id, order);
        this.upsertOrder(order);
      });

    this.orderUpdates.connectionState$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(state => {
        this.connectionState = state;
        if (state === 'reconnecting') {
          this.wasReconnecting = true;
        } else if (state === 'live') {
          this.connectionError = '';
          if (this.wasReconnecting) {
            this.wasReconnecting = false;
            this.loadOrders();
          }
        }
      });

    this.loadOrders();
    this.connectSignalR();
  }

  ngOnDestroy(): void {
    this.orderUpdates.disconnect().catch(error => {
      console.error('Could not disconnect the order updates hub.', error);
    });
  }

  get filteredOrders(): AdminOrder[] {
    return this.orders.filter(order => this.matchesFilter(order, this.filter));
  }

  countFor(filter: OrderFilter): number {
    return this.orders.filter(order => this.matchesFilter(order, filter)).length;
  }

  isAwaitingCounterPayment(order: AdminOrder): boolean {
    return order.paymentMethod === 'counter' &&
      order.paymentStatus !== 'Paid' &&
      order.orderStatus === 'AwaitingCounterPayment';
  }

  isAwaitingOnlinePayment(order: AdminOrder): boolean {
    return order.paymentMethod === 'online' &&
      order.paymentStatus !== 'Paid' &&
      order.orderStatus === 'AwaitingPayment';
  }

  statusClass(order: AdminOrder): string {
    return order.orderStatus.toLowerCase();
  }

  nextAction(order: AdminOrder): string | null {
    if (this.isAwaitingCounterPayment(order)) {
      return 'Confirm payment & start preparing';
    }
    if (order.paymentStatus === 'Paid' && order.orderStatus === 'Preparing') {
      return 'Mark ready';
    }
    if (order.paymentStatus === 'Paid' && order.orderStatus === 'Ready') {
      return 'Complete order';
    }
    return null;
  }

  performNextAction(order: AdminOrder): void {
    const action = this.nextAction(order);
    if (!action || this.updatingOrderId !== null) {
      return;
    }

    this.updatingOrderId = order.id;
    this.errorMessage = '';
    const request = this.isAwaitingCounterPayment(order)
      ? this.orderService.confirmCounterPayment(order.id)
      : this.orderService.updateOrderStatus(
          order.id,
          order.orderStatus === 'Preparing' ? 'Ready' : 'Completed'
        );

    request.subscribe({
      next: updatedOrder => {
        this.upsertOrder(updatedOrder);
        this.updatingOrderId = null;
      },
      error: (error: unknown) => {
        this.errorMessage = this.getErrorMessage(
          error,
          'Could not update this order. Refresh the page and try again.'
        );
        this.updatingOrderId = null;
      }
    });
  }

  retryConnection(): void {
    this.connectSignalR();
  }

  private connectSignalR(): void {
    this.connectionError = '';
    this.orderUpdates.connect().catch((error: unknown) => {
      this.connectionError = this.getErrorMessage(
        error,
        'Live updates are unavailable. Check that the API is running.'
      );
    });
  }

  loadOrders(): void {
    const requestId = ++this.orderLoadRequest;
    this.realtimeOrders.clear();
    this.loading = true;
    this.orderService.getOrders().subscribe({
      next: orders => {
        if (requestId !== this.orderLoadRequest) {
          return;
        }
        const ordersById = new Map(orders.map(order => [order.id, order]));
        for (const [orderId, order] of this.realtimeOrders) {
          ordersById.set(orderId, order);
        }
        this.orders = [...ordersById.values()].sort(
          (left, right) =>
            new Date(right.createdAtUtc).getTime() -
            new Date(left.createdAtUtc).getTime()
        );
        this.loading = false;
      },
      error: (error: unknown) => {
        if (requestId !== this.orderLoadRequest) {
          return;
        }
        this.errorMessage = this.getErrorMessage(
          error,
          'Could not load orders. Check that the API and database are running.'
        );
        this.loading = false;
      }
    });
  }

  private upsertOrder(order: AdminOrder): void {
    this.orders = [
      order,
      ...this.orders.filter(existing => existing.id !== order.id)
    ].sort(
      (left, right) =>
        new Date(right.createdAtUtc).getTime() -
        new Date(left.createdAtUtc).getTime()
    );
  }

  private matchesFilter(order: AdminOrder, filter: OrderFilter): boolean {
    switch (filter) {
      case 'payment':
        return order.paymentStatus !== 'Paid' &&
          (order.orderStatus === 'AwaitingPayment' ||
            order.orderStatus === 'AwaitingCounterPayment');
      case 'preparing':
        return order.orderStatus === 'Preparing';
      case 'ready':
        return order.orderStatus === 'Ready';
      case 'completed':
        return order.orderStatus === 'Completed';
      default:
        return true;
    }
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const apiMessage = error.error?.message;
      if (typeof apiMessage === 'string' && apiMessage.length > 0) {
        return apiMessage;
      }
    }
    if (error instanceof Error && error.message) {
      return error.message;
    }
    return fallback;
  }
}
