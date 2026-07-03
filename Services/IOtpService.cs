namespace Med_Map.Services
{
    public enum OtpVerificationStatus
    {
        Success,
        InvalidOrExpired,
        TooManyAttempts
    }

    public class OtpGenerationResult
    {
        public bool Success { get; set; }
        public int RetryAfterSeconds { get; set; }
        public OtpResponseDataDTO? Data { get; set; }
    }

    public class OtpVerificationResult
    {
        public OtpVerificationStatus Status { get; set; }
        public OtpCode? Otp { get; set; }
    }

    public interface IOtpService
    {
        Task<OtpGenerationResult> GenerateAndSendOtpAsync(ApplicationUser user, OtpPurpose purpose);
        Task<OtpVerificationResult> VerifyOtpAsync(Guid sessionId, string code, OtpPurpose purpose);
    }
}
