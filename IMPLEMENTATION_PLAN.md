# Cafe Automate — Implementation Plan v2

## Project Summary

Full-stack order-ahead cafe system. Single host deployment on Stewart Pakistan server.
Frontend: HTML5 / CSS3 / Vanilla JS (evolving from existing `cafe-automate (1).html`).
Backend: ASP.NET Core Web API (.NET 8) + EF Core + PostgreSQL (SQLite fallback).
Auth: JWT Bearer, 2 hardcoded admin seeds, user self-registration.
Realtime: SignalR for live order/payment status across all dashboards.

---

## Admin Credentials (seeded at startup)

| Role | Username | Password |
|------|----------|----------|
| Website Admin | admin1SE@stewart.com | admin@1234# |
| Cafe Admin | cafe.admin@stewart.com | admin2@4321# |
| User | self-registration | — |

---

## Existing Assets

- `cafe-automate (1).html` — single-file prototype (cream/terracotta palette, Fraunces + Outfit fonts, no login, client-only cart)
- `hero section.jpg` — night storefront photo (charcoal frames, amber wood glow); referenced in HTML as `hero-cafe.jpg` (rename needed)

**Hero image fix:** rename / copy `hero section.jpg` → `hero-cafe.jpg` at the start of Phase 1.

---

## Design Tokens (inherit from existing HTML)

```
--bg: #f7ede0        (warm cream)
--card: #fffaf2
--sand: #e9c6a3
--clay: #c1592e      (primary terracotta)
--clay-dark: #92401f
--gold: #d9a256
--charcoal: #221a15
--sky: #3f6f92
--sage: #6f8f5c
--cream: #fff2e2
--radius: 20px
Fonts: Fraunces (display) + Outfit (body) via Google Fonts
```

Login page and all new pages/dashboards use these same tokens.

---

## Role Permission Matrix

| Capability | Website Admin | Cafe Admin | User |
|---|---|---|---|
| Manage users (view / enable / disable) | YES | — | — |
| Monitor Cafe Admin activity (view-only) | YES | — | — |
| Add / Edit All Menu items | YES | — | view only |
| Add / Edit Daily Menu items | view only | YES | view only |
| View Contact Us messages | YES | — | submits only |
| Place orders (Daily + All Menu) | — | — | YES |
| Update Order status | view only | YES | view own |
| Update Payment status | view only | YES | view own |
| Set cafe bank / card transfer details | — | YES | view only |

---

## Database Schema

### Users
```
Id, FullName, Email (unique), PasswordHash, Role (1=WebsiteAdmin / 2=CafeAdmin / 3=User),
IsActive, CreatedAt
```

### AllMenuItems
```
Id, Name, Description, Price, ImageUrl, IsAvailable, CreatedAt
```

### DailyMenuItems
```
Id, Name, Price, Quantity, Status (Available / SoldOut), Date, CreatedAt
```

### Orders
```
Id, UserId (FK), OrderStatus (Pending / Received / Delivered),
PaymentStatus (Pending / Received), TotalAmount, CreatedAt, UpdatedAt
```

### OrderItems
```
Id, OrderId (FK), SourceType (DailyMenu / AllMenu), MenuItemId,
ItemNameSnapshot, UnitPriceSnapshot, Quantity
```

### CafePaymentDetails (single row)
```
Id, AccountHolderName, BankName, AccountNumber, IBANOrCardNumber, Instructions, UpdatedAt
```

### ContactMessages
```
Id, Name, Email, Message, SubmittedAt, IsRead
```

---

## Order Status Flow

```
User checks out cart
        |
        v
Order created: OrderStatus=Pending, PaymentStatus=Pending
        |  (SignalR push to Cafe Admin dashboard)
        v
Cafe Admin sets OrderStatus = Received (acknowledges, starts preparing)
        |
User transfers money via shown bank/card details
Cafe Admin confirms receipt → PaymentStatus = Received
        |
Food served
Cafe Admin sets OrderStatus = Delivered
        |
        v
Live updates pushed via SignalR to:
  - User's own dashboard
  - Website Admin oversight view
```

