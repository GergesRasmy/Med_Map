using Microsoft.EntityFrameworkCore.Storage;

namespace Med_Map.Repositories.OrderRepos
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Mm_Context context;
        private IDbContextTransaction? transaction;

        public UnitOfWork(Mm_Context context)
        {
            this.context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            transaction = await context.Database.BeginTransactionAsync();
            return transaction;
        }

        public async Task CommitAsync()
        {
            if (transaction != null)
                await transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (transaction != null)
                await transaction.RollbackAsync();
        }
    }

}
