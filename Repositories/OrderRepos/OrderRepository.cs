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
        // Inside your OrderRepository.cs
        public async Task<bool> CancelOrder(string orderId, string userId)
        {
            // You likely have access to the DbContext here, which is exactly where it belongs!
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == Guid.Parse(orderId) && o.CustomerId == userId);

                // Validate that order exists and is in a cancellable state
                if (order == null || order.Status != StatusList.Pending)
                    return false;

                // Restore stock
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

                order.Status = StatusList.Cancelled;

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
