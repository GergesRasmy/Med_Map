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
        private readonly IEmailService emailService;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config
                                            , IOtpRepository otpRepository, IOtpService otpService, ISessionRepository sessionRepository
                                            , IEmailService emailService, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.config = config;
            this.otpRepository = otpRepository;
            this.otpService = otpService;
            this.sessionRepository = sessionRepository;
            this.emailService = emailService;
            this.logger = logger;
        }
        #endregion

        //TODO testing 
        [HttpPost("register")]              //api/account/register
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (await userManager.Users.AnyAsync(u => u.NormalizedEmail == model.email.ToUpper()))
                return ErrorResponse("Email already in use.", ErrorCodes.ValidationError);
            if (await userManager.Users.AnyAsync(u => u.NormalizedUserName == model.userName.ToUpper()))
                return ErrorResponse("user Name already in use.", ErrorCodes.ValidationError);
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = model.userName,
                Email = model.email,
                EmailConfirmed = false
            };
            var result = await userManager.CreateAsync(user, model.password);
            if (!result.Succeeded)
                return ErrorResponse("user creation failed", ErrorCodes.ProfileCreationFailed);
            try
            {
                if (model.role == RoleConstants.Names.Customer || model.role == RoleConstants.Names.Pharmacy)
                    await userManager.AddToRoleAsync(user, model.role);
                else return ErrorResponse("Role is Invalid", ErrorCodes.InvalidInput);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Role assignment failed for user {UserId}", user.Id);
                var deleteResult = await userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                    logger.LogError("Failed to rollback user {UserId} after role assignment failure", user.Id);
                return ErrorResponse("Registration failed during profile setup.", ErrorCodes.ProfileCreationFailed);
            }
            try
            {
                var otpRecord = await otpService.GenerateAndSendOtpAsync(user);
                return SuccessResponse(otpRecord,"User created, please verify using the OTP.", SuccessCodes.RegistrationPending);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OTP send failed for user {UserId}", user.Id);
                return ErrorResponse("User created but verification email failed to send. Please request a new OTP.", ErrorCodes.OtpSendFailed);
            }
        }

        [HttpPost("verifyOtp")]           //api/Account/verifyotp
        public async Task<IActionResult> verifyOtp([FromBody] VerifyOtpDTO model)
        {
            //Check if the OTP exists, matches, hasn't been used, and isn't expired
            var otpRecord = await otpRepository.FindValidOtpAsync(model.sessionId, model.code);
            if (otpRecord == null) 
                return ErrorResponse("Invalid or expired OTP code.", ErrorCodes.InvalidOtp);

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

            //Create session and generate token
            var AuthData = await CreateUserSessionAndTokenAsync(user);
            return SuccessResponse(AuthData,"Account verified successfully.",SuccessCodes.AccountVerified);
        }

        [HttpPost("requestNewOtp")]           //api/Account/requestnewotp
        public async Task<IActionResult> requestNewOtp([FromBody] ResendOtpDto model)
        {

            //Find the user by email
            var user = await userManager.FindByEmailAsync(model.email);
            if (user == null) 
                return ErrorResponse("User Not Found",ErrorCodes.UserNotFound);

            //Check if the user is already verified
            if(user.EmailConfirmed) 
                return ErrorResponse("Email already verified, Please login to your account.", ErrorCodes.Emailconfirmed);

            //Generate and send new OTP
            try
            {
                var otpData = await otpService.GenerateAndSendOtpAsync(user);
                return SuccessResponse(otpData, "Verification code sent successfully.", SuccessCodes.RegistrationPending);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OTP send failed for user {UserId}", user.Id);
                return ErrorResponse("Verification process failed.", ErrorCodes.OtpSendFailed);
            }
        }

        [HttpPost("login")]           //api/Account/login
        public async Task<IActionResult> login([FromBody]LoginDTO userDto)
        {
            //Find the user by email and verify the password
            var user = await userManager.FindByEmailAsync(userDto.email);
            if (user == null || !await userManager.CheckPasswordAsync(user, userDto.password))
                return ErrorResponse("Invalid Email or Password",ErrorCodes.InvalidCredentials); 

            if (!user.EmailConfirmed)         //User must be verified
                return ErrorResponse("Email not verified, Please verify your Account.", ErrorCodes.EmailUnconfirmed);

            //Create session and generate token
            var AuthData = await CreateUserSessionAndTokenAsync(user);
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

        // Helper method to create a session And return auth response data
        private async Task<AuthResponseDataDTO> CreateUserSessionAndTokenAsync(ApplicationUser user)
        {
            //Create and store session in database
            var sessionId = Guid.NewGuid();
            var jwtId = Guid.NewGuid().ToString();
            var expirationTime = DateTime.UtcNow.AddHours(Constant.tokenExpirationTime);

            var session = new UserSession
            {
                Id = sessionId,
                UserId = user.Id,
                JwtId = jwtId,
                IsActive = true,
                ExpiresAt = expirationTime
            };
            await sessionRepository.InsertAsync(session);

            //Build Claims
            var roles = await userManager.GetRolesAsync(user);
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            claims.Add(new Claim("sid", sessionId.ToString()));

            //Return the Auth Data DTO
            return new AuthResponseDataDTO
            {
                token = GenerateToken(user, claims, jwtId, expirationTime),
                expiration = expirationTime,
                role = roles.FirstOrDefault() ?? string.Empty
            };
        }
    }
}

