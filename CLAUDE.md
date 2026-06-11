# Med_Map API — CLAUDE.md

## Project Overview
ASP.NET Core Web API (.NET 10) for a pharmacy map platform. Customers can find nearby pharmacies, browse medicine inventories, and place orders. Pharmacies manage their inventory and order fulfillment.

**Remote:** https://github.com/GergesRasmy/Med_Map.git (team repo, owner: GergesRasmy)

---

## Tech Stack
- **Framework:** ASP.NET Core Web API, .NET 10
- **Database:** SQL Server + EF Core 10 with `NetTopologySuite` for geospatial queries (SRID 4326)
- **Auth:** ASP.NET Identity + JWT Bearer tokens. Two roles: `Customer`, `Pharmacy`
- **API Docs:** Swagger spec + Scalar UI at `/scalar` (dev only)
- **Payments:** Paymob integration (`PaymobService`) — not fully tested
- **Containerization:** Docker + `docker-compose.yml`

## Project Structure
```
Controllers/        — one controller per domain
DTO/                — input/output DTOs, grouped by domain
Models/             — EF Core entities
  ordersANDmedicine/  Orders, OrderItem, MedicineMaster, enums
  pharmacy/           PharmacyProfile, PharmacyInventory
  customer/           Customer
  AI/                 DoctorRequest, Recommendation, Wallet (not yet wired up)
Repositories/       — repository interfaces + implementations per domain
Services/           — AccountService, EmailService, OtpService, FileService, PaymobService
Filters/            — ValidateModelAttribute, MultipleResponseTypesOperationFilter
Constants/          — RoleConstants, ErrorCodes, SuccessCodes
Migrations/         — EF Core migrations
globalUsing.cs      — project-wide global usings
```

---

## API Endpoints

### Account (`/api/account`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/register` | None | Register user (username, email, password, role) |
| POST | `/login` | None | Login, returns JWT |
| POST | `/verifyOtp` | None | Verify OTP for new users |
| POST | `/requestNewOtp` | None | Request a new OTP |

### Customer (`/api/customer`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/customerPublicGet?id=` | None | Public customer data (must be active) |
| POST | `/register` | Customer JWT | First-time customer registration |
| PATCH | `/update` | Customer JWT | Partial update of customer data |

### User (`/api/user`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/privateGet` | Any JWT | Private profile (customer or pharmacy data) |

### Medicine (`/api/medicine`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/add` | Any (admin later) | Add medicine to master DB |
| GET | `/order/allMedicine` | None | Paginated list of all medicines |
| GET | `/getById?id=` | None | Get medicine by ID |
| PATCH | `/update` | Any (admin later) | Patch medicine fields |
| DELETE | `/delete?id=` | Any (admin later) | Delete medicine |
| GET | `/search?query=` | None | Search by name (paginated) |

### Orders (`/api/order`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/place` | Customer | Place an order (atomic: validates → transaction → deducts stock) |
| PATCH | `/update-status` | Pharmacy | Advance order status (body: `{orderId, nextStatus}`) |
| GET | `/myOrders` | Customer or Pharmacy | List caller's orders |
| GET | `?id=` | Customer or Pharmacy | Get single order by ID |
| PATCH | `/cancel/{orderId}` | Customer | Cancel an order (restores stock) |

### Pharmacy (`/api/pharmacy`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/register` | Pharmacy | Create pharmacy profile |
| PATCH | `/update` | Pharmacy | Update pending profile |
| PATCH | `/activateProfile?userId=` | Any (admin later) | Activate pending profile |
| GET | `/pharmacypublicGet?id=` | None | Public pharmacy data |
| GET | `/searchPharmacyByName?name=` | None | Search by name (paginated) |
| GET | `/nearestPharmacy?latitude=&longitude=&radiusInMeters=` | None | Spatial proximity search |

