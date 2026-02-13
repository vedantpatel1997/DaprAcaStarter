import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DaprApiService } from './dapr-api.service';
import { HealthResponse, Order, ServiceInfo } from './models';

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

  readonly createOrderForm = this.fb.nonNullable.group({
    customerId: ['cust-101', Validators.required],
    product: ['Laptop Stand', Validators.required],
    quantity: [2, [Validators.required, Validators.min(1)]],
    unitPrice: [39.99, [Validators.required, Validators.min(0.01)]]
  });

  readonly getOrderForm = this.fb.nonNullable.group({
    orderId: ['', Validators.required]
  });

  readonly publishOrderForm = this.fb.nonNullable.group({
    orderId: ['', Validators.required],
    status: ['Shipped', Validators.required]
  });

  readonly invokeForm = this.fb.nonNullable.group({
    message: ['hello via dapr invocation', Validators.required]
  });

  readonly loading = signal(false);
  readonly serviceInfo = signal<ServiceInfo | null>(null);
  readonly healthInfo = signal<HealthResponse | null>(null);
  readonly selectedOrder = signal<Order | null>(null);
  readonly invokeResult = signal<unknown | null>(null);
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

  onLoadServiceInfo(): void {
    this.loading.set(true);
    this.api.getServiceInfo().subscribe({
      next: (result) => {
        this.serviceInfo.set(result);
        this.message.set('Service info loaded.');
        this.log(`Loaded root metadata. appId=${result.appId}`);
      },
      error: (error) => this.handleError('Failed to load service info.', error),
      complete: () => this.loading.set(false)
    });
  }

  onHealthCheck(): void {
    this.loading.set(true);
    this.api.getHealth().subscribe({
      next: (result) => {
        this.healthInfo.set(result);
        this.message.set('Health check successful.');
        this.log(`Health is ${result.status} at ${result.utc}`);
      },
      error: (error) => this.handleError('Health check failed.', error),
      complete: () => this.loading.set(false)
    });
  }

  onCreateOrder(): void {
    if (this.createOrderForm.invalid) {
      this.message.set('Create Order form is invalid.');
      return;
    }

    this.loading.set(true);
    this.api.createOrder(this.createOrderForm.getRawValue()).subscribe({
      next: (order) => {
        this.selectedOrder.set(order);
        this.getOrderForm.patchValue({ orderId: order.id });
        this.publishOrderForm.patchValue({ orderId: order.id });
        this.message.set(`Order created: ${order.id}`);
        this.log(`Created order ${order.id}, saved to state store and published to topic.`);
      },
      error: (error) => this.handleError('Failed to create order.', error),
      complete: () => this.loading.set(false)
    });
  }

  onGetOrder(): void {
    if (this.getOrderForm.invalid) {
      this.message.set('Enter a valid order ID.');
      return;
    }

    this.loading.set(true);
    const orderId = this.getOrderForm.getRawValue().orderId;
    this.api.getOrder(orderId).subscribe({
      next: (order) => {
        this.selectedOrder.set(order);
        this.message.set(`Order loaded: ${order.id}`);
        this.log(`Read order ${order.id} from statestore.`);
      },
      error: (error) => this.handleError(`Failed to load order ${orderId}.`, error),
      complete: () => this.loading.set(false)
    });
  }

  onPublishEvent(): void {
    if (this.publishOrderForm.invalid) {
      this.message.set('Publish form is invalid.');
      return;
    }

    this.loading.set(true);
    const payload = this.publishOrderForm.getRawValue();
    this.api.publishOrderEvent(payload).subscribe({
      next: () => {
        this.message.set(`Published event for order ${payload.orderId}.`);
        this.log(`Published manual order event. orderId=${payload.orderId}, status=${payload.status}`);
      },
      error: (error) => this.handleError('Failed to publish order event.', error),
      complete: () => this.loading.set(false)
    });
  }

  onInvokeSelf(): void {
    if (this.invokeForm.invalid) {
      this.message.set('Invocation message is required.');
      return;
    }

    this.loading.set(true);
    const payload = this.invokeForm.getRawValue();
    this.api.invokeSelf(payload).subscribe({
      next: (response) => {
        this.invokeResult.set(response);
        this.message.set('Dapr service invocation succeeded.');
        this.log(`Invoked internal endpoint through Dapr sidecar. message="${payload.message}"`);
      },
      error: (error) => this.handleError('Service invocation failed.', error),
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
