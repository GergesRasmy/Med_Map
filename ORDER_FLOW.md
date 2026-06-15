# Order Flow — Med_Map API

> Reference for Flutter integration. All string enum fields are **case-insensitive** on the server.

---

## Enum Values

### `paymentOption` / `PaymentOptions`
| Value | Notes |
|-------|-------|
| `"Cash"` | Pickup only |
| `"Online"` | Required for Delivery; payment processed via Kashier |

### `fulfillmentType` / `FulfillmentType`
| Value | Notes |
|-------|-------|
| `"Delivery"` | Order shipped to customer location |
| `"Pickup"` | Customer picks up at pharmacy |

### `status` / `StatusList`
| Value | Meaning |
|-------|---------|
| `"Pending"` | Awaiting online payment confirmation |
| `"Recorded"` | Order confirmed, waiting for pharmacy to act |
| `"Packaged"` | Pharmacy has packed the order |
| `"OutForDelivery"` | Delivery only — courier dispatched |
| `"ReadyForPickup"` | Pickup only — ready at counter |
| `"Delivered"` | Order completed |
| `"Canceled"` | Order cancelled (stock restored automatically) |

> `"Confirmed"` exists in the enum but is not used in any current flow.

---

## Business Rules

- **Delivery requires Online payment.** Sending `fulfillmentType: "Delivery"` with `paymentOption: "Cash"` returns 400.
- **Cash + Pickup** is valid and starts at `Recorded`.
- **Online payment** (any fulfillment) starts at `Pending` until payment is confirmed.
- **Item quantity** is capped at 1–10 per line item (server enforced).
- **Price snapshot:** `unitPrice` in each order item is locked at placement time from the pharmacy's inventory — it does not change if the pharmacy later updates the price.

---

## State Machines

### Delivery path
```
Recorded → Packaged → OutForDelivery → Delivered
```
- Pharmacy can also move any of those to `Canceled` (including from `OutForDelivery`).
- Customer can cancel only from `Recorded` or `Packaged`.

### Pickup path
```
Recorded → Packaged → ReadyForPickup → Delivered
```
- Pharmacy can move `Recorded` or `Packaged` to `Canceled`.
- `ReadyForPickup → Canceled` is **not** allowed (order is already prepared).
- Customer can cancel only from `Recorded` or `Packaged`.

---

## Endpoints

### 1. Validate Cart (call before placing)
```
POST /api/order/validate-cart
Authorization: Bearer <Customer JWT>
Content-Type: application/json
```

Reconciles the local (possibly stale) cart against current server state **before** the customer commits to placing an order. It does **not** write anything or reserve stock — it just reports back fresh prices, current stock, the delivery fee for each pharmacy, and the platform fees.

The cart may span more than one pharmacy, so the body is a **JSON array**, one entry per pharmacy. The shapes match the client `CartPharmacyOrder` / `CartItem` models, so the local cart can be posted verbatim. The server only relies on `pharmacyId` and each item's `medicineId` + `quantity`; the other fields are accepted and (for names/prices) echoed back for diffing.

**Request body:**
```json
[
  {
    "pharmacyId": "user-id-string-of-the-pharmacy",
    "pharmacyName": "Al Ezaby",
    "pharmacyLatitude": 30.0444,
    "pharmacyLongitude": 31.2357,
    "deliveryAvailability": true,
    "paymentOption": "Online",
    "fulfillmentType": "Delivery",
    "deliveryLatitude": 30.05,
    "deliveryLongitude": 31.24,
    "deliveryAddressDescription": "Flat 3, Tahrir St.",
    "items": [
      {
        "medicineId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "quantity": 2,
        "unitPrice": 45.00,
        "tradeName": "Panadol",
        "genericName": "Paracetamol",
        "priceUnitIsoCode": "EGP"
      }
    ]
  }
]
```

