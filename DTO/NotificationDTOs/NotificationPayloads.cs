namespace Med_Map.DTO.NotificationDTOs;

public record OrderPlacedPayload(
    Guid OrderId,
    string CustomerId,
    decimal TotalAmount,
    int ItemCount,
    string FulfillmentType,
    DateTime CreatedAt
);

public record OrderStatusChangedPayload(
    Guid OrderId,
    string NewStatus,
    string FulfillmentType
);

public record OrderCancelledPayload(
    Guid OrderId,
    string CustomerId
);

public record WalletDepositedPayload(
    Guid WalletId,
    decimal Amount,
    string Currency,
    Guid OrderId
);

public record StockChange(
    Guid MedicineId,
    string MedicineName,
    int NewStockQuantity
);

public record InventoryStockChangedPayload(
    List<StockChange> Changes
);

public record WithdrawalCompletedPayload(
    Guid TransactionId,
    Guid WalletId
);

public record WithdrawalCancelledPayload(
    Guid TransactionId,
    Guid WalletId,
    decimal RefundedAmount,
    string Currency
);
