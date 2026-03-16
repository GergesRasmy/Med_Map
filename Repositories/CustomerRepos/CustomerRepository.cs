using Microsoft.AspNetCore.Http.HttpResults;

namespace Med_Map.Repositories.CustomerRepos
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly Mm_Context _context;

        public CustomerRepository(Mm_Context _Context)
        {
            _context = _Context;
        }
        public async Task InsertAsync (Customer customer)
        { 
            await _context.Customer.AddAsync(customer);
            await SaveChangesAsync();
        }

        public async Task<Customer?> GetByIdAsync(string id, bool asNoTracking = false)
        {
            var query = _context.Customer.Include(c => c.User)
                                         .Where(c => c.ApplicationUserId == id);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
