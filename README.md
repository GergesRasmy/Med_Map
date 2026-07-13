# MedLink API (Med_Map)

ASP.NET Core Web API (.NET 10) backend for MedLink — a pharmacy platform where
customers find nearby pharmacies, browse medicine inventories, and place orders,
while pharmacies manage their inventory, services, and order fulfillment.

## Tech Stack

- **Framework:** ASP.NET Core Web API, .NET 10
- **Database:** SQL Server + EF Core 10, with `NetTopologySuite` for geospatial queries (SRID 4326)
- **Auth:** ASP.NET Identity + JWT bearer tokens, 3 roles: `Customer`, `Pharmacy`, `Admin`
- **Real-time:** SignalR hub (`NotificationHub`) at `/hubs/notifications`
- **Email:** Mailtrap Send API (OTP codes for registration/password reset)
- **Payments:** Kashier integration
- **AI:** proxies to a separate PharmaAI service (medicine Q&A + OCR)
- **API docs:** Swagger JSON + Scalar UI (dev only)

## Project Structure

```
Controllers/    — one controller per domain
DTO/            — input/output DTOs, grouped by domain
Models/         — EF Core entities (customer, pharmacy, orders/medicine, AI)
Repositories/   — repository interfaces + implementations per domain
Services/       — EmailService, OtpService, FileService, KashierService, AiService, AccountService
Filters/        — ValidateModelAttribute, MultipleResponseTypesOperationFilter
Constants/      — RoleConstants (Roles.cs), ErrorCodes, SuccessCodes, Constant.cs (tunables)
Migrations/     — EF Core migrations
Seeders/        — dev-only demo data (admin/customer/pharmacy/medicine), toggled by Constant.IncludeSeeders
globalUsing.cs  — project-wide global usings
```

## Prerequisites

- .NET 10 SDK
- SQL Server (or SQL Server LocalDB on Windows)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Setup

1. **Clone**
   ```
   git clone https://github.com/GergesRasmy/Med_Map.git
   ```
2. **Restore dependencies**
   ```
   dotnet restore
   ```
3. **Configure secrets**
   Copy `appsettings.Example.json` to `appsettings.json` (gitignored — holds real
   local secrets and is never committed). The JWT key in the example already
   works out of the box. `Kashier` and `Mailtrap` values are placeholders —
   only needed if you're testing payments or real email delivery; otherwise
   OTP codes are also printed to the console (see `OtpService.cs`).
4. **Create the database**
   ```
   dotnet ef database update
   ```
5. **Run**
   ```
   dotnet run
   ```
   API listens on `http://localhost:5136` by default (see `Properties/launchSettings.json`).

On startup in `Development`, demo data (admin/customer/pharmacy accounts, sample
medicines, sample uploads) is seeded automatically. Set `Constant.IncludeSeeders`
to `false` in `Constants/Constant.cs` to skip this (e.g. against a shared dev DB).

## API Docs

In `Development` mode: Scalar UI at `/scalar`, raw OpenAPI JSON at
`/swagger/v1/swagger.json`.

## Auth

JWT bearer tokens, 3 roles: `Customer`, `Pharmacy`, `Admin`. Include
`Authorization: Bearer <token>` on any endpoint marked "Auth" below. Some
pharmacy-facing endpoints additionally require an active (admin-approved)
pharmacy profile.

## Endpoints

### Account — `/api/account`
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/register` | — | Register a user (customer or pharmacy), sends an OTP |
| POST | `/verifyOtp` | — | Verify registration OTP, activates the account |
| POST | `/requestNewOtp` | — | Resend a registration OTP |
| POST | `/forgotPassword` | — | Send a password-reset OTP |
| POST | `/resetPassword` | — | Reset password using a verified OTP |
| POST | `/login` | — | Log in, returns JWT |
| POST | `/logout` | ✓ | Revoke the current session (server-side session invalidation) |

### User — `/api/user`
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/privateGet` | ✓ | Caller's private profile (customer or pharmacy data) |
| PATCH | `/changePassword` | ✓ | Change password |

### Customer — `/api/customer`
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/customerPublicGet?id=` | — | Public customer data |
| POST | `/register` | Customer | First-time customer profile registration |
| PATCH | `/update` | Customer | Partial update of customer data |
| POST | `/avatar` | Customer | Upload customer avatar |

### Pharmacy — `/api/pharmacy`
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/register` | Pharmacy | Create pharmacy profile (pending admin approval) |
| PATCH | `/update` | Pharmacy | Update pending profile |
| PATCH | `/updateProfile` | Pharmacy | Update an active profile |
| POST | `/activateProfile?userId=` | Admin | Approve a pending pharmacy profile |
| POST | `/rejectProfile?userId=` | Admin | Reject a pending profile, with a reason |
| GET | `/pharmacyPublicGet?id=` | — | Public pharmacy data |
| GET | `/searchPharmacyByName?name=` | — | Search by name (paginated) |
| GET | `/nearestPharmacy?latitude=&longitude=&radiusInMeters=` | — | Spatial proximity search |
| GET | `/pharmacyDetails/{id}` | Admin | Full pharmacy details |
| GET | `/pharmacies` | Admin | Search/filter all pharmacies |