**Success response (200):**
```json
{
  "data": {
    "pharmacies": [
      {
        "pharmacyId": "user-id-string-of-the-pharmacy",
        "pharmacyName": "Al Ezaby",
        "found": true,
        "deliveryAvailable": true,
        "deliveryFee": 25.00,
        "items": [
          {
            "medicineId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "tradeName": "Panadol",
            "genericName": "Paracetamol",
            "currentUnitPrice": 48.00,
            "previousUnitPrice": 45.00,
            "priceChanged": true,
            "priceUnitIsoCode": "EGP",
            "requestedQuantity": 2,
            "availableQuantity": 10,
            "isAvailable": true,
            "lineTotal": 96.00,
            "message": "Price updated"
          }
        ],
        "subtotal": 96.00,
        "isValid": true
      }
    ],
    "fees": {
      "appFee": 5.00,
      "cashOnDeliveryFee": 15.00,
      "onlineFee": 7.00
    },
    "isValid": true
  },
  "message": "Cart validated",
  "code": "data_retrieved_successfully"
}
```

**Per-pharmacy fields:**
| Field | Meaning |
|-------|---------|
| `found` | `false` if the pharmacy no longer exists or has no active profile (all its items come back `isAvailable: false`). |
| `pharmacyName` | Authoritative name from the active profile — may differ from the stale client value. |
| `deliveryAvailable` | The profile's `HaveDelivary` flag. |
| `deliveryFee` | The pharmacy profile's `DeliveryFee`. Returned as `0` when the pharmacy doesn't offer delivery. |
| `subtotal` | Sum of `lineTotal` over available items, at **current** prices. Excludes `deliveryFee` and all platform fees. |
| `isValid` | `true` only when the pharmacy was found and every item is available at the requested quantity. |

**Per-item fields:**
| Field | Meaning |
|-------|---------|
| `currentUnitPrice` | Fresh price from the pharmacy inventory (`null` if no longer stocked). |
| `previousUnitPrice` | The `unitPrice` the client sent — echoed for diffing. |
| `priceChanged` | `true` when `previousUnitPrice` was sent and differs from `currentUnitPrice`. |
| `availableQuantity` | Current stock (`0` if not stocked). |
| `isAvailable` | `true` when stock ≥ requested quantity (same rule `/place` enforces). |
| `lineTotal` | `currentUnitPrice × requestedQuantity`, or `0` when unavailable. |
| `message` | Note when something is off: `"Out of stock"`, `"Only N in stock"`, `"No longer available at this pharmacy"`, `"Pharmacy not found or unavailable"`, or `"Price updated"`. |

**Top-level fields:**
- `fees` — platform fees the client should display/add on top: `appFee`, `cashOnDeliveryFee`, `onlineFee` (server constants).
- `isValid` — `true` only when **every** pharmacy in the cart is valid.

> This endpoint never deducts or holds stock. A cart that validates can still fail at `/place` if stock is depleted in between — `/place` re-checks atomically.

**Error cases:**
| Condition | HTTP | Description |
|-----------|------|-------------|
| Empty / missing array | 400 | `"Cart must contain at least one pharmacy"` |

---

### 2. Place Order
```
POST /api/order/place
Authorization: Bearer <Customer JWT>
Content-Type: application/json
```

**Request body:**
```json
{
  "paymentOption": "Cash",
  "fulfillmentType": "Pickup",
  "deliveryLongitude": 31.2357,
  "deliveryLatitude": 30.0444,
  "pharmacyId": "user-id-string-of-the-pharmacy",
  "items": [
    { "medicineId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "quantity": 2 },
    { "medicineId": "7cb93a12-1234-4abc-9def-aabbccddeeff", "quantity": 1 }
  ]
}
```

> `pharmacyId` is the pharmacy's **user ID** (string), the same ID returned by `/api/pharmacy/pharmacypublicGet`.  
> `deliveryLongitude` / `deliveryLatitude` are always required, even for Pickup (pass the pharmacy's coordinates or the customer's location).

**Success response (200):**
```json
{
  "data": {
    "id": "a1b2c3d4-0000-0000-0000-000000000000",
    "createdAt": "2026-06-14T10:00:00Z",
    "totalAmount": 125.50,
    "status": "Recorded",
    "fulfillmentType": "Pickup",
    "items": [
      { "MedicineName": "Panadol", "Quantity": 2, "UnitPrice": 45.00 },
      { "MedicineName": "Brufen", "Quantity": 1, "UnitPrice": 35.50 }
    ]
  },
  "message": "Order created successfully",
  "code": "DataCreated"
}
```

