using Med_Map.DTO.NotificationDTOs;

namespace Med_Map.Hubs;

public interface INotificationClient
{
    Task OrderPlaced(OrderPlacedPayload payload);
    Task OrderStatusChanged(OrderStatusChangedPayload payload);
    Task OrderCancelled(OrderCancelledPayload payload);
    Task WalletDeposited(WalletDepositedPayload payload);
    Task InventoryStockChanged(InventoryStockChangedPayload payload);
    Task WithdrawalCompleted(WithdrawalCompletedPayload payload);
    Task WithdrawalCancelled(WithdrawalCancelledPayload payload);
}
