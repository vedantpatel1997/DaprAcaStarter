import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AddCartItemRequest,
  Cart,
  CheckoutOrder,
  HealthResponse,
  Product,
  ServiceInfo
} from './models';

@Injectable({ providedIn: 'root' })
export class DaprApiService {
  private readonly http = inject(HttpClient);
  private readonly defaultBaseUrl =
    'https://storefront-api.blackocean-54455b91.westus2.azurecontainerapps.io';
  private readonly baseUrl = signal(this.defaultBaseUrl);

  get apiBaseUrl(): string {
    return this.baseUrl();
  }

  setApiBaseUrl(url: string): void {
    const normalized = url.trim().replace(/\/+$/, '');
    this.baseUrl.set(normalized || this.defaultBaseUrl);
  }

  getServiceInfo(): Observable<ServiceInfo> {
    return this.http.get<ServiceInfo>(this.url('/'));
  }

  getHealth(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>(this.url('/healthz'));
  }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.url('/api/products'));
  }

  getCart(customerId: string): Observable<Cart> {
    return this.http.get<Cart>(this.url(`/api/cart/${encodeURIComponent(customerId)}`));
  }

  addCartItem(customerId: string, payload: AddCartItemRequest): Observable<Cart> {
    return this.http.post<Cart>(this.url(`/api/cart/${encodeURIComponent(customerId)}/items`), payload);
  }

  checkout(customerId: string): Observable<CheckoutOrder> {
    return this.http.post<CheckoutOrder>(this.url(`/api/checkout/${encodeURIComponent(customerId)}`), {});
  }

  getOrder(orderId: string): Observable<CheckoutOrder> {
    return this.http.get<CheckoutOrder>(this.url(`/api/orders/${encodeURIComponent(orderId)}`));
  }

  private url(path: string): string {
    return `${this.baseUrl()}${path}`;
  }
}
