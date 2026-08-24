# MyPost architecture

MyPost is a modular monolith. The Domain project owns business invariants, Application owns use cases and ports, Infrastructure owns PostgreSQL and Identity adapters, and the API, web app, and CLI are delivery adapters.

Dependencies point inward: adapters depend on Application and Domain; Domain depends on no framework. PostgreSQL is the primary web persistence store. The legacy JSON console remains temporarily as a regression reference until equivalent CLI behavior is protected by the new tests.
