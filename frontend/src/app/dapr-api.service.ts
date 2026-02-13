import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateOrderRequest,
  HealthResponse,
  InvocationRequest,
  Order,
  PublishOrderEventRequest,
  ServiceInfo
} from './models';

@Injectable({ providedIn: 'root' })
export class DaprApiService {
  private readonly http = inject(HttpClient);
  private readonly defaultBaseUrl =
    'https://ca-dapr-aca-backend.blackocean-54455b91.westus2.azurecontainerapps.io';
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

  createOrder(payload: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.url('/orders'), payload);
  }

  getOrder(orderId: string): Observable<Order> {
    return this.http.get<Order>(this.url(`/orders/${encodeURIComponent(orderId)}`));
  }

  publishOrderEvent(payload: PublishOrderEventRequest): Observable<void> {
    return this.http.post<void>(this.url('/publish/orders'), payload);
  }

  invokeSelf(payload: InvocationRequest): Observable<object> {
    return this.http.post<object>(this.url('/invoke/self'), payload);
  }

  private url(path: string): string {
    return `${this.baseUrl()}${path}`;
  }
}
