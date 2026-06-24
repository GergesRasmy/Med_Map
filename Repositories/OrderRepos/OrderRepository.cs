namespace Med_Map.Repositories.OrderRepos
{
    public class OrderRepository: IOrderRepository
    {
        private readonly Mm_Context _context;

        public OrderRepository(Mm_Context context)
        {
            _context = context;
        }

        public void Insert(Orders order)
        {
            _context.Orders.Add(order);
        }

        public async Task<(List<Orders> items, int totalCount)> GetAllOrdersAsync(string id, string role, int page, int pageSize, StatusList? status = null)
        {
            IQueryable<Orders> query = _context.Orders.AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Medicine)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Service);

            query = role switch
            {
                "Pharmacy" => query.Where(o => o.PharmacyUserId == id),
                "Customer" => query.Where(o => o.CustomerId == id),
                _ => query.Where(_ => false)
            };

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            query = query.OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<OrderStatsDTO> GetOrderStatsAsync(string id, string role)
        {
            IQueryable<Orders> query = _context.Orders.AsNoTracking();

            query = role switch
            {
                "Pharmacy" => query.Where(o => o.PharmacyUserId == id),
                "Customer" => query.Where(o => o.CustomerId == id),
                _ => query.Where(_ => false)
            };

            var today = DateTime.UtcNow.Date;

            return new OrderStatsDTO
            {
                newOrders      = await query.CountAsync(o => o.Status == StatusList.Recorded),
                preparing      = await query.CountAsync(o => o.Status == StatusList.Packaged),
                outForDelivery = await query.CountAsync(o => o.Status == StatusList.OutForDelivery),
                completedToday = await query.CountAsync(o => o.Status == StatusList.Delivered && o.DeliveredAt >= today)
            };
        }

        public async Task<Orders?> GetOrderByIdAsync(string orderId)
        {
            if (!Guid.TryParse(orderId, out var guid)) return null;

            return await _context.Orders.AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Medicine)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Service)
                .FirstOrDefaultAsync(o => o.Id == guid);
        }

        public async Task UpdateAsync(Orders order)
        {
            _context.Orders.Update(order);
            await SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(Guid orderId, StatusList nextStatus, DateTime? deliveredAt = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return false;

                // Restore stock only for medicine items when cancelling
                if (nextStatus == StatusList.Canceled && order.Status != StatusList.Canceled)
                {
                    foreach (var item in order.OrderItems.Where(i => i.MedicineId.HasValue))
                    {
                        var inventory = await _context.PharmacyInventory
                            .FirstOrDefaultAsync(pi => pi.PharmacyUserId == order.PharmacyUserId
                                                    && pi.MedicineId == item.MedicineId);
                        if (inventory != null)
                            inventory.StockQuantity += item.Quantity;
                    }
                }

                order.Status = nextStatus;

                if (deliveredAt.HasValue)
                    order.DeliveredAt = deliveredAt.Value;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> CancelOrder(string orderId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!Guid.TryParse(orderId, out var guid)) return false;

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == guid && o.CustomerId == userId);

                if (order == null)
                    return false;

                var nonCancellableStatuses = new[]
                {
                    StatusList.Delivered,
                    StatusList.Canceled,
                    StatusList.OutForDelivery,
                    StatusList.ReadyForPickup
                };

                if (nonCancellableStatuses.Contains(order.Status))
                    return false;

                // Restore stock only for medicine items
                foreach (var item in order.OrderItems.Where(i => i.MedicineId.HasValue))
                {
                    var inventory = await _context.PharmacyInventory
                        .FirstOrDefaultAsync(pi => pi.PharmacyUserId == order.PharmacyUserId
                                                && pi.MedicineId == item.MedicineId);
                    if (inventory != null)
                        inventory.StockQuantity += item.Quantity;
                }

                order.Status = StatusList.Canceled;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
