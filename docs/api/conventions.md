# API conventions

All endpoints are rooted at `/api/v1`. JSON enums use readable names. Collection responses use `items`, `page`, `pageSize`, `totalCount`, and `totalPages`. Filters are query parameters and clients should keep them in URL state.

Write failures use Problem Details with a trace identifier. Validation failures include an `errors` dictionary. Authentication uses a short-lived bearer access token; the rotating refresh token is stored only as a SHA-256 hash server-side and sent to the browser in an HttpOnly, same-site cookie.

Authentication and public tracking are rate-limited. Customer and courier endpoints enforce resource ownership in the application layer in addition to role policies. Public tracking intentionally excludes phone numbers, full street addresses, sender identity, courier identity, delivery notes, and internal assignment data.
