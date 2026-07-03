namespace Med_Map.Services
{
    public class OtpService: IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;

        public OtpService(IOtpRepository otpRepository, IEmailService emailService)
        {
            _otpRepository = otpRepository;
            _emailService = emailService;
        }

        public async Task<OtpGenerationResult> GenerateAndSendOtpAsync(ApplicationUser user, OtpPurpose purpose)
        {
            //Throttle how often a new OTP can be requested for the same user/purpose
            var latest = await _otpRepository.GetLatestAsync(user.Id, purpose);
            if (latest != null)
            {
                var cooldownEnd = latest.CreatedAt.AddSeconds(Constant.OtpResendCooldownSeconds);
                if (cooldownEnd > DateTime.UtcNow)
                {
                    return new OtpGenerationResult
                    {
                        Success = false,
                        RetryAfterSeconds = (int)Math.Ceiling((cooldownEnd - DateTime.UtcNow).TotalSeconds)
                    };
                }
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            var otpSessionId = Guid.NewGuid();
            var expirationTime = DateTime.UtcNow.AddMinutes(Constant.OtpExpirationTime);

            await _otpRepository.InsertAsync(new OtpCode
            {
                UserId = user.Id,
                Code = otpCode,
                SessionId = otpSessionId,
                ExpiresAt = expirationTime,
                Purpose = purpose,
                IsUsed = false
            });
            var otpData = new OtpResponseDataDTO
            {
                sessionId = otpSessionId,
                expiration = expirationTime
            };

            string subject = purpose == OtpPurpose.PasswordReset ? "Med-Map Password Reset Code" : "Med-Map Verification Code";
            string body = $"<h2>Welcome to Med-Map!{user.UserName}</h2><p>Your code is: <b>{otpCode}</b></p>";
            Console.WriteLine($"{subject} \n {body}");
            //await _emailService.SendEmailAsync(user.Email, subject, body);

            return new OtpGenerationResult { Success = true, Data = otpData };
        }

        public async Task<OtpVerificationResult> VerifyOtpAsync(Guid sessionId, string code, OtpPurpose purpose)
        {
            var activeOtp = await _otpRepository.GetActiveSessionAsync(sessionId, purpose);
            if (activeOtp == null)
                return new OtpVerificationResult { Status = OtpVerificationStatus.InvalidOrExpired };

            if (activeOtp.AttemptCount >= Constant.OtpMaxAttempts)
                return new OtpVerificationResult { Status = OtpVerificationStatus.TooManyAttempts };

            if (string.IsNullOrWhiteSpace(code) || activeOtp.Code != code)
            {
                activeOtp.AttemptCount++;
                if (activeOtp.AttemptCount >= Constant.OtpMaxAttempts)
                    activeOtp.IsUsed = true; // lock the session out, caller must request a new OTP
                await _otpRepository.UpdateAsync(activeOtp);

                return new OtpVerificationResult
                {
                    Status = activeOtp.AttemptCount >= Constant.OtpMaxAttempts
                        ? OtpVerificationStatus.TooManyAttempts
                        : OtpVerificationStatus.InvalidOrExpired
                };
            }

            //Mark the OTP as used immediately to prevent replay attacks
            activeOtp.IsUsed = true;
            await _otpRepository.UpdateAsync(activeOtp);

            return new OtpVerificationResult { Status = OtpVerificationStatus.Success, Otp = activeOtp };
        }
    }
}
