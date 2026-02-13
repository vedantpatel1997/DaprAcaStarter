import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DaprApiService } from './dapr-api.service';
import { Cart, CheckoutOrder, HealthResponse, Product, ServiceInfo } from './models';

@Component({
  selector: 'app-root',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly api = inject(DaprApiService);
  private readonly fb = inject(FormBuilder);

  readonly apiBaseUrl = this.fb.nonNullable.control(this.api.apiBaseUrl, [Validators.required]);

  readonly customerId = this.fb.nonNullable.control('cust-101', [Validators.required]);
  readonly addToCartForm = this.fb.nonNullable.group({
    productId: ['P-100', Validators.required],
    quantity: [1, [Validators.required, Validators.min(1), Validators.max(25)]]
  });

  readonly loading = signal(false);
  readonly serviceInfo = signal<ServiceInfo | null>(null);
  readonly healthInfo = signal<HealthResponse | null>(null);
  readonly products = signal<Product[]>([]);
  readonly cart = signal<Cart | null>(null);
  readonly latestOrder = signal<CheckoutOrder | null>(null);
  readonly message = signal<string>('Ready');
  readonly activity = signal<string[]>([]);

  onApplyApiBaseUrl(): void {
    if (this.apiBaseUrl.invalid) {
      this.message.set('Enter a valid API base URL first.');
      return;
    }

    this.api.setApiBaseUrl(this.apiBaseUrl.getRawValue());
    this.log(`API base URL changed to ${this.api.apiBaseUrl}`);
  }

  onLoadOverview(): void {
    this.loading.set(true);

    this.api.getServiceInfo().subscribe({
      next: (info) => {
        this.serviceInfo.set(info);
        this.log(`Storefront appId=${info.appId} routes to ${info.services.productsAppId}, ${info.services.cartAppId}, ${info.services.checkoutAppId}`);
      },
      error: (error) => this.handleError('Failed to load service info.', error)
    });

    this.api.getHealth().subscribe({
      next: (health) => {
        this.healthInfo.set(health);
        this.message.set('Storefront metadata and health loaded.');
      },
      error: (error) => this.handleError('Health check failed.', error),
      complete: () => this.loading.set(false)
    });
  }

  onLoadProducts(): void {
    this.loading.set(true);
    this.api.getProducts().subscribe({
      next: (products) => {
        this.products.set(products);
        if (products.length > 0 && !products.some((p) => p.id === this.addToCartForm.getRawValue().productId)) {
          this.addToCartForm.patchValue({ productId: products[0].id });
        }

        this.message.set(`Loaded ${products.length} products.`);
        this.log('Storefront invoked products-service via Dapr service invocation.');
      },
      error: (error) => this.handleError('Failed to load products.', error),
      complete: () => this.loading.set(false)
    });
  }

  onLoadCart(): void {
    if (this.customerId.invalid) {
      this.message.set('Customer ID is required.');
      return;
    }

    const customerId = this.customerId.getRawValue();
    this.loading.set(true);

    this.api.getCart(customerId).subscribe({
      next: (cart) => {
        this.cart.set(cart);
        this.message.set(`Cart loaded for ${customerId}.`);
        this.log('Storefront invoked cart-service and read cart state from statestore.');
      },
      error: (error) => this.handleError('Failed to load cart.', error),
      complete: () => this.loading.set(false)
    });
  }

  onAddToCart(): void {
    if (this.customerId.invalid || this.addToCartForm.invalid) {
      this.message.set('Customer and add-to-cart form must be valid.');
      return;
    }

    const customerId = this.customerId.getRawValue();
    const form = this.addToCartForm.getRawValue();
    const product = this.products().find((p) => p.id === form.productId);

    if (!product) {
      this.message.set('Choose a valid product first.');
      return;
    }

    this.loading.set(true);
    this.api
      .addCartItem(customerId, {
        productId: product.id,
        productName: product.name,
        unitPrice: product.price,
        quantity: form.quantity
      })
      .subscribe({
        next: (cart) => {
          this.cart.set(cart);
          this.message.set(`Added ${form.quantity} x ${product.name} to cart.`);
          this.log('cart-service updated cart in statestore through Dapr state API.');
        },
        error: (error) => this.handleError('Failed to add product to cart.', error),
        complete: () => this.loading.set(false)
      });
  }

  onCheckout(): void {
    if (this.customerId.invalid) {
      this.message.set('Customer ID is required.');
      return;
    }

    const customerId = this.customerId.getRawValue();
    this.loading.set(true);

    this.api.checkout(customerId).subscribe({
      next: (order) => {
        this.latestOrder.set(order);
        this.cart.set({ customerId, items: [], total: 0 });
        this.message.set(`Checkout complete: ${order.orderId}`);
        this.log('checkout-service saved order, published checkout.completed.v1, and cart-service subscription cleared cart.');
      },
      error: (error) => this.handleError('Checkout failed.', error),
      complete: () => this.loading.set(false)
    });
  }

  onLoadOrder(): void {
    const orderId = this.latestOrder()?.orderId;
    if (!orderId) {
      this.message.set('Run checkout first to query an order.');
      return;
    }

    this.loading.set(true);
    this.api.getOrder(orderId).subscribe({
      next: (order) => {
        this.latestOrder.set(order);
        this.message.set(`Order ${order.orderId} loaded.`);
        this.log('Storefront invoked checkout-service to read order state.');
      },
      error: (error) => this.handleError('Failed to load order.', error),
      complete: () => this.loading.set(false)
    });
  }

  private handleError(prefix: string, error: unknown): void {
    const details = this.formatError(error);
    this.message.set(`${prefix} ${details}`);
    this.log(`${prefix} ${details}`);
    this.loading.set(false);
  }

  private formatError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string') {
        return error.error;
      }

      if (error.error?.message) {
        return error.error.message;
      }

      return `HTTP ${error.status} ${error.statusText}`;
    }

    return 'Unexpected error';
  }

  private log(entry: string): void {
    this.activity.update((items) => [new Date().toISOString() + ' - ' + entry, ...items].slice(0, 12));
  }
}
