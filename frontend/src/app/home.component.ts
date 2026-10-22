import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { MenuItem } from './models/api.models';
import { MenuService } from './services/menu.service';
import { OrderService } from './services/order.service';
import { TableService } from './services/table.service';

type BasketLine = {
  item: MenuItem;
  quantity: number;
};

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  private readonly menuService = inject(MenuService);
  private readonly tableService = inject(TableService);
  private readonly orderService = inject(OrderService);

  menu: MenuItem[] = [];
  loadingMenu = true;
  menuError = '';

  tableStatus: 'unselected'|'checking' | 'valid' | 'invalid' = 'unselected';
  tableNumber:number|null = null;
  tables=Array.from({ length: 7 }, (_, i) => i + 1);

  basket: BasketLine[] = [];
  categories = ['All', 'Pizza', 'Sides', 'Drinks', 'Desserts'];
  category = 'All';
  showBasket = false;

  message = '';
  paymentMethod: 'online' | 'counter' = 'online';
  submitting = false;

  ngOnInit(): void {
    this.loadMenu();
  const rawTableNumber = new URLSearchParams(window.location.search).get('table');
  if (rawTableNumber === null) {
    return;
  }
  if (!/^\d+$/.test(rawTableNumber)) {
    this.tableStatus = 'invalid';
    return;
  }
  const tableNumber = Number(rawTableNumber);

  if (!Number.isInteger(tableNumber) || tableNumber < 1 || tableNumber > 40) {
    this.tableStatus = 'invalid';
    return;
  }

  this.selectTable(tableNumber);
    // this.checkTable();
  }
  selectTable(tableNumber: number): void {
  this.tableNumber = tableNumber;
  this.checkTable(tableNumber);
}

  get filteredMenu(): MenuItem[] {
    return this.menu.filter(
      item => this.category === 'All' || item.category === this.category
    );
  }

  get itemCount(): number {
    return this.basket.reduce((count, line) => count + line.quantity, 0);
  }

  get total(): number {
    return this.basket.reduce(
      (sum, line) => sum + line.item.price * line.quantity,
      0
    );
  }

  add(item: MenuItem): void {
    const line = this.basket.find(basketLine => basketLine.item.id === item.id);

    if (line) {
      line.quantity++;
    } else {
      this.basket.push({ item, quantity: 1 });
    }

    this.message = '';
    this.showBasket = true;
  }

  change(itemId: number, amount: number): void {
    const line = this.basket.find(basketLine => basketLine.item.id === itemId);

    if (!line) {
      return;
    }

    line.quantity += amount;

    if (line.quantity <= 0) {
      this.basket = this.basket.filter(
        basketLine => basketLine.item.id !== itemId
      );
    }
  }

  placeOrder(): void {
    if (
      this.tableStatus !== 'valid' ||
      this.tableNumber === null ||
      this.basket.length === 0 ||
      this.submitting
    ) {
      return;
    }

    this.submitting = true;
    this.message = '';

    this.orderService
      .createOrder({
        tableNumber: this.tableNumber,
        paymentMethod: this.paymentMethod,
        items: this.basket.map(line => ({
          menuItemId: line.item.id,
          quantity: line.quantity
        }))
      })
      .subscribe({
        next: order => {
          this.message = `Order ${order.id} created. ${order.message}`;
          this.basket = [];
          this.submitting = false;
        },
        error: (error: HttpErrorResponse) => {
          this.message =
            error.error?.message ||
            'Could not place the order. Check the API and try again.';
          this.submitting = false;
        }
      });
  }

  private loadMenu(): void {
    this.menuService.getMenu().subscribe({
      next: items => {
        this.menu = items.map(item => ({
          ...item,
          emoji: item.emoji || this.emojiForCategory(item.category)
        }));
        this.loadingMenu = false;
      },
      error: () => {
        this.menuError =
          'Could not load the menu. Check that the API and SQL Server are running.';
        this.loadingMenu = false;
      }
    });
  }


  private checkTable(tableNumber: number): void {
    this.tableStatus = 'checking';
    this.tableService.getTable(tableNumber).subscribe({
      next: table => {
           if (this.tableNumber !== tableNumber) {
        return;
      }
        this.tableStatus = table.active === true ? 'valid' : 'invalid';
      },
      error: () => {
           if (this.tableNumber !== tableNumber) {
        return;
      }
        this.tableStatus = 'invalid';
      }
    });
  }

  private emojiForCategory(category: string): string {
    switch (category.toLowerCase()) {
      case 'pizza':
        return '🍕';
      case 'sides':
        return '🧄';
      case 'drinks':
        return '🥤';
      case 'desserts':
        return '🍰';
      default:
        return '🍽️';
    }
  }
}
