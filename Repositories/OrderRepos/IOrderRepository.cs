namespace Med_Map.Repositories.OrderRepos
{
    public interface IOrderRepository
    {
        void Insert(Orders order);
        Task<(List<Orders> items, int totalCount)> GetAllOrdersAsync(string id, string role, int page, int pageSize, StatusList? status = null);
        Task<OrderStatsDTO> GetOrderStatsAsync(string id, string role);
        Task<Orders?> GetOrderByIdAsync(string orderId);
        Task<bool> CancelOrder(string orderId, string userId);
        Task SaveChangesAsync();
        Task<bool> UpdateStatusAsync(Guid orderId, StatusList nextStatus, DateTime? deliveredAt);
        Task UpdateAsync(Orders order);
    }
}
