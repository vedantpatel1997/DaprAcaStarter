export interface ServiceInfo {
  message: string;
  appId: string;
  dapr: {
    stateStore: string;
    pubsub: string;
    topic: string;
  };
}

export interface HealthResponse {
  status: string;
  utc: string;
}

export interface CreateOrderRequest {
  customerId: string;
  product: string;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  id: string;
  customerId: string;
  product: string;
  quantity: number;
  unitPrice: number;
  createdUtc: string;
  total: number;
}

export interface PublishOrderEventRequest {
  orderId: string;
  status: string;
}

export interface InvocationRequest {
  message: string;
}