**Error cases:**
| Condition | HTTP | Description |
|-----------|------|-------------|
| Delivery + Cash | 400 | `"Delivery requires card payment"` |
| Empty items list | 400 | `"Order must contain at least one item"` |
| Medicine not in pharmacy inventory | 400 | `"Medicine not in pharmacy inventory"` |
| Insufficient stock | 400 | `"Not enough stock for <TradeName>"` |
| Invalid enum string | 400 | `"Invalid payment option"` / `"Invalid fulfillment type"` |

---

### 3. Update Order Status (Pharmacy only)
```
PATCH /api/order/update-status
Authorization: Bearer <Pharmacy JWT>
Content-Type: application/json
```

**Request body:**
```json
{
  "orderId": "a1b2c3d4-0000-0000-0000-000000000000",
  "nextStatus": "Packaged"
}
```

> `nextStatus` must follow the state machine for the order's fulfillment type. Invalid transitions return 400.

**Success response (200):** same `OrderResponseDTO` shape as Place Order.

**Error cases:**
| Condition | HTTP | Description |
|-----------|------|-------------|
| Invalid status string | 400 | `"Invalid status value"` |
| Illegal transition | 400 | `"Cannot transition from X to Y for a Z order"` |
| Order belongs to another pharmacy | 400 | `"Unauthorized"` |

---

### 4. Get My Orders
```
GET /api/order/myOrders
Authorization: Bearer <Customer or Pharmacy JWT>
```

No request body or query params. Returns all orders belonging to the caller.

**Success response (200):**
```json
{
  "data": [ /* array of OrderResponseDTO */ ],
  "message": "Orders retrieved successfully",
  "code": "DataRetrieved"
}
```

Returns an empty array (not an error) if the caller has no orders.

---

### 5. Get Order by ID
```
GET /api/order?id=a1b2c3d4-0000-0000-0000-000000000000
Authorization: Bearer <Customer or Pharmacy JWT>
```

Returns a single order. Customers can only fetch their own orders; pharmacies can only fetch orders placed with them.

**Success response (200):** single `OrderResponseDTO` wrapped in `data`.

---

### 6. Cancel Order (Customer only)
```
PATCH /api/order/cancel/{orderId}
Authorization: Bearer <Customer JWT>
```

No request body. `orderId` is the order's Guid in the URL path.

**Example:**
```
PATCH /api/order/cancel/a1b2c3d4-0000-0000-0000-000000000000
```

Stock is restored automatically inside a DB transaction.

**Success response (200):**
```json
{
  "data": "Order cancelled successfully",
  "message": "Order cancelled successfully",
  "code": "DataUpdated"
}
```

**Error case:** returns 400 with `"Order cannot be cancelled at this stage."` if the order is in `OutForDelivery`, `ReadyForPickup`, `Delivered`, or already `Canceled`.

---

## OrderResponseDTO Shape (all endpoints)

```json
{
  "id": "guid-string",
  "createdAt": "2026-06-14T10:00:00Z",
  "totalAmount": 125.50,
  "status": "Recorded",
  "fulfillmentType": "Pickup",
  "items": [
    {
      "MedicineName": "string",
      "Quantity": 2,
      "UnitPrice": 45.00
    }
  ]
}
```

> Note the **mixed casing** in `items`: `MedicineName`, `Quantity`, `UnitPrice` start with uppercase — match this exactly when deserializing.

---

## Typical Flutter Flow

```
1. Customer browses pharmacy inventory → gets medicineId (Guid) and pharmacyId (string)
2. Customer builds cart and taps "Review / Checkout"
   → POST /api/order/validate-cart  (post the local cart array)
   → reconcile fresh prices/stock, show deliveryFee + fees, surface any priceChanged / out-of-stock items
3. Customer confirms and taps "Place Order"
   → POST /api/order/place
   → on success: show order summary, store order.id
4. Customer can view order status
   → GET /api/order?id=<orderId>
5. Customer can cancel if status is Recorded or Packaged
   → PATCH /api/order/cancel/<orderId>
6. Pharmacy sees incoming orders
   → GET /api/order/myOrders
7. Pharmacy advances status step by step
   → PATCH /api/order/update-status  { orderId, nextStatus }
```
