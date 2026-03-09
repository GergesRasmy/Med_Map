
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

        public async Task<OtpCode?> FindValidOtpAsync(Guid sessionId, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || sessionId == Guid.Empty)
            {
                return null;
            }
            return await _context.OtpCodes.AsNoTracking().FirstOrDefaultAsync(o => o.SessionId == sessionId
                               && o.Code == code
                               && !o.IsUsed
                               && o.ExpiresAt > DateTime.UtcNow);

        }

        public async Task UpdateAsync(OtpCode otp)
        {
            _context.OtpCodes.Update(otp);
            await _context.SaveChangesAsync();
        }
    }
}
