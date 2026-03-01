using Med_Map.Models.AI;

namespace Med_Map.Repositories.Account
{
    public interface ISessionRepository
    {
        Task InsertAsync(UserSession session);
        Task<UserSession> FindByIdAsync(string sessionId);
        Task UpdateAsync(UserSession session);
    }
}
