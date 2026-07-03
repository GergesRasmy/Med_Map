
namespace Med_Map.Repositories.Account
{
    public class OtpRepository : IOtpRepository
    {
        private readonly Mm_Context _context;

        public OtpRepository(Mm_Context context)
        {
            _context = context;
        }

        public async Task InsertAsync(OtpCode otp)
        {
            await _context.OtpCodes.AddAsync(otp);
            await _context.SaveChangesAsync();
        }

        public async Task<OtpCode?> GetActiveSessionAsync(Guid sessionId, OtpPurpose purpose)
        {
            if (sessionId == Guid.Empty)
            {
                return null;
            }
            return await _context.OtpCodes.FirstOrDefaultAsync(o => o.SessionId == sessionId
                               && o.Purpose == purpose
                               && !o.IsUsed
                               && o.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<OtpCode?> GetLatestAsync(string userId, OtpPurpose purpose)
        {
            return await _context.OtpCodes.AsNoTracking()
                .Where(o => o.UserId == userId && o.Purpose == purpose)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(OtpCode otp)
        {
            _context.OtpCodes.Update(otp);
            await _context.SaveChangesAsync();
        }
    }
}
