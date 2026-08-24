# Architecture decisions

## ADR-001 — Modular monolith

**Decision:** Keep one deployable backend while separating Domain, Application, Infrastructure, and delivery adapters by project references.

**Why:** The product needs strong business boundaries and testability without the operational cost or distributed consistency problems of premature services.

## ADR-002 — PostgreSQL as the source of truth

**Decision:** Replace JSON persistence with PostgreSQL through EF Core and Npgsql.

**Why:** Shipment uniqueness, concurrent writes, identity, filtering, pagination, and auditable lifecycle history require transactional persistence and database constraints.

## ADR-003 — Lifecycle rules inside the aggregate

**Decision:** Status changes, assignments, deliveries, cancellations, and returns mutate only through `Shipment` methods.

**Why:** Every adapter receives the same legal transition rules, terminal-state protection, idempotency behavior, and history recording.

## ADR-004 — Address snapshots on shipments

**Decision:** A shipment stores sender and destination snapshots rather than live address-book references.

**Why:** Editing or deleting a saved address must not alter an already-created shipment or its audit trail.

## ADR-005 — Short access token plus rotating refresh cookie

**Decision:** Return short-lived JWT access tokens to browser memory and rotate HttpOnly refresh cookies backed by server-side hashes.

**Why:** This keeps bearer tokens out of persistent browser storage, permits session revocation, and limits replay exposure while retaining a practical SPA session.

## ADR-006 — Privacy-safe public projection

**Decision:** Public tracking uses a dedicated DTO and endpoint rather than serializing the authenticated shipment view.

**Why:** Public lookup must never reveal phone numbers, streets, sender/courier identity, delivery notes, or internal assignments.

## ADR-007 — One application layer for API and CLI

**Decision:** Keep the CLI as a thin adapter over `ShipmentService`.

**Why:** Maintaining separate rules or JSON persistence would recreate the inconsistent behavior the migration was designed to remove.

