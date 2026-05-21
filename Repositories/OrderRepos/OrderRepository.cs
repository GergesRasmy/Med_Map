namespace Med_Map.Repositories.OrderRepos
{
    public class OrderRepository: IOrderRepository
    {
        private readonly Mm_Context _context;

        public OrderRepository(Mm_Context context)
        {
            _context = context;
        }

        public async Task InsertAsync(Orders order)
        {
            _context.Orders.Add(order);
            await SaveChangesAsync();
        }

        public async Task<List<Orders>?> GetAllOrdersAsync(string id,string role)
        {
            IQueryable<Orders> query = _context.Orders.AsNoTracking().Include(o => o.OrderItems) 
                                       .ThenInclude(oi => oi.Medicine);
            if (role == "Pharmacy")
                return await query.Where(o => o.PharmacyProfileId == Guid.Parse(id)).ToListAsync();

            if (role == "Customer")
                return await query.Where(o => o.CustomerId == id).ToListAsync();

            return new List<Orders>();
        }
        public async Task<Orders?> GetOrderByIdAsync(string orderId)
        {
            if (!Guid.TryParse(orderId, out var guid)) return null;

            return await _context.Orders.AsNoTracking().Include(o => o.OrderItems)
                                        .ThenInclude(oi => oi.Medicine)
                                        .FirstOrDefaultAsync(o => o.Id == guid);
        }
        public async Task UpdateAsync(Orders order)
        {
            _context.Orders.Update(order);
            await SaveChangesAsync();
        }
        public async Task<bool> UpdateStatusAsync(Guid orderId, StatusList nextStatus)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return false;

                // If moving TO Canceled, we must restore the stock
                if (nextStatus == StatusList.Canceled && order.Status != StatusList.Canceled)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var inventory = await _context.PharmacyInventory
                            .FirstOrDefaultAsync(pi => pi.PharmacyProfileId == order.PharmacyProfileId
                                                    && pi.MedicineId == item.MedicineId);
                        if (inventory != null)
                        {
                            inventory.StockQuantity += item.Quantity;
                        }
                    }
                }

                order.Status = nextStatus;

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
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == Guid.Parse(orderId) && o.CustomerId == userId);

                if (order == null)
                    return false;

                // Block cancellation on terminal or in-progress states
                var nonCancellableStatuses = new[]
                {
                    StatusList.Delivered,
                    StatusList.Canceled,
                    StatusList.OutForDelivery,  // already on the way, too late
                    StatusList.ReadyForPickup   // already prepared, too late
                };

                if (nonCancellableStatuses.Contains(order.Status))
                    return false;

                // Restore stock
                foreach (var item in order.OrderItems)
                {
                    var inventory = await _context.PharmacyInventory
                        .FirstOrDefaultAsync(pi => pi.PharmacyProfileId == order.PharmacyProfileId
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
