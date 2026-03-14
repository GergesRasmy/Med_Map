namespace Med_Map.Repositories.Account
{
    public interface ISessionRepository
    {
        Task InsertAsync(UserSession session);
        Task<UserSession?> FindByIdAsync(Guid sessionId);
        Task UpdateAsync(UserSession session);
    }
}
