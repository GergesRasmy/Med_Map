using Med_Map.DTO;
using Med_Map.Repositories;
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
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration config;
        private readonly IOtpRepository otpRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly IEmailService emailService;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config
                                            , IOtpRepository otpRepository, ISessionRepository sessionRepository,IPharmacyRepository pharmacyRepository,IEmailService emailService)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.config = config;
            this.otpRepository = otpRepository;
            this.sessionRepository = sessionRepository;
            this.pharmacyRepository = pharmacyRepository;
            this.emailService = emailService;
        }

        [HttpPost("RegisterPharmacy")]           //api/Account/RegisterPharmacy
        public async Task<IActionResult> RegisterPharmacy([FromForm] PharmacyRegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);     // Check if the model state is valid

            if (!model.TermConditions) return BadRequest("Accept terms to proceed.");

            // Check if the user already exists
            if (await userManager.FindByEmailAsync(model.Email) != null) return BadRequest("Email already in use.");

            // Save Images to local folder (wwwroot/uploads)
            string pharmacyImagePath = await SaveFile(model.NationalId, "National_Id");
            string licenseImagePath = await SaveFile(model.LicenseImage, "Pharmacy_License");

            // Create the Identity User
            ApplicationUser appUser = new ApplicationUser
            {
                UserName = model.PharmacyName,
                Email = model.Email,
                PhoneNumber = model.PharmacistPhoneNumber,
                IsActive = false
            };

            IdentityResult result = await userManager.CreateAsync(appUser, model.PasswordHash);

            if (result.Succeeded)
            {
                try
                {
                    // Assign Role
                    await userManager.AddToRoleAsync(appUser, "Pharmacy");

                    // Create Pharmacy Profile record
                    var pharmacy = new Pharmacy
                    {
                        ApplicationUserId = appUser.Id,
                        PharmacyName = model.PharmacyName,
                        LicenseNumber = model.LicenseNumber,
                        Location = model.Location,
                        OpeningTime = model.OpeningTime,
                        ClosingTime = model.ClosingTime,
                        Is24Hours = model.Is24Hours,
                        PharmacistPhoneNumber = model.PharmacistPhoneNumber,
                        NationalIdUrl = pharmacyImagePath,
                        LicenseImageUrl = licenseImagePath
                    };

                    await pharmacyRepository.InsertAsync(pharmacy);

                    // OTP Sending and return sessionId for verification
                    return await SendOtpInternal(appUser);
                }
                catch (Exception)
                {
                    // If profile creation fails, delete the Identity user so they can try again
                    await userManager.DeleteAsync(appUser);
                    return BadRequest(result.Errors);
                }
            }

            return BadRequest(result.Errors);
            
        }


        [HttpPost("RegisterCustomer")]           //api/Account/RegisterCustomer
        public async Task<IActionResult> RegisterCustomer(CustomerRegisterDTO model)
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

                // OTP Sending and return sessionId for verification
                return await SendOtpInternal(appUser);
               
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("VerifyOTP")]           //api/Account/verifyotp
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

            var token = GenerateToken(user, claims, jwtId,expirationTime);

            return Ok(new
            {
                token = token,
                expiration = expirationTime,
                message = "Account verified and logged in successfully."
            });
        }

        [HttpPost("RequestNewOtp")]           //api/Account/requestnewotp
        public async Task<IActionResult> RequestNewOtp([FromBody] ResendOtpDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("User not found.");

            return await SendOtpInternal(user);
        }

        [HttpPost("Login")]           //api/PharmacyAccount/login
        public async Task<IActionResult> LoginAsync([FromBody]LoginDTO userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dbUser = await userManager.FindByEmailAsync(userDto.Email);
            if (dbUser == null || !await userManager.CheckPasswordAsync(dbUser, userDto.Password))
                return BadRequest("Invalid email or password."); // If the user is not found or the password is incorrect, return an error message

            if (!dbUser.EmailConfirmed)         //User must be verified
                return BadRequest("Email not verified. Please verify your OTP.");

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
                token = GenerateToken(dbUser, claims, jwtId, expiration),
                expiration = expiration
            });
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

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecurityKey"]));

            var token = new JwtSecurityToken(
                issuer: config["JWT:IssuerIP"],
                audience: config["JWT:AudienceIP"],
                expires: _expires,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }       
        // Helper method to Save incoming Images
        private async Task<string> SaveFile(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            // Define path: wwwroot/uploads/
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return the relative path to store in DB
            return $"/uploads/{folderName}/{uniqueFileName}";
        }
        // Helper method to send otp
        private async Task<IActionResult> SendOtpInternal(ApplicationUser user)
        {
            var otpCode = new Random().Next(100000, 999999).ToString();
            var otpSessionId = Guid.NewGuid();

            await otpRepository.InsertAsync(new OtpCode
            {
                UserId = user.Id,
                Code = otpCode,
                SessionId = otpSessionId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            });

            try
            {
                string subject = "Med-Map Verification Code";
                string body = $"<h2>Welcome to Med-Map!</h2><p>Your code is: <b>{otpCode}</b></p>";
                await emailService.SendEmailAsync(user.Email, subject, body);

                return Ok(new { message = "Registration successful. Verify your email.", sessionId = otpSessionId });
            }
            catch
            {
                return Ok(new { message = "User created, but email failed. Request a new OTP.", sessionId = otpSessionId, emailError = true });
            }
        }
    }
}

