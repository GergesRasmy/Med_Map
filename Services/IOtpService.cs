namespace Med_Map.Services
{
    public interface IOtpService
    {
        Task<OtpResponseDataDTO> GenerateAndSendOtpAsync(ApplicationUser user);
    }
}
