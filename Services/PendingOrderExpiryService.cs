namespace Med_Map.Services
{
    public class PendingOrderExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<PendingOrderExpiryService> logger;
        // IHubContext is singleton — safe to inject directly into a BackgroundService
        private readonly IHubContext<NotificationHub, INotificationClient> hub;

        public PendingOrderExpiryService(IServiceScopeFactory scopeFactory, ILogger<PendingOrderExpiryService> logger,
                                         IHubContext<NotificationHub, INotificationClient> hub)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
            this.hub = hub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExpireStaleOrdersAsync();
                await Task.Delay(TimeSpan.FromMinutes(Constant.PendingOrderExpiryMinutes), stoppingToken);
            }
        }

        private async Task ExpireStaleOrdersAsync()
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Mm_Context>();

            var cutoff = DateTime.UtcNow.AddMinutes(-Constant.PendingOrderExpiryMinutes);

            var expiredOrders = await context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Status == StatusList.Pending
                         && o.PaymentType == PaymentOptions.Online
                         && o.CreatedAt < cutoff)
                .ToListAsync();

            if (expiredOrders.Count == 0) return;

            foreach (var order in expiredOrders)
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                bool committed = false;
                try
                {
                    foreach (var item in order.OrderItems)
                    {
                        var inventory = await context.PharmacyInventory
                            .FirstOrDefaultAsync(pi => pi.PharmacyUserId == order.PharmacyUserId
                                                    && pi.MedicineId == item.MedicineId);
                        if (inventory != null)
                            inventory.StockQuantity += item.Quantity;
                    }

                    order.Status = StatusList.Canceled;
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    committed = true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Failed to expire pending order {OrderId}.", order.Id);
                }

                // Notifications are outside the try/catch so a hub failure can't
                // trigger a rollback on an already-committed transaction.
                if (committed)
                {
                    logger.LogInformation("Expired pending order {OrderId} cancelled and stock restored.", order.Id);

                    // Notify the customer: their unpaid order was auto-cancelled
                    await hub.Clients.User(order.CustomerId).OrderStatusChanged(
                        new OrderStatusChangedPayload(order.Id, StatusList.Canceled.ToString(), order.FulfillmentType.ToString()));

                    // Notify the pharmacy: a pending order was cancelled and stock was restored
                    await hub.Clients.User(order.PharmacyUserId).OrderCancelled(
                        new OrderCancelledPayload(order.Id, order.CustomerId));
                }
            }
        }
    }
}