Order Status and Payment Status are **independent** — both badges shown at all times on every order row.

---

## API Endpoints

### Auth
```
POST   /api/auth/signup
POST   /api/auth/login
GET    /api/auth/me
```

### Users (Website Admin only)
```
GET    /api/users
PATCH  /api/users/{id}/status
```

### All Menu
```
GET    /api/all-menu                  public
POST   /api/all-menu                  [WebsiteAdmin]
PUT    /api/all-menu/{id}             [WebsiteAdmin]
DELETE /api/all-menu/{id}             [WebsiteAdmin]
```

### Daily Menu
```
GET    /api/daily-menu                public
POST   /api/daily-menu                [CafeAdmin]
PUT    /api/daily-menu/{id}           [CafeAdmin]
PATCH  /api/daily-menu/{id}/status    [CafeAdmin]  toggle Available/SoldOut
DELETE /api/daily-menu/{id}           [CafeAdmin]
```

### Orders
```
POST   /api/orders                    [User]  checkout cart
GET    /api/orders/mine               [User]
GET    /api/orders                    [CafeAdmin, WebsiteAdmin]
PATCH  /api/orders/{id}/order-status  [CafeAdmin]
PATCH  /api/orders/{id}/payment-status [CafeAdmin]
```

### Cafe Payment Details
```
GET    /api/cafe-payment-details      public (shown at checkout)
PUT    /api/cafe-payment-details      [CafeAdmin]
```

### Contact
```
POST   /api/contact                   public
GET    /api/contact                   [WebsiteAdmin]
PATCH  /api/contact/{id}/read         [WebsiteAdmin]
```

---

## Folder / File Structure

```
cafe/
├── hero-cafe.jpg                         ← rename from "hero section.jpg"
├── cafe-automate (1).html                ← existing prototype (kept as reference)
├── IMPLEMENTATION_PLAN.md                ← this file
│
├── index.html                            ← main public site (rebuilt from prototype)
├── login.html                            ← login + signup page (Phase 2, built first)
├── dashboard-user.html
├── dashboard-cafe-admin.html
├── dashboard-website-admin.html
│
├── css/
│   ├── tokens.css                        ← CSS custom properties, shared across all pages
│   ├── main.css                          ← site styles (header, hero, menu, contact, footer)
│   ├── login.css                         ← login/signup card + floating animations
│   └── dashboard.css                     ← shared dashboard shell styles
│
├── js/
│   ├── api.js                            ← fetch wrapper, JWT attach, SignalR connect
│   ├── auth.js                           ← login/signup logic, token storage
│   ├── cart.js                           ← in-memory cart, badge bounce
│   ├── menu.js                           ← public menu rendering + sold-out animation
│   ├── orders.js                         ← order creation, status display
│   └── dashboard-*.js                    ← per-dashboard logic
│
└── CafeAutomate.Api/                     ← ASP.NET Core Web API project
    ├── CafeAutomate.Api.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Production.json       ← Postgres conn string via env var
    │
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Migrations/
    │
    ├── Models/
    │   ├── User.cs
    │   ├── AllMenuItem.cs
    │   ├── DailyMenuItem.cs
    │   ├── Order.cs
    │   ├── OrderItem.cs
    │   ├── CafePaymentDetails.cs
    │   └── ContactMessage.cs
    │
    ├── DTOs/                             ← request/response shapes
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── UsersController.cs
    │   ├── AllMenuController.cs
    │   ├── DailyMenuController.cs
    │   ├── OrdersController.cs
    │   ├── CafePaymentController.cs
    │   └── ContactController.cs
    │
    ├── Hubs/
    │   └── OrderHub.cs                   ← SignalR hub
    │
    ├── Services/
    │   ├── AuthService.cs
    │   ├── TokenService.cs
    │   └── SeederService.cs              ← seeds 2 admin accounts at startup
    │
    └── Middleware/
        └── RoleGuard.cs
```

---

## Animations Spec

