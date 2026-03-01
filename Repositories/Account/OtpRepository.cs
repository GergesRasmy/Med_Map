using Med_Map.Models.customer;

namespace Med_Map.Repositories.Account
{
    public class OtpRepository : IOtpRepository
    {
        private readonly Mm_Context _context;

        public OtpRepository(Mm_Context context)
        {
            _context = context;
        }

        // Corresponds to: OtpCodes.Insert(otpRecord) in pseudo-code
        public async Task InsertAsync(OtpCode otp)
        {
            await _context.OtpCodes.AddAsync(otp);
            await _context.SaveChangesAsync();
        }

        // Corresponds to the Find logic in pseudo-code VerifyOtp
        public async Task<OtpCode?> FindValidOtpAsync(Guid sessionId, string code)
        {
            return await _context.OtpCodes.FirstOrDefaultAsync(o => o.SessionId == sessionId
                               && o.Code == code
                               && !o.IsUsed
                               && o.ExpiresAt > DateTime.Now);
        }

        // Corresponds to: OtpCodes.Update(otpRecord)
        public async Task UpdateAsync(OtpCode otp)
        {
            _context.OtpCodes.Update(otp);
            await _context.SaveChangesAsync();
        }
    }
}
