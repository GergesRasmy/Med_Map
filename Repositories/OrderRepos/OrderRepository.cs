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
            IQueryable<Orders> query = _context.Orders.Include(o => o.OrderItems) 
                                       .ThenInclude(oi => oi.Medicine);
            if (role == "Pharmacy")
                return await query.Where(o => o.PharmacyId == id).ToListAsync();

            if (role == "Customer")
                return await query.Where(o => o.CustomerId == id).ToListAsync();

            return new List<Orders>();
        }
        public async Task<Orders?> GetOrderByIdAsync(string orderId)
        {
            if (!Guid.TryParse(orderId, out var guid)) return null;

            return await _context.Orders.Include(o => o.OrderItems)
                                        .ThenInclude(oi => oi.Medicine)
                                        .FirstOrDefaultAsync(o => o.Id == guid);
        }
        public async Task<bool> CancelOrder(string orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order != null && (order.Status == StatusList.Pending || order.Status == StatusList.Preparing))
            {
                order.Status = StatusList.Cancelled;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
