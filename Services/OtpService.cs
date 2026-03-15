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
        public async Task<OtpResponseDataDTO> GenerateAndSendOtpAsync(ApplicationUser user)
        {
            var otpCode = new Random().Next(100000, 999999).ToString();
            var otpSessionId = Guid.NewGuid();
            var expirationTime = DateTime.UtcNow.AddMinutes(Constant.OtpExpirationTime);

            await _otpRepository.InsertAsync(new OtpCode
            {
                UserId = user.Id,
                Code = otpCode,
                SessionId = otpSessionId,
                ExpiresAt = expirationTime,
                IsUsed = false
            });
            var otpData = new OtpResponseDataDTO
            {
                sessionId = otpSessionId,
                expiration = expirationTime
            };

            string subject = "Med-Map Verification Code";
            string body = $"<h2>Welcome to Med-Map!{user.UserName}</h2><p>Your code is: <b>{otpCode}</b></p>";
            Console.WriteLine($"{subject} \n {body}");
            //await _emailService.SendEmailAsync(user.Email, subject, body);

            return otpData;
        }
        

    }
}
