export interface MenuItem {
  id: number;
  name: string;
  description: string;
  category: string;
  price: number;
  imageUrl?: string | null;
  emoji?: string | null;
  available: boolean;
}

export interface TableInfo {
  tableNumber: number;
  active: boolean;
}

export interface OrderItemRequest {
  menuItemId: number;
  quantity: number;
}

export interface CreateOrderRequest {
  tableNumber: number;
  paymentMethod: 'online' | 'counter';
  items: OrderItemRequest[];
}

export interface CreateOrderResponse {
  id: string;
  tableNumber: number;
  total: number;
  paymentMethod: 'online' | 'counter';
  paymentStatus: string;
  orderStatus: string;
  createdAtUtc: string;
  message: string;
}
