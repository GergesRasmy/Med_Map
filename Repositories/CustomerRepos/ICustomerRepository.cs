namespace Med_Map.Repositories.CustomerRepos
{
    public interface ICustomerRepository
    {
        Task InsertAsync(Customer customer);
        Task<Customer?> GetByIdAsync(string id, bool asNoTracking);
        Task SaveChangesAsync();

    }
}
