# Legacy console migration

The original JSON console was retained until its useful behavior was represented by the modular model and protected by tests. It was then removed to avoid maintaining a second source of domain truth.

| Legacy behavior | New owner | Protection |
|---|---|---|
| Person/home/letter validation | Domain value objects and Application request validation | Domain and Application tests |
| Duplicate-safe adds | Database unique constraints and customer-reference idempotency | Application idempotency test and migration indexes |
| Repeated processing without duplicate queue entries | Idempotent shipment transitions and courier assignment | Domain repeat-operation tests |
| Delivery routing | Explicit shipment lifecycle and courier delivery result | Domain transition tests |
| Return to sender | `ReturnInitiated → ReturningToSender → ReturnedToSender` | Domain return-flow test |
| JSON state | PostgreSQL aggregate, history, assignments, and address snapshots | EF migration and API integration tests |

`apps/MyPost.Cli` remains as the optional console adapter and calls `ShipmentService`; it contains no duplicated shipment rules or persistence logic.