### Pharmacy Inventory (`/api/pharmacyInventory`)
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/insertMedicine` | Pharmacy | Add medicine batch (price, qty, expiry) |
| POST | `/updateInventory` | Pharmacy | Update an inventory entry |
| DELETE | `/removeMedicine` | Pharmacy | Remove medicine from inventory |
| GET | `/viewInventory?page=` | Pharmacy | View own inventory (paginated) |
| GET | `/viewMedicineBatches/{medicineId}` | Pharmacy | View all batches of a medicine |

---

## Key Patterns

### Response Base
All controllers extend `ResponceBaseController` (note the typo — do not rename, it's wired into DI). Use helper methods:
- `SuccessResponse(data, message, SuccessCodes.X)` — wraps `SuccessResponseDTO<T>`
- `ErrorResponse(message, ErrorCodes.X)` / `ErrorResponse(message, code, detail)` — wraps `ErrorResponseDTO<T>`

### Order State Machine
Orders support two fulfillment paths:

**Delivery:** `Recorded → Packaged → OutForDelivery → Delivered` (cancel allowed until OutForDelivery)

**Pickup:** `Recorded → Packaged → ReadyForPickup → Delivered` (cancel allowed until ReadyForPickup)

Cancellation always restores stock atomically inside a DB transaction.

### Atomic Order Creation
Order placement (`POST /api/order/place`) follows a two-phase pattern:
1. Validate all items and check stock *before* opening a transaction
2. Open transaction → insert order → deduct inventory → commit (or rollback on failure)

Price stored in `OrderItem.unitPrice` at order time (snapshot from inventory, not live medicine price).

### Unit of Work
`IUnitOfWork` / `UnitOfWork` wraps EF Core transaction lifecycle (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`). Used in `OrdersController` for order placement. Registered as Scoped in DI.

### Repository Pattern
Every domain has an `I{Domain}Repository` interface + implementation. All registered as `AddScoped` in `Program.cs`. Repos call `_context.SaveChangesAsync()` directly.

### Auth & Identity
- JWT claims: `ClaimTypes.NameIdentifier` = user ID, `ClaimTypes.Role` = role string
- `user.IsActive` must be true (OTP verified) before most operations
- Pharmacy endpoints resolve `pharmacy.ActiveProfile` from the user ID — most require an active profile

### Geospatial
Locations stored as `NetTopologySuite.Geometries.Point` with SRID 4326. Always create points as:
```csharp
var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
var point = factory.CreatePoint(new Coordinate(longitude, latitude)); // lon first!
```

---

## Current Local Changes (uncommitted)
- `Controllers/PharmacyInventoryController.cs` — stray TODO comment added
- `DTO/MedicineDTOs/AddMedicineDTO.cs` — MinLength removed, MaxLength relaxed 30→300
- `DTO/MedicineDTOs/UpdateMedicineDTO.cs` — same validation relaxation

## Remote Ahead (2 commits not yet pulled)
Commits `b17717d` and `eb04ed7` on `origin/master` include:
- `IUnitOfWork` / `UnitOfWork` added + registered in DI
- `OrdersController` fully rewritten: atomic placement, fixed pharmacy `myOrders` bug (was passing userId instead of ActiveProfile.Id), status update route changed to `PATCH /update-status` with body DTO, cancel stock restoration moved to repo layer
- `OrderRepository`: `CancelOrder` and `UpdateStatusAsync` now handle stock restoration inside their own transactions
- `OrderItem.unitPrice` populated at order creation (price snapshot)
- 2 new EF migrations: `fullfillment`, `orderunitprice`
- `UpdateOrderDTO` added (`{orderId, nextStatus}` strings)

---

## Development Notes
- Run with `docker-compose up` or `dotnet run`
- Migrations: `dotnet ef database update`
- `appsettings.json` holds connection string (`DefaultConnection`) and JWT config — not committed with secrets
- Swagger JSON: `/swagger/v1/swagger.json`, Scalar UI: `/scalar` (dev mode)
- Models in `AI/` folder (DoctorRequest, Recommendation, Wallet, WithdrawalRequest) exist in the schema but are not yet wired to any controller
