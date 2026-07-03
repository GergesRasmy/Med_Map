namespace Med_Map.Repositories.Account
{
    public interface IOtpRepository
    {
        Task InsertAsync(OtpCode otp);

        // Tracked lookup used by OtpService to verify a code and record attempts against the same row
        Task<OtpCode?> GetActiveSessionAsync(Guid sessionId, OtpPurpose purpose);

        // Most recent OTP for a user/purpose, used to enforce the resend cooldown
        Task<OtpCode?> GetLatestAsync(string userId, OtpPurpose purpose);

        Task UpdateAsync(OtpCode otp);
    }
}
