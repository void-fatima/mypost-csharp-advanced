# MyPost design system

Status: source of truth for `apps/MyPost.Web`  
Version: 1.0 — 2026-08-24

## Provenance

The requested `$ui-ux-pro-max` skill was not available in the execution environment. Work originally stopped as requested; the user then explicitly directed implementation to continue. This system is therefore derived from the supplied MyPost product brief and verified implementation constraints, not represented as output from that unavailable skill.

## Product character

MyPost is a virtual postal operations product: precise, reassuring, efficient, and visibly honest about simulated data. The visual language is Swiss-inspired rather than template-like: a firm grid, left-aligned type, restrained color, clear rules, and route-line motifs that explain movement. It must never imply a real external postal service, real-time vehicle position, payment settlement, or a live carrier integration.

## Product patterns

- Public experience: compact marketing header, tracking as the primary above-fold task, product proof through real application capabilities, and a clear virtual-demo disclosure.
- Authenticated experience: responsive role-aware shell, medium-density operational content, URL-owned filters, and one primary action per view.
- Shipment detail: identity band → current-state summary → next legal actions → chronological status timeline → address and package facts.
- Tables: desktop table from 768px; stacked labelled records below 768px. Search and status filters remain visible and results are paginated.
- Forms: one question group at a time for shipment creation. The step indicator includes text and number; validation is inline and summarized through an `aria-live` surface.

## Brand

The mark combines an open parcel corner with a route node. It is code-native SVG, uses `currentColor`, remains recognizable at 24px, and is always accompanied by the MyPost wordmark in navigation. Never stretch, rotate, outline with a second color, or place it on insufficient contrast.

Copy rules:

- Use direct operational language: “Create shipment”, “Awaiting pickup”, “Assign courier”.
- Prefer concrete reassurance over generic SaaS claims.
- Explicitly label demo data and virtual workflows.
- Never use “live”, “real-time”, “guaranteed”, or “official” for simulated behavior.

## Color tokens

All component colors reference semantic tokens; raw values do not appear in components.

| Token | Light | Dark | Use |
|---|---:|---:|---|
| `canvas` | `#F6F8FC` | `#0B1220` | application background |
| `surface` | `#FFFFFF` | `#111B2E` | cards, navigation |
| `surface-subtle` | `#EEF3FA` | `#17243A` | grouped regions |
| `border` | `#D9E2EF` | `#2A3A55` | visible boundaries |
| `text` | `#122033` | `#F4F7FB` | primary text |
| `text-muted` | `#52647A` | `#AEBBD0` | secondary text |
| `primary` | `#2563EB` | `#60A5FA` | actions, route progress |
| `primary-strong` | `#1749B8` | `#93C5FD` | hover/high contrast |
| `accent` | `#EA580C` | `#FB923C` | parcel/action accent |
| `success` | `#15803D` | `#4ADE80` | delivered/success |
| `warning` | `#A16207` | `#FACC15` | awaiting/attention |
| `danger` | `#B91C1C` | `#F87171` | failed/destructive |
| `info` | `#0369A1` | `#38BDF8` | informational states |

Status badges pair color with an icon and readable status text. Returned is not styled as failure; it uses warning/neutral treatment because it is a completed operational outcome.

## Typography

Font stack: Inter Variable when available, then `Inter`, `ui-sans-serif`, system UI. Numbers use tabular variants in tracking codes, money, and metrics.

- Display: 48/52, 700 desktop; 36/40 mobile.
- Page title: 30/36, 700.
- Section title: 20/28, 650.
- Body: 15/24, 400.
- Label: 13/18, 600.
- Caption: 12/18, 500.

Line length is capped near 68 characters for explanatory text. Tracking codes use 14/20, 650 with `0.04em` tracking.

## Layout and spacing

Base unit: 4px. Allowed spacing: 4, 8, 12, 16, 20, 24, 32, 40, 48, 64, 80. Application content max-width is 1440px; public reading content is 1200px. Desktop sidebar is 264px; collapsed navigation is not used because labels aid comprehension.

