using Med_Map.Models.AI;

namespace Med_Map.Repositories.Account
{
    public class SessionRepository : ISessionRepository
    {
        private readonly Mm_Context _context;
        public SessionRepository(Mm_Context context) => _context = context;

        public async Task InsertAsync(UserSession session)
        {
            await _context.UserSession.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        public async Task<UserSession?> FindByIdAsync(string sessionId)
        {
            return await _context.UserSession.FindAsync(sessionId);
        }

        public async Task UpdateAsync(UserSession session)
        {
            _context.UserSession.Update(session);
            await _context.SaveChangesAsync();
        }
    }
}
