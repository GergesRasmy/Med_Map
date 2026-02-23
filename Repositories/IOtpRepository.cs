namespace Med_Map.Repositories
{
    public interface IOtpRepository
    {
        Task InsertAsync(OtpCode otp);
        Task<OtpCode?> FindValidOtpAsync(Guid sessionId, string code);
        Task UpdateAsync(OtpCode otp);
    }
}
