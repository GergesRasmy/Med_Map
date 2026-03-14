using Med_Map.DTO.CustomerDTOs;
using Med_Map.DTO.PharmacyDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Med_Map.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ResponceBaseController
    {
        #region ctor
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration config;
        private readonly IOtpRepository otpRepository;
        private readonly IOtpService otpService;
        private readonly ISessionRepository sessionRepository;
        private readonly IFileService fileService;
        private readonly IEmailService emailService;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config
                                            , IOtpRepository otpRepository, IOtpService otpService, ISessionRepository sessionRepository
                                            , IFileService fileService, IEmailService emailService)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.config = config;
            this.otpRepository = otpRepository;
            this.otpService = otpService;
            this.sessionRepository = sessionRepository;
            this.fileService = fileService;
            this.emailService = emailService;
        }
        #endregion

        [HttpPost("verifyOtp")]           //api/Account/verifyotp
        public async Task<IActionResult> verifyOtp([FromBody] VerifyOtpDTO model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return ErrorResponse("Validation failed", ErrorCodes.ValidationError, errors);
            }    // Check if the model state is valid

            //Check if the OTP exists, matches, hasn't been used, and isn't expired
            var otpRecord = await otpRepository.FindValidOtpAsync(model.sessionId, model.code);

            if (otpRecord == null) return ErrorResponse("Invalid or expired OTP code.", ErrorCodes.InvalidOtp);

            //Mark the OTP as used immediately to prevent replay attacks
            otpRecord.IsUsed = true;
            await otpRepository.UpdateAsync(otpRecord);

            //Find the user associated with this OTP
            var user = await userManager.FindByIdAsync(otpRecord.UserId);
            if (user == null)
                return ErrorResponse("User not found.", ErrorCodes.UserNotFound);

            //Activate the user account
            user.EmailConfirmed = true;
            var updateResult = await userManager.UpdateAsync(user);
            

            if (!updateResult.Succeeded)
                return ErrorResponse("Failed to activate user account.", ErrorCodes.ActivitionFailed);

            //Create a Session record in the database
            Guid sessionId = Guid.NewGuid();
            string jwtId = Guid.NewGuid().ToString();
            DateTime expirationTime = DateTime.UtcNow.AddHours(Constant.tokenExpirationTime);

            var session = new UserSession
            {
                Id = sessionId,
                UserId = user.Id,
                JwtId = jwtId,
                IsActive = true,
                ExpiresAt = expirationTime
            };
            await sessionRepository.InsertAsync(session);

            //Generate the JWT using the helper
            var roles = await userManager.GetRolesAsync(user);
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            claims.Add(new Claim("sid", sessionId.ToString()));

            var AuthData = new AuthResponseDataDTO
            {
                token = GenerateToken(user, claims, jwtId, expirationTime),
                expiration = expirationTime,
                role = roles.FirstOrDefault()
            };
            return SuccessResponse(AuthData,"Account verified successfully.",SuccessCodes.AccountVerified);
        }

        [HttpPost("requestNewOtp")]           //api/Account/requestnewotp
        public async Task<IActionResult> requestNewOtp([FromBody] ResendOtpDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return ErrorResponse("Validation failed", ErrorCodes.ValidationError, errors);
            }
            var user = await userManager.FindByEmailAsync(model.email);
            if (user == null) return ErrorResponse("User Not Found",ErrorCodes.UserNotFound);

            try
            {
                var otpData = await otpService.GenerateAndSendOtpAsync(user);
                return SuccessResponse(otpData, "Verification code sent successfully.", SuccessCodes.RegistrationPending);
            }
            catch (Exception ex)
            {
                return ErrorResponse("Verification process failed.", ErrorCodes.OtpSendFailed, ex.Message);
            }
        }

        [HttpPost("login")]           //api/Account/login
        public async Task<IActionResult> login([FromBody]LoginDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return ErrorResponse("Validation failed", ErrorCodes.ValidationError, errors);
            }      // Check if the model state is valid

            var dbUser = await userManager.FindByEmailAsync(userDto.email);
            if (dbUser == null || !await userManager.CheckPasswordAsync(dbUser, userDto.password))
                return ErrorResponse("Invaild Email or Password",ErrorCodes.InvalidCredentials); 

            if (!dbUser.EmailConfirmed)         //User must be verified
                return ErrorResponse("Email not verified, Please verify your Account.", ErrorCodes.EmailUnconfirmed);

            Guid sessionId = Guid.NewGuid();
            string jwtId = Guid.NewGuid().ToString(); 
            DateTime expirationTime = DateTime.UtcNow.AddHours(Constant.tokenExpirationTime);

            await sessionRepository.InsertAsync(new UserSession
            {
                Id = sessionId,
                UserId = dbUser.Id,
                JwtId = jwtId,
                IsActive = true,
                ExpiresAt = expirationTime
            });

            // Generate token using the helper
            var roles = await userManager.GetRolesAsync(dbUser);
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            claims.Add(new Claim("sid", sessionId.ToString()));
            var AuthData = new AuthResponseDataDTO
            {
                token = GenerateToken(dbUser, claims, jwtId, expirationTime),
                expiration = expirationTime,
                role = roles.FirstOrDefault()
            };
            return SuccessResponse(AuthData,"Login Successful",SuccessCodes.LoginSuccess);
        }

        // Helper method to generate JWT token
        private string GenerateToken(ApplicationUser user, List<Claim> extraClaims, string jwtId, DateTime _expires)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId) // Use the same JTI as the Session record
            };

            authClaims.AddRange(extraClaims);

            var securityKey = config["JWT:SecurityKey"];
            if (string.IsNullOrEmpty(securityKey))
            {
                throw new InvalidOperationException("JWT SecurityKey is missing in appsettings.json. Please check your configuration.");
            }
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));


            var token = new JwtSecurityToken(
                issuer: config["JWT:IssuerIP"],
                audience: config["JWT:AudienceIP"],
                expires: _expires,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

