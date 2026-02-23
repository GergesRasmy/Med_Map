namespace Med_Map.Repositories
{
    public interface ISessionRepository
    {
        Task InsertAsync(UserSession session);
        Task<UserSession> FindByIdAsync(string sessionId);
        Task UpdateAsync(UserSession session);
    }
}
