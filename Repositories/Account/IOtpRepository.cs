using Med_Map.Models.customer;

namespace Med_Map.Repositories.Account
{
    public interface IOtpRepository
    {
        Task InsertAsync(OtpCode otp);
        Task<OtpCode?> FindValidOtpAsync(Guid sessionId, string code);
        Task UpdateAsync(OtpCode otp);
    }
}
