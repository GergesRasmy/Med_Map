using Microsoft.AspNetCore.Http.HttpResults;

namespace Med_Map.Repositories.Account
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
            await _context.SaveChangesAsync();
        }
        
        public async Task<Customer?> GetByIdAsync(string id)
        {
            return await _context.Customer.FirstAsync(c => c.ApplicationUserId == id);
        }
        
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