### Login Card (Phase 2 — built first)
- Full-page background: `hero-cafe.jpg` full-bleed with dark overlay matching `--charcoal`
- Card floats: `@keyframes floatCard { 0%,100%{transform:translateY(0)} 50%{transform:translateY(-10px)} }` on a 4s ease-in-out loop
- 3 blurred accent circles drift behind the card in `--clay`, `--gold`, `--sky` at 40–60 % opacity, each on a different slow drift animation (6s / 8s / 10s)
- Tab toggle between Login and Sign Up with a smooth 0.3 s slide/fade
- Submit button: pulse ring on loading, shake (`@keyframes shake`) on error

### Sold-Out Badge
```css
@keyframes stampIn {
  0%   { transform: scale(2) rotate(-15deg); opacity: 0; }
  100% { transform: scale(1) rotate(-8deg);  opacity: 1; }
}
```
- Card grays out to 55 % opacity
- Red stamp-style label rotates in via `stampIn`
- Add-to-Cart button disabled; clicking triggers a brief `shake` animation

### Cart Badge
- `@keyframes cartBounce { 0%,100%{transform:scale(1)} 50%{transform:scale(1.45)} }` fires on every item add (150 ms)

### SignalR Status Badge
- When a status update arrives, the badge does a 300 ms color-fade cross-dissolve from old to new color

---

## Build Phases

### Phase 1 — Foundation
- Create `CafeAutomate.Api/` project with `dotnet new webapi`
- Install packages: `Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.AspNetCore.SignalR`
- `AppDbContext.cs` — all 7 entity sets, auto-fallback logic (`Npgsql` if Postgres DSN env var present, else SQLite)
- Initial EF migration + seed check
- Rename `hero section.jpg` → `hero-cafe.jpg`
- Extract `css/tokens.css` from prototype (CSS custom properties only)

### Phase 2 — Auth + Login Page (BUILT FIRST)
- `AuthController` — signup, login (returns JWT), me
- `SeederService` — seeds Website Admin + Cafe Admin on startup if not present
- `TokenService` — HS256 JWT, configurable secret via env var
- **`login.html` + `css/login.css` + `js/auth.js`** — floating card, background hero, tab slide, role-based redirect after login:
  - WebsiteAdmin → `dashboard-website-admin.html`
  - CafeAdmin → `dashboard-cafe-admin.html`
  - User → `index.html` (stays on site, cart activates)
- Header Login button added to `index.html` pointing to `login.html`

### Phase 3 — All Menu Module
- `AllMenuController` + CRUD DTOs
- `menu.js` — renders All Menu grid, category tabs, sold-out animation
- `dashboard-website-admin.html` All Menu CRUD panel (add/edit/delete, image URL input)
- Sold-out stamp animation wired

### Phase 4 — Daily Menu Module
- `DailyMenuController` + status toggle endpoint
- `dashboard-cafe-admin.html` Daily Menu panel — add today's items, qty field, toggle Available/SoldOut
- Public `index.html` Daily Menu section re-rendered from API (replaces hardcoded JS array)

### Phase 5 — Cart & Checkout
- `cart.js` — in-memory cart (survives page nav via `sessionStorage`), badge bounce animation
- Cart drawer updated: items from Daily Menu and/or All Menu mixed
- Checkout: calls `POST /api/orders`, receives order id + cafe payment details in response
- Checkout confirmation screen shows bank/card transfer instructions (from `CafePaymentDetails`)

### Phase 6 — Order / Payment Workflow + SignalR
- `OrderHub.cs` — groups: `order-{orderId}`, `cafe-admin`, `website-admin`
- `OrdersController` — status PATCH endpoints broadcast via hub
- `dashboard-cafe-admin.html` — live orders queue, Order status control (Received / Delivered), Payment status flip (Received)
- `dashboard-user.html` — order history, live badge updates via SignalR
- `dashboard-website-admin.html` — read-only orders view, live updates
- Cafe payment details editor in Cafe Admin dashboard

### Phase 7 — Contact Us
- `ContactController` — public POST, admin GET + read PATCH
- `index.html` contact form wired to API (replaces demo toast)
- `dashboard-website-admin.html` inbox panel — unread count badge, mark read

### Phase 8 — Website Admin Oversight
- Users table in Website Admin dashboard — enable/disable toggle
- Cafe Admin activity log view (orders placed/modified, daily menu changes, last login)

