namespace Med_Map.Repositories.OrderRepos
{
    public interface IOrderRepository
    {
        Task InsertAsync(Orders order);
        Task<List<Orders>?> GetAllOrdersAsync(string id, string role);
        Task<Orders?> GetOrderByIdAsync(string orderId);
        Task<bool> CancelOrder(string orderId);
        Task SaveChangesAsync();


    }
}
