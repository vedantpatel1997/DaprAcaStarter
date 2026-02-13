export interface ServiceInfo {
  message: string;
  appId: string;
  dapr: {
    stateStore: string;
    pubsub: string;
    topic: string;
  };
  services: {
    productsAppId: string;
    cartAppId: string;
    checkoutAppId: string;
  };
  workflow: string[];
}

export interface HealthResponse {
  status: string;
  utc: string;
}

export interface Product {
  id: string;
  name: string;
  price: number;
  currency: string;
  description: string;
}

export interface CartItem {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Cart {
  customerId: string;
  items: CartItem[];
  total: number;
}

export interface AddCartItemRequest {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
}

export interface CheckoutOrder {
  orderId: string;
  customerId: string;
  items: CartItem[];
  total: number;
  checkedOutUtc: string;
  status: string;
}
