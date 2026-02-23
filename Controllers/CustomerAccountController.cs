using Med_Map.DTO;
using Med_Map.Repositories;
using Med_Map.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Med_Map.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerAccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration config;
        private readonly IOtpRepository otpRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly IEmailService emailService;

        public CustomerAccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config
                                         , IOtpRepository otpRepository,ISessionRepository sessionRepository,IEmailService emailService )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.config = config;
            this.otpRepository = otpRepository;
            this.sessionRepository = sessionRepository;
            this.emailService = emailService;
        }

        [HttpPost("Register")]           //api/CustomerAccount/Register
        public async Task<IActionResult> Register(CustomerRegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);                 // Check if the model state is valid
            // Check if the user has accepted the terms and conditions
            if (!model.TermConditions) return BadRequest("You must accept the terms and conditions to register.");
           
            // Check for existing Phonenumber
            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == model.Phone))
                return BadRequest("Phone number is already in use.");

            ApplicationUser appUser = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.Phone,
                IsActive = false // User is inactive until OTP is verified
            };

            IdentityResult result = await userManager.CreateAsync(appUser, model.PasswordHash);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, "Customer");  // Assign the "Customer" role to the newly created user

                // --- OTP Logic ---
                var otpCode = new Random().Next(100000, 999999).ToString();
                var otpSessionId = Guid.NewGuid();

                var otpRecord = new OtpCode
                {
                    UserId = appUser.Id,
                    Code = otpCode,
                    SessionId = otpSessionId,
                    ExpiresAt = DateTime.Now.AddMinutes(5),
                    IsUsed = false
                };

                await otpRepository.InsertAsync(otpRecord);

                try
                {
                    string subject = "Med-Map Verification Code";
                    string body = $@"
                    <div style='font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 20px;'>
                    <h2 style='color: #2d89ef;'>Welcome to Med-Map!</h2>
                    <p>Thank you for registering. Please use the following code to verify your account:</p>
                    <div style='font-size: 24px; font-weight: bold; color: #333; letter-spacing: 5px; padding: 10px; background: #f4f4f4; text-align: center;'>
                        {otpCode}
                    </div>
                    <p>This code will expire in 5 minutes.</p>
                    </div>";

                    await emailService.SendEmailAsync(appUser.Email, subject, body);
                }
                catch (Exception ex)
                {
                    // Graceful failure: User exists, but email didn't send.
                    return Ok(new
                    {
                        message = "User created, but email failed to send. Please request a new OTP.",
                        sessionId = otpSessionId,
                        emailError = true
                    });
                }
                // Return SessionId so frontend knows what to verify
                return Ok(new
                {
                    message = "Registration successful. Verify your email.",
                    sessionId = otpSessionId
                });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("VerifyOTP")]           //api/CustomerAccount/verifyotp
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOtpDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Check if the OTP exists, matches, hasn't been used, and isn't expired
            var otpRecord = await otpRepository.FindValidOtpAsync(model.SessionId, model.Code);

            if (otpRecord == null)
                return BadRequest("Invalid or expired OTP code.");

            //Mark the OTP as used immediately to prevent replay attacks
            otpRecord.IsUsed = true;
            await otpRepository.UpdateAsync(otpRecord);

            //Find the user associated with this OTP
            var user = await userManager.FindByIdAsync(otpRecord.UserId);
            if (user == null)
                return NotFound("User no longer exists.");

            //Activate the user account
            user.EmailConfirmed = true;
            user.IsActive = true;
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return BadRequest("Failed to activate user account.");

            //Create a Session record in the database
            Guid sessionId = Guid.NewGuid();
            string jwtId = Guid.NewGuid().ToString();
            DateTime expirationTime = DateTime.UtcNow.AddHours(1);

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

            var token = GenerateToken(user, claims,jwtId);

            return Ok(new
            {
                token = token,
                expiration = expirationTime,
                message = "Account verified and logged in successfully."
            });
        }

        [HttpPost("RequestNewOtp")]      //api/CustomerAccount/requestnewotp
        public async Task<IActionResult> RequestNewOtp([FromBody] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User not found.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            var otpSessionId = Guid.NewGuid();

            await otpRepository.InsertAsync(new OtpCode
            {
                UserId = user.Id,
                Code = otpCode,
                SessionId = otpSessionId,
                ExpiresAt = DateTime.Now.AddMinutes(5),
                IsUsed = false
            });
            try
            {
                string subject = "Med-Map Verification Code";
                string body = $@"
                    <div style='font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 20px;'>
                    <h2 style='color: #2d89ef;'>Welcome to Med-Map!</h2>
                    <p>Thank you for registering. Please use the following code to verify your account:</p>
                    <div style='font-size: 24px; font-weight: bold; color: #333; letter-spacing: 5px; padding: 10px; background: #f4f4f4; text-align: center;'>
                        {otpCode}
                    </div>
                    <p>This code will expire in 5 minutes.</p>
                    </div>";

                await emailService.SendEmailAsync(user.Email, subject, body);
                return Ok(new { sessionId = otpSessionId, message = "New code sent!" });
            }
            catch (Exception ex)
            {
                // Graceful failure: User exists, but email didn't send.
                return Ok(new
                {
                    message = "Email failed to send. Please request a new OTP.",
                    sessionId = otpSessionId,
                    emailError = true
                });
            }
        }

        [HttpPost("Login")]             //api/CustomerAccount/login
        public async Task<IActionResult> LoginAsync(CustomerLoginDTO userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dbUser = await userManager.FindByEmailAsync(userDto.Email);
            if (dbUser == null || !await userManager.CheckPasswordAsync(dbUser, userDto.Password))
                return BadRequest("Invalid email or password.");         // If the user is not found or the password is incorrect, return an error message

            //User must be verified
            if (!dbUser.EmailConfirmed)
                return BadRequest("Email not verified. Please verify your OTP.");

            // Create a Session Record for auditing
            Guid sessionId = Guid.NewGuid();
            string jwtId = Guid.NewGuid().ToString();
            DateTime expiration = DateTime.UtcNow.AddHours(1);

            await sessionRepository.InsertAsync(new UserSession
            {
                Id = sessionId,
                UserId = dbUser.Id,
                JwtId = jwtId,
                IsActive = true,
                ExpiresAt = expiration
            });

            // Generate token using the helper
            var roles = await userManager.GetRolesAsync(dbUser);
            var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            claims.Add(new Claim("sid", sessionId.ToString()));

            return Ok(new
            {
                token = GenerateToken(dbUser, claims,jwtId),
                expiration = expiration
            });
        }

        // Helper method to generate JWT token
        private string GenerateToken(ApplicationUser user, List<Claim> extraClaims, string jwtId)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId)
            };

            authClaims.AddRange(extraClaims);

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecurityKey"]));

            var token = new JwtSecurityToken(
                issuer: config["JWT:IssuerIP"],
                audience: config["JWT:AudienceIP"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
