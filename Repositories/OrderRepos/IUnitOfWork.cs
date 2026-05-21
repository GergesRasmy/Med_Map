using Microsoft.EntityFrameworkCore.Storage;

namespace Med_Map.Repositories.OrderRepos
{
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
