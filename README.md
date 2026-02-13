# Dapr ACA Microservices Storefront

This repository contains a complete Dapr-based microservices sample deployed on Azure Container Apps (ACA).

## Architecture

- `frontend` (Angular): user interface.
- `DaprAcaStarter` (`storefront-api`): backend-for-frontend (BFF) entry point.
- `microservices/ProductsService` (`products-service`): product catalog.
- `microservices/CartService` (`cart-service`): cart state management + checkout event subscriber.
- `microservices/CheckoutService` (`checkout-service`): checkout orchestration + order persistence + pub/sub publisher.
- Redis: shared infrastructure for Dapr `statestore` and `pubsub` components.

## Request and Event Flow

1. UI calls `storefront-api`.
2. `storefront-api` uses Dapr service invocation by app-id:
   - `products-service`
   - `cart-service`
   - `checkout-service`
3. `cart-service` stores cart in `statestore` (Redis).
4. `checkout-service` reads cart, creates order, stores order in `statestore`, publishes `checkout.completed.v1` to `pubsub`.
5. `cart-service` receives topic event and clears the customer cart.

## Repository Structure

- `DaprAcaStarter`: storefront API project.
- `microservices`: backend microservices.
- `frontend`: Angular application.
- `DaprAcaStarter/components`: local Dapr component manifests.
- `DaprAcaStarter/aca`: ACA deployment manifests/scripts.

## Local Development (Full System)

Prerequisites:

- .NET SDK 10
- Node.js 22+
- Docker Desktop
- Dapr CLI initialized (`dapr init`)

Start Redis:

```powershell
docker run --name dapr-redis -d -p 6379:6379 redis:7
# or if it already exists:
docker start dapr-redis
```

Run services in separate terminals:

```powershell
# terminal 1
cd DaprAcaStarter
dapr run --app-id storefront-api --app-port 8080 --dapr-http-port 3500 --resources-path ./components -- dotnet run --urls http://localhost:8080

# terminal 2
cd microservices/ProductsService
dapr run --app-id products-service --app-port 8081 --dapr-http-port 3501 --resources-path ../../DaprAcaStarter/components -- dotnet run --urls http://localhost:8081

# terminal 3
cd microservices/CartService
dapr run --app-id cart-service --app-port 8082 --dapr-http-port 3502 --resources-path ../../DaprAcaStarter/components -- dotnet run --urls http://localhost:8082

# terminal 4
cd microservices/CheckoutService
dapr run --app-id checkout-service --app-port 8083 --dapr-http-port 3503 --resources-path ../../DaprAcaStarter/components -- dotnet run --urls http://localhost:8083

# terminal 5
cd frontend
npm install
npm start
```

Open `http://localhost:4200` and keep API URL as `http://localhost:8080`.

## Build and Test

```powershell
dotnet build "Dapr ACA.sln"
cd frontend
npm run build
```

## Azure Container Apps Deployment (Production-style)

Reference environment:

- Subscription: `6a3bb170-5159-4bff-860b-aa74fb762697`
- Resource Group: `rg-vkp-dev-container-apps`
- Managed Environment: `cae-vkp-dev`
- Shared ACR: `acrvkpshared.azurecr.io` in `rg-vkp-shared-resources`

### 1) Login and set context

```powershell
az login
az account set --subscription 6a3bb170-5159-4bff-860b-aa74fb762697
az acr login -n acrvkpshared -g rg-vkp-shared-resources
```

### 2) Build and push images

```powershell
# storefront API
cd DaprAcaStarter
docker build -t acrvkpshared.azurecr.io/storefront-api:latest .
docker push acrvkpshared.azurecr.io/storefront-api:latest

# products
cd ..\microservices\ProductsService
docker build -t acrvkpshared.azurecr.io/products-service:latest .
docker push acrvkpshared.azurecr.io/products-service:latest

# cart
cd ..\CartService
docker build -t acrvkpshared.azurecr.io/cart-service:latest .
docker push acrvkpshared.azurecr.io/cart-service:latest

# checkout
cd ..\CheckoutService
docker build -t acrvkpshared.azurecr.io/checkout-service:latest .
docker push acrvkpshared.azurecr.io/checkout-service:latest

# frontend
cd ..\..\frontend
docker build -t acrvkpshared.azurecr.io/storefront-frontend:latest .
docker push acrvkpshared.azurecr.io/storefront-frontend:latest
```

Import Redis image once:

```powershell
az acr import -n acrvkpshared -g rg-vkp-shared-resources --source docker.io/library/redis:7 --image redis:7 --force
```

### 3) Remove old apps from CAE (across all resource groups)

```powershell
$envId = '/subscriptions/6a3bb170-5159-4bff-860b-aa74fb762697/resourceGroups/rg-vkp-dev-container-apps/providers/Microsoft.App/managedEnvironments/cae-vkp-dev'
$apps = az resource list --resource-type Microsoft.App/containerApps --query "[?properties.managedEnvironmentId=='$envId'].{name:name,rg:resourceGroup}" -o json | ConvertFrom-Json
foreach ($a in $apps) {
  az containerapp delete -g $a.rg -n $a.name -y
}
```

### 4) Deploy Redis + Dapr components + apps

Use manifests in `DaprAcaStarter/aca`.

```powershell
cd DaprAcaStarter/aca
# optional helper script (expects required params)
# .\deploy-microservices.ps1 -ResourceGroup rg-vkp-dev-container-apps -ContainerAppsEnvironment cae-vkp-dev -RemoveLegacyApp
```

Or run commands manually with `az containerapp create`/`az containerapp update` as documented inside `DaprAcaStarter/aca/README.md`.

### 5) Verify cloud endpoints

```powershell
Invoke-RestMethod https://storefront-api.blackocean-54455b91.westus2.azurecontainerapps.io/healthz
Invoke-WebRequest https://storefront-frontend.blackocean-54455b91.westus2.azurecontainerapps.io -UseBasicParsing
```

## Logs and Operational Validation (Log Analytics)

Workspace ID (example from current environment): `2fbe93f5-f503-4b8f-8771-060a1a60de8f`

```powershell
az extension add --name log-analytics --upgrade --yes

$ws='2fbe93f5-f503-4b8f-8771-060a1a60de8f'
az monitor log-analytics query --workspace $ws --analytics-query "ContainerAppConsoleLogs_CL | where TimeGenerated > ago(30m) | where ContainerAppName_s in ('storefront-api','products-service','cart-service','checkout-service') | project TimeGenerated, ContainerAppName_s, Log_s | top 100 by TimeGenerated desc" -o table
```

## Git Workflow

```powershell
git add -A
git status
git commit -m "Refactor to Dapr storefront microservices with full docs"
git push origin main
```