### Pharmacy Inventory — `/api/pharmacyInventory`  (all Pharmacy)
| Method | Path | Description |
|---|---|---|
| POST | `/insertMedicine` | Add a medicine batch (price, qty, expiry) |
| POST | `/updateInventory` | Update an inventory entry |
| DELETE | `/removeMedicine` | Remove a medicine from inventory |
| GET | `/viewInventory?page=` | View own inventory (paginated) |
| GET | `/search?query=&pharmacyId=&page=` | Search own inventory |
| GET | `/viewMedicineBatches/{medicineId}` | View all batches of a medicine |

### Pharmacy Service — `/api/pharmacyService`  (all Pharmacy)
| Method | Path | Description |
|---|---|---|
| POST | `/add` | Add an in-store service (e.g. blood pressure check) |
| PATCH | `/update` | Update a service |
| DELETE | `/delete?id=` | Delete a service |
| GET | `/myServices?page=&pageSize=` | List own services |
| GET | `/search?query=&pharmacyId=&page=&pageSize=` | Search services |
| GET | `/{id}` | Get a single service |

### Medicine — `/api/medicine`
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/add` | — (admin later) | Add medicine to the master catalog |
| PATCH | `/update` | — (admin later) | Patch medicine fields |
| DELETE | `/delete?id=` | — (admin later) | Delete medicine |
| GET | `/allMedicine` | — | Paginated list of all medicines |
| GET | `/getById?id=` | — | Get medicine by ID |
| GET | `/search?query=` | — | Search by name (paginated) |

### Orders — `/api/order`
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/place` | Customer | Place an order (atomic: validate → transaction → deduct stock) |
| POST | `/validate-cart` | Customer | Validate a cart (stock, fees) before placing |
| PATCH | `/update-status` | Pharmacy | Advance order status (body: `{orderId, nextStatus}`) |
| GET | `/stats` | ✓ | Order statistics for the caller |
| GET | `/myOrders?page=&pageSize=&status=` | ✓ | List caller's orders |
| GET | `?id=` | ✓ | Get a single order |
| PATCH | `/cancel/{orderId}` | ✓ | Cancel an order (restores stock) |

Order fulfillment paths:
- **Delivery:** `Recorded → Packaged → OutForDelivery → Delivered` (cancel allowed until `OutForDelivery`)
- **Pickup:** `Recorded → Packaged → ReadyForPickup → Delivered` (cancel allowed until `ReadyForPickup`)

### Payments — `/api/payments`
| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/initiate` | ✓ | Start a Kashier payment for an order |
| GET | `/status/{orderId}` | ✓ | Check payment status |
| POST | `/webhook` | — | Kashier payment webhook |

### Wallet — `/api/wallet`  (all Pharmacy)
| Method | Path | Description |
|---|---|---|
| GET | `/` | Get wallet balance |
| POST | `/setPin` | Set withdrawal PIN |
| POST | `/changePin` | Change withdrawal PIN |
| GET | `/transactions` | List wallet transactions |
| POST | `/withdraw` | Request a withdrawal |

### Admin Wallet — `/api/admin/wallet`  (all Admin)
| Method | Path | Description |
|---|---|---|
| GET | `/withdrawals` | List withdrawal requests |
| PATCH | `/withdrawals/{id}/complete` | Mark a withdrawal complete |
| PATCH | `/withdrawals/{id}/cancel` | Cancel a withdrawal |

### Files — `/api/files`
| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/avatars/{fileName}` | — | Serve a customer avatar |
| GET | `/medicine-images/{fileName}` | — | Serve a medicine image |
| GET | `/licenses/{fileName}` | Admin, Pharmacy | Serve a pharmacy license document |
| GET | `/national-ids/{fileName}` | Admin, Pharmacy | Serve a national ID document |

### AI — `/api/ai`  (all Customer)
| Method | Path | Description |
|---|---|---|
| POST | `/query` | Ask the medicine chatbot a question |
| POST | `/ocr/medicine` | OCR a medicine package image |
| POST | `/ocr/prescription` | OCR a prescription image |

Proxies to the separate PharmaAI FastAPI service (`AiService:BaseUrl` in config).

## Key Patterns

- **Response envelope:** all controllers extend `ResponceBaseController` (typo
  intentional — wired into DI, don't rename) and return `SuccessResponseDTO<T>` /
  `ErrorResponseDTO<T>` via `SuccessResponse(...)` / `ErrorResponse(...)`.
- **Atomic order placement:** validate everything first, then open a DB
  transaction to insert the order and deduct stock (commit or roll back).
  `IUnitOfWork` wraps the EF Core transaction lifecycle.
- **Geospatial points:** always `lon` before `lat`:
  ```csharp
  var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
  var point = factory.CreatePoint(new Coordinate(longitude, latitude));
  ```
