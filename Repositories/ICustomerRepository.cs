namespace Med_Map.Repositories.Account
{
    public interface ICustomerRepository
    {
        Task InsertAsync(Customer customer);
        Task<Customer?> GetByIdAsync(string id);
        Task SaveChangesAsync();

    }
}