### Phase 9 — Polish
- Responsive pass across all pages (mobile header, drawer, dashboards)
- Empty states (no orders yet, empty menu, no messages)
- Error states (API down toast, 401 redirect to login)
- Accessibility: focus rings, aria-labels, keyboard nav on modals
- Performance: lazy-load menu images, debounce search/filter inputs

### Phase 10 — Deployment (Stewart Pakistan Server)
> Awaiting server details (Windows/IIS vs Linux, domain, Postgres location).
> Plan below covers both paths.

**Windows / IIS path:**
1. Install ASP.NET Core Hosting Bundle on server
2. `dotnet publish -c Release -o ./publish`
3. Create IIS site pointing at `publish/` folder
4. Add `web.config` (auto-generated by publish) for Kestrel reverse proxy
5. Set environment variables: `ConnectionStrings__Postgres`, `Jwt__Secret`, `ASPNETCORE_ENVIRONMENT=Production`
6. Serve static frontend from same IIS site's `wwwroot`

**Linux / Nginx path:**
1. `dotnet publish -c Release -o ./publish`
2. Create `systemd` service unit for the API on port 5000
3. Nginx reverse proxy: `location /api { proxy_pass http://localhost:5000; }`, static frontend served from `/var/www/cafe/`
4. HTTPS via Let's Encrypt / Certbot
5. Set env vars in systemd unit file

**Both paths — security checklist:**
- CORS locked to production domain only
- Force HTTPS redirect (`UseHttpsRedirection`)
- JWT secret in env var, never in source
- SQLite fallback path writable by app pool / service user
- `appsettings.Production.json` excluded from git

---

## Key Implementation Notes

- **Password hashing:** `BCrypt` (via `BCrypt.Net-Next` NuGet) for all stored passwords including seeded admins
- **JWT expiry:** 7 days for users, 1 day for admins (configurable)
- **Image storage:** Phase 3 uses URL input only (no file upload); binary upload can be added later
- **Cart persistence:** `sessionStorage` key `ca_cart`; cleared on logout or successful order submit
- **SignalR transport:** WebSockets with long-polling fallback; frontend uses the official `@microsoft/signalr` CDN script
- **SQLite fallback:** activated when env var `ConnectionStrings__Postgres` is absent; database file at `Data/cafe.db`
- **EF migrations:** run automatically on startup (`context.Database.MigrateAsync()`) so deployment is one-step
- **Seeder guard:** checks `Users.Any(u => u.Role == WebsiteAdmin)` before inserting; safe to restart

---

## What Gets Built First (Login Page Detail)

File: `login.html`

Structure:
```
<body class="auth-body">          ← hero-cafe.jpg full-bleed + dark overlay
  <div class="auth-orbs">        ← 3 blurred drifting circles (clay / gold / sky)
  <div class="auth-card">        ← floating card, --card background, --shadow
    <div class="auth-logo">      ← same logo-mark as header (☕ terracotta circle)
    <div class="auth-tabs">      ← Login | Sign Up tab slider
    <form id="loginForm">
      email input, password input (toggle reveal), Submit btn
    <form id="signupForm"> (hidden initially)
      full name, email, password, confirm password, Submit btn
    <p class="auth-switch">      ← "New here? Create account" link
```

CSS animations:
```css
@keyframes floatCard {
  0%, 100% { transform: translateY(0px); }
  50%       { transform: translateY(-10px); }
}
@keyframes orbDrift1 {
  0%, 100% { transform: translate(0, 0) scale(1); }
  50%       { transform: translate(30px, -20px) scale(1.1); }
}
/* orbDrift2, orbDrift3 at different offsets and durations */
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  20%, 60% { transform: translateX(-6px); }
  40%, 80% { transform: translateX(6px); }
}
```

JS flow (`auth.js`):
1. On Login submit → `POST /api/auth/login` → store JWT in `localStorage` as `ca_token`
2. Decode JWT role claim → redirect based on role
3. On Signup submit → `POST /api/auth/signup` → auto-login on success
4. Any 401 from any page → `window.location = 'login.html'`
