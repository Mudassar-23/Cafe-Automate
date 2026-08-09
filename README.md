# Cafe Automate — Order-Ahead Cafe System

A full-stack order-ahead web application for a cafe. Customers browse the menu, add items to a cart, and checkout. The Cafe Admin manages daily specials and fulfills orders in real time. The Website Admin controls the permanent menu, users, and oversight.

---

## Table of Contents

- [Live URLs (dev)](#live-urls-dev)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Environment Variables (.env)](#environment-variables-env)
- [Admin Credentials](#admin-credentials)
- [Roles & Permissions](#roles--permissions)
- [Features by Role](#features-by-role)
- [API Reference](#api-reference)
- [Database Schema](#database-schema)
- [Order & Payment Flow](#order--payment-flow)
- [Real-Time Updates (SignalR)](#real-time-updates-signalr)
- [Deployment](#deployment)

---

## Live URLs (dev)

| Service | URL |
|---|---|
| Public website | http://localhost:5500/index.html |
| Login page | http://localhost:5500/login.html |
| Cafe Admin dashboard | http://localhost:5500/dashboard-cafe-admin.html |
| Website Admin dashboard | http://localhost:5500/dashboard-website-admin.html |
| User dashboard | http://localhost:5500/dashboard-user.html |
| API (Swagger) | http://localhost:5112/swagger |

---

## Tech Stack

### Frontend
| Layer | Choice |
|---|---|
| Markup | HTML5 |
| Styling | CSS3 — custom properties, no framework |
| Logic | Vanilla JavaScript (ES2022) |
| Real-time | SignalR JS client (CDN) |
| Fonts | Fraunces (display) + Outfit (body) via Google Fonts |

### Backend
| Layer | Choice |
|---|---|
| Runtime | .NET 8 (ASP.NET Core Web API) |
| ORM | Entity Framework Core 8 |
| Database | SQLite (dev fallback) / PostgreSQL (production) |
| Auth | JWT Bearer (HS256), BCrypt password hashing |
| Real-time | ASP.NET Core SignalR |
| Config | DotNetEnv — loads `.env` at startup |

---

## Project Structure

```
cafe/
├── .env                          ← secrets (never commit — see .gitignore)
├── .env.example                  ← copy this to .env to get started
├── .gitignore
├── README.md
├── IMPLEMENTATION_PLAN.md
│
├── index.html                    ← public site (hero, daily menu, all menu, contact)
├── login.html                    ← login + signup
├── dashboard-user.html           ← customer order tracking
├── dashboard-cafe-admin.html     ← cafe staff: orders, daily menu, payment details
├── dashboard-website-admin.html  ← owner: all menu, users, orders overview, inbox
├── hero-cafe.jpg                 ← hero background image
│
├── css/
│   ├── tokens.css                ← CSS custom properties (colours, radii, fonts)
│   ├── main.css                  ← public site styles
│   ├── login.css                 ← auth card + floating orb animations
│   └── dashboard.css             ← shared dashboard shell
│
├── js/
│   ├── api.js                    ← fetch wrapper (JWT attach, 401 logout, SignalR helper)
│   ├── auth.js                   ← login / signup / role-based redirect
│   ├── cart.js                   ← sessionStorage cart, badge bounce
│   ├── menu.js                   ← public menu rendering, sold-out stamp
│   └── orders.js                 ← cart checkout, order confirmation modal
│
└── CafeAutomate.Api/             ← ASP.NET Core Web API
    ├── Program.cs                ← app startup, DI, middleware pipeline
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── appsettings.Production.json
    │
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── UsersController.cs
    │   ├── AllMenuController.cs
    │   ├── DailyMenuController.cs
    │   ├── OrdersController.cs
    │   ├── CafePaymentController.cs
    │   └── ContactController.cs
    │
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Migrations/
    │
    ├── DTOs/
    │   ├── AuthDtos.cs
    │   ├── MenuDtos.cs
    │   ├── OrderDtos.cs
    │   └── OtherDtos.cs
    │
    ├── Hubs/
    │   └── OrderHub.cs           ← SignalR hub (groups: cafe-admin, website-admin, order-{id})
    │
    ├── Middleware/
    │   └── RoleGuard.cs          ← ClaimsPrincipal extension methods (IsCafeAdmin, etc.)
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
    └── Services/
        ├── TokenService.cs       ← JWT generation
        └── SeederService.cs      ← seeds admin accounts from .env on first run
```

---

## Getting Started

### Option A — Docker (recommended)

**Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop/)

```bash
git clone <repo-url>
cd cafe
cp .env.example .env      # edit .env — set passwords and secrets
docker-compose up --build
```

| URL | What it serves |
|---|---|
| http://localhost:5500 | Public website + all dashboards |
| http://localhost:5500/swagger | Swagger API docs (via nginx proxy) |
| http://localhost:5112 | Direct API access (also exposed) |

Everything runs in one command. nginx handles static files and proxies `/api` and `/hubs` to the backend. Postgres is used as the database.

Stop everything:
```bash
docker-compose down          # keep database volumes
docker-compose down -v       # also wipe database volumes
```

---

### Option B — Local dev (no Docker)

**Prerequisites:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Python 3](https://www.python.org/)

```bash
git clone <repo-url>
cd cafe
cp .env.example .env      # edit .env
```

Start the API (SQLite is used automatically when Postgres is unreachable):
```bash
dotnet run --project CafeAutomate.Api --urls http://localhost:5112
```

On first run the API will:
- Create the SQLite database at `CafeAutomate.Api/App_Data/cafe.db`
- Apply all EF Core migrations automatically
- Seed the two admin accounts from your `.env` values

Serve the static frontend:
```bash
python -m http.server 5500
```

Open **http://localhost:5500/index.html** in your browser.
The frontend auto-detects it's running on port 5500 and hits the API directly at `http://localhost:5112`.

---

## Environment Variables (.env)

Copy `.env.example` to `.env` and fill in your values. The API reads this file automatically at startup via `DotNetEnv`.

```dotenv
# JWT signing secret — change this to something long and random in production
Jwt__Secret=your_jwt_secret_here

# Website Admin seed account (created once on first startup)
Seed__WebsiteAdmin__FullName=Website Admin
Seed__WebsiteAdmin__Email=admin@yourdomain.com
Seed__WebsiteAdmin__Password=your_secure_password

# Cafe Admin seed account
Seed__CafeAdmin__FullName=Cafe Admin
Seed__CafeAdmin__Email=cafeadmin@yourdomain.com
Seed__CafeAdmin__Password=your_secure_password
```

> **Note:** The seeder only creates each admin account once (on first startup when no account with that role exists). If you change the email or password in `.env` after first run, use the Website Admin dashboard's **Change Password** action to update the stored credentials.

### Database — PostgreSQL with automatic SQLite fallback

```dotenv
# PostgreSQL (production) — if connection fails, falls back to SQLite
DATABASE_URL=postgresql://postgres:yourpassword@localhost:5432/CAFEAuto

# SQLite fallback (auto-used if PostgreSQL is unavailable)
DATABASE_FALLBACK_URL=sqlite:///./App_Data/cafe.db
```

At startup the API opens a test connection to `DATABASE_URL` (5 second timeout).
If it succeeds, Postgres is used. If it fails for any reason — server down, wrong
password, missing database — the API logs the reason and boots on the SQLite file
instead, so the cafe can keep taking orders. Watch for this line in the console:

```
[db] Using PostgreSQL.
[db] PostgreSQL unavailable (…). Falling back to SQLite.
```

Notes:
- Percent-encode special characters in the password (`@` → `%40`, `#` → `%23`).
- `DATABASE_FALLBACK_URL` is relative to the `CafeAutomate.Api` folder. Avoid
  `./data` — on Windows it collides with the `Data/` source folder.
- The two databases are separate stores; data written to one is not visible in
  the other.

---

## Admin Credentials

These are set in `.env` and seeded automatically. The default values for local development are:

| Role | Email | Password |
|---|---|---|
| Website Admin | `admin1se@stewart.com` | `admin@1234#` |
| Cafe Admin | `cafe.admin@stewart.com` | `admin2@4321#` |
| Customer | self-registration via Sign Up | — |

> Change these in `.env` before going to production.

---

## Roles & Permissions

| Capability | Website Admin | Cafe Admin | Customer |
|---|:---:|:---:|:---:|
| View public menu & contact form | ✓ | ✓ | ✓ |
| Register / log in | ✓ | ✓ | ✓ |
| Add items to cart & checkout | — | — | ✓ |
| View own orders + live status | — | — | ✓ |
| Manage All Menu (add / edit / delete) | ✓ | — | — |
| Manage Daily Menu (add / edit / toggle sold-out) | — | ✓ | — |
| View Daily Menu (read-only) | ✓ | — | — |
| View all orders + live updates | ✓ | ✓ | — |
| Update order status (Pending → Received → Delivered) | — | ✓ | — |
| Update payment status (Pending → Received) | — | ✓ | — |
| Set bank / card payment details | — | ✓ | — |
| View contact inbox + mark read | ✓ | — | — |
| List all users (all roles) | ✓ | — | — |
| Enable / disable user accounts | ✓ | — | — |
| Change any user's password | ✓ | — | — |
| Delete user accounts | ✓ | — | — |

---

## Features by Role

### Public site (`index.html`)
- Hero section with cafe info, CTA buttons, animated marquee
- **Daily Menu** — today's specials, loaded live from the API, with "Sold Out" stamp
- **All Menu** — full catalogue with category tabs (Food, Drink)
- **Cart drawer** — add from both menus, quantity controls, running total
- **Checkout** — places the order, shows bank transfer details for payment
- **Contact Us** — sends a message directly to the Website Admin inbox
- Header Login button → My Orders button after sign-in

### Login / Sign Up (`login.html`)
- Animated floating card with blurred accent orbs
- Toggle between Login and Sign Up tabs
- After login, auto-redirects to the correct dashboard by role
- Password reveal toggle

### Customer Dashboard (`dashboard-user.html`)
- Order history table with live status badges (via SignalR)
- Profile panel

### Cafe Admin Dashboard (`dashboard-cafe-admin.html`)
- **Live Orders** — real-time incoming orders (SignalR push), status counters
- **Daily Menu** — add today's specials (name, emoji, price, quantity), edit, toggle sold-out, delete
- **Payment Details** — set bank/card info shown to customers at checkout

### Website Admin Dashboard (`dashboard-website-admin.html`)
- **All Menu** — full CRUD for the permanent menu (name, description, price, image URL, category, availability toggle)
- **All Orders** — read-only oversight with live status updates
- **Users** — lists every account across all roles; enable/disable, change password, delete (with self-delete guard)
- **Contact Inbox** — all submitted messages, unread count badge, mark-as-read
- **Daily Menu** — read-only view of today's items

---

## API Reference

All write endpoints require a `Bearer` token in the `Authorization` header.

### Auth

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/signup` | Public | Register a new customer account |
| POST | `/api/auth/login` | Public | Login, returns JWT |
| GET | `/api/auth/me` | Any | Return current user info |

### Users

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/users` | Website Admin | List all users |
| PATCH | `/api/users/{id}/status` | Website Admin | Enable or disable an account |
| PATCH | `/api/users/{id}/password` | Website Admin | Change a user's password |
| DELETE | `/api/users/{id}` | Website Admin | Delete an account (cascades orders) |

### All Menu

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/all-menu` | Public | List all menu items |
| POST | `/api/all-menu` | Website Admin | Add a menu item |
| PUT | `/api/all-menu/{id}` | Website Admin | Edit a menu item |
| DELETE | `/api/all-menu/{id}` | Website Admin | Delete a menu item |

### Daily Menu

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/daily-menu` | Public | Today's daily items |
| POST | `/api/daily-menu` | Cafe Admin | Add today's special |
| PUT | `/api/daily-menu/{id}` | Cafe Admin | Edit a daily item |
| PATCH | `/api/daily-menu/{id}/status` | Cafe Admin | Toggle Available / Sold Out |
| DELETE | `/api/daily-menu/{id}` | Cafe Admin | Delete a daily item |

### Orders

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/orders` | Customer | Place order from cart |
| GET | `/api/orders/mine` | Customer | Customer's own orders |
| GET | `/api/orders` | Cafe Admin / Website Admin | All orders |
| PATCH | `/api/orders/{id}/order-status` | Cafe Admin | Update order status |
| PATCH | `/api/orders/{id}/payment-status` | Cafe Admin | Update payment status |

### Cafe Payment Details

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/cafe-payment-details` | Public | Bank/card info for checkout |
| PUT | `/api/cafe-payment-details` | Cafe Admin | Update bank/card info |

### Contact

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/contact` | Public | Submit a contact message |
| GET | `/api/contact` | Website Admin | List all messages |
| PATCH | `/api/contact/{id}/read` | Website Admin | Mark a message as read |

### SignalR Hub

```
ws://localhost:5112/hubs/orders
```

Token passed via query string: `?access_token=<jwt>`

| Event (server → client) | Received by | Payload |
|---|---|---|
| `NewOrder` | Cafe Admin, Website Admin | Full order object |
| `OrderStatusUpdated` | Customer (own order), Website Admin | `{ id, orderStatus }` |
| `PaymentStatusUpdated` | Customer (own order), Website Admin | `{ id, paymentStatus }` |

---

## Database Schema

### Users
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| FullName | string | |
| Email | string | Unique, stored lowercase |
| PasswordHash | string | BCrypt |
| Role | int | 1 = WebsiteAdmin, 2 = CafeAdmin, 3 = User |
| IsActive | bool | Disabled accounts cannot log in |
| CreatedAt | DateTime | UTC |

### AllMenuItems
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Name | string | |
| Description | string | |
| Price | decimal(10,2) | |
| ImageUrl | string | URL only |
| Category | string | food / drink |
| IsAvailable | bool | |
| CreatedAt | DateTime | |

### DailyMenuItems
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Name | string | |
| Price | decimal(10,2) | |
| Quantity | int | |
| Status | enum | Available / SoldOut |
| Emoji | string | Displayed on the card |
| Date | DateOnly | Filter by today |
| CreatedAt | DateTime | |

### Orders
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| UserId | int FK → Users | Cascade delete |
| OrderStatus | enum | Pending / Received / Delivered |
| PaymentStatus | enum | Pending / Received |
| TotalAmount | decimal(10,2) | |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

### OrderItems
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| OrderId | int FK → Orders | Cascade delete |
| SourceType | enum | DailyMenu / AllMenu |
| MenuItemId | int | ID in source table (snapshot) |
| ItemNameSnapshot | string | Name at time of order |
| UnitPriceSnapshot | decimal(10,2) | Price at time of order |
| Quantity | int | |

### CafePaymentDetails *(single row)*
| Column | Type |
|---|---|
| AccountHolderName | string |
| BankName | string |
| AccountNumber | string |
| IBANOrCardNumber | string |
| Instructions | string |
| UpdatedAt | DateTime |

### ContactMessages
| Column | Type |
|---|---|
| Id | int PK |
| Name | string |
| Email | string |
| Message | string |
| SubmittedAt | DateTime |
| IsRead | bool |

---

## Order & Payment Flow

```
Customer adds items → places order
         │
         ▼
Order created  →  OrderStatus: Pending  |  PaymentStatus: Pending
         │  SignalR pushes to Cafe Admin dashboard
         ▼
Cafe Admin acknowledges  →  OrderStatus: Received
         │
Customer transfers payment (bank/card details shown at checkout)
         │
Cafe Admin confirms receipt  →  PaymentStatus: Received
         │
Food prepared and served
         │
Cafe Admin marks  →  OrderStatus: Delivered
         │
         ▼
Live status badges update in real-time on:
  • Customer's dashboard
  • Cafe Admin's orders queue
  • Website Admin's oversight view
```

Both `OrderStatus` and `PaymentStatus` are updated **independently** — a customer can pay before or after the order is received.

---

## Real-Time Updates (SignalR)

The API broadcasts events over SignalR. No polling needed — all dashboards update instantly.

| Client joins group | When |
|---|---|
| `cafe-admin` | Cafe Admin logs in |
| `website-admin` | Website Admin logs in |
| `order-{id}` | Customer opens their dashboard |

JWT is passed to the hub via `?access_token=<token>` query string (SignalR cannot send custom headers on WebSocket upgrade).

---

## Deployment

### Windows / IIS

1. Install the [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8)
2. Publish:
   ```bash
   dotnet publish CafeAutomate.Api -c Release -o ./publish
   ```
3. Create an IIS site pointing at `./publish`
4. Set environment variables on the application pool:
   - `Jwt__Secret`
   - `DATABASE_URL` (if using Postgres)
   - `DATABASE_FALLBACK_URL` (SQLite path used when Postgres is unreachable)
   - `ASPNETCORE_ENVIRONMENT=Production`
5. Serve the static frontend files (`index.html`, `css/`, `js/`, etc.) from the same or a separate IIS site.

### Linux / Nginx

1. Publish:
   ```bash
   dotnet publish CafeAutomate.Api -c Release -o ./publish
   ```
2. Create a `systemd` service for the API on port 5000.
3. Configure Nginx:
   ```nginx
   location /api { proxy_pass http://localhost:5000; }
   location /hubs { proxy_pass http://localhost:5000; }
   location / { root /var/www/cafe; try_files $uri $uri/ /index.html; }
   ```
4. HTTPS via Let's Encrypt / Certbot.
5. Set env vars in the systemd unit file (never in source).

### Production security checklist

- [ ] Change all `.env` credentials from the dev defaults
- [ ] Set `Jwt__Secret` to a long random string (32+ characters)
- [ ] Lock `AllowedOrigins` in `appsettings.json` to your production domain
- [ ] Never commit `.env` to git (already in `.gitignore`)
- [ ] Use PostgreSQL instead of SQLite
- [ ] Enable HTTPS and keep `UseHttpsRedirection` active

---

## Design Tokens

```css
--bg:         #f7ede0   /* warm cream background */
--card:       #fffaf2   /* card surface */
--sand:       #e9c6a3   /* subtle border / divider */
--clay:       #c1592e   /* primary terracotta — buttons, badges */
--clay-dark:  #92401f   /* hover state */
--gold:       #d9a256   /* accent */
--charcoal:   #221a15   /* primary text */
--sky:        #3f6f92   /* info / Website Admin accent */
--sage:       #6f8f5c   /* success green */
--cream:      #fff2e2   /* input background */
--radius:     20px      /* border radius */
```

Fonts: **Fraunces** (display headings) + **Outfit** (body / UI) via Google Fonts.

---

*Built for Cafe Automate, Lahore.*
