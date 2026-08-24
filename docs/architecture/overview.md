# MyPost architecture

MyPost is a modular monolith. Domain owns business invariants, Application owns authorized use cases and ports, Infrastructure owns PostgreSQL and Identity adapters, and the API, web app, and CLI are delivery adapters.

Dependencies point inward: adapters depend on Application and Domain; Domain depends on no framework. PostgreSQL is the single persistence source of truth. The original JSON implementation was removed after its relevant behavior was mapped to the new model and protected by domain, application, and endpoint tests.

## Runtime boundaries

- The React client communicates only through the versioned HTTP API.
- The API authenticates the caller, applies transport concerns, and delegates rules to Application and Domain.
- Application services enforce role/resource ownership and use repository interfaces rather than EF Core types.
- Infrastructure supplies EF Core repositories, Identity users and roles, JWT creation, refresh-token rotation, and development data.
- The CLI resolves the same Application service graph and is not a second implementation of shipment behavior.

## Data ownership

`Shipment` is the consistency boundary for status, assignment, delivery, and return transitions. Tracking events and courier assignments are children of that aggregate. Sender and destination addresses are copied into immutable snapshots at creation so later address-book edits cannot rewrite historical shipment facts.

The database owns uniqueness for tracking codes and customer references. EF Core uses the aggregate version as an optimistic-concurrency token, while application operations are idempotent where retries are expected.

## Authentication boundary

ASP.NET Core Identity owns credentials and roles. A successful login returns a short-lived access token and sets a rotating refresh token as an HttpOnly same-site cookie. Only the refresh-token hash is stored. The browser keeps access tokens in memory and retries an authorized request once after a successful refresh.

## Operational decisions

The API publishes `/health`, OpenAPI in development, structured Problem Details, configured CORS, and separate rate-limit policies for authentication and public tracking. Development migration and seed behavior is opt-in through configuration; production defaults do not auto-migrate or seed.

See [decisions.md](decisions.md) for the concise architecture decision record and [legacy-migration.md](legacy-migration.md) for the behavior mapping from the original console.
