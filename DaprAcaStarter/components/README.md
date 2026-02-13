# Local Dapr Components (`DaprAcaStarter/components`)

These manifests are used for local `dapr run` sessions.

## Files

- `statestore.yaml`: Redis state store (`statestore`)
- `pubsub.yaml`: Redis pub/sub broker (`pubsub`)

## Local Requirements

Redis must be running at `localhost:6379`.

```powershell
docker run --name dapr-redis -d -p 6379:6379 redis:7
```

## Usage

Start any service with:

```powershell
dapr run --resources-path ./components ...
```

For microservices outside `DaprAcaStarter`, point to this folder with a relative path:

```powershell
dapr run --resources-path ../../DaprAcaStarter/components ...
```