Breakpoints:

- `sm` 640px, `md` 768px, `lg` 1024px, `xl` 1280px, `2xl` 1440px.
- 375px: single column, 16px gutters, bottom mobile navigation, stacked records.
- 768px: 24px gutters, tables become available, multi-column forms only for tightly related fields.
- 1024px: persistent sidebar, dashboard grids.
- 1440px: maximum operational density without stretching reading widths.

Radii: 6px controls, 10px cards/dialogs, full radius only for badges and avatar shapes. Shadows are limited to overlays and elevated menus; cards use borders.

## Components

- Buttons: 44px minimum height, visible icon plus label except theme/menu affordances with accessible names. Primary blue, secondary bordered, danger reserved for destructive confirmation.
- Inputs: 44px height, persistent label, optional hint, error below. Focus ring is 3px with adequate offset.
- Cards: border and surface; no nested card stacks unless the inner surface is interactive.
- Status badge: icon + text; no color-only meaning.
- Timeline: ordered list with route rail, current event emphasized, timestamps rendered semantically with `<time>`.
- Dialog/menu: focus trapped, Escape closes, triggering control regains focus.
- Toast/status: success and errors announced politely; server errors keep entered data.
- Chart: only status distribution and throughput trends. Always accompanied by labelled values or a table.

## State matrix

Every data surface implements:

| State | Required response |
|---|---|
| Loading | geometry-matched skeleton, `aria-busy=true` |
| Empty | specific explanation and relevant next action |
| Error | concise message, retry action, no raw exception |
| Disabled | reduced emphasis plus native `disabled`; reason remains visible |
| Submitting | action label changes, duplicate submission blocked |
| Success | persistent result or polite status message; focus moves to result when appropriate |

Destructive actions require confirmation. Optimistic updates are limited to reversible preference changes; shipment lifecycle mutations wait for server confirmation.

## Motion

Transitions last 150–220ms with standard ease-out. Use opacity and transform for menus, focus surfaces, and status feedback. Do not animate table layout or shipment positions. Under `prefers-reduced-motion: reduce`, remove nonessential transitions and smooth scrolling.

## Accessibility

- WCAG 2.2 AA contrast target; primary text ≥ 4.5:1 and large text ≥ 3:1.
- Semantic landmarks, a skip link, one `<h1>`, sequential headings, and meaningful document titles.
- 44×44px touch targets, visible keyboard focus, logical DOM/tab order.
- Errors use `aria-invalid` and `aria-describedby`; async states use `aria-live` without stealing focus.
- Mobile navigation labels remain visible. Icon-only controls require accessible names and tooltips where meaning is not obvious.
- Data tables include captions and scoped headers; mobile alternatives preserve labels.
- Charts expose the same information as text.

## React implementation rules

- Feature routes are lazy-loaded. TanStack Query owns remote state; URL search parameters own filter and pagination state.
- React Hook Form and Zod own write-form state and client validation; backend Problem Details remains authoritative.
- Access tokens live only in memory. Refresh is performed with the HttpOnly cookie and `credentials: include`; no token or private profile is written to local storage.
- Shared primitives own visual tokens and interaction behavior. Pages compose features and never duplicate raw API requests.
- Tailwind utilities use semantic CSS variables declared once in `src/styles.css`. Dark mode is a root class and respects the system preference on first load.

## Pre-delivery checklist

- [ ] All required routes render real API state.
- [ ] Loading, empty, error, disabled, submitting, and success states verified.
- [ ] Keyboard navigation and focus return verified.
- [ ] Contrast and non-color status meaning reviewed.
- [ ] 375, 768, 1024, and 1440px layouts verified.
- [ ] Reduced motion verified.
- [ ] Filters survive refresh through URL state.
- [ ] Public tracking contains no private sender, courier, phone, street, or note data.
- [ ] No fake map, payment, live vehicle, or external-post claims.
- [ ] Production build, tests, and critical browser flows pass.
