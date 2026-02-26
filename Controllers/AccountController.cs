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

        [HttpPost("registerPharmacy")]           //api/Account/registerPharmacy
        public async Task<IActionResult> registerPharmacy([FromForm] PharmacyRegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);     // Check if the model state is valid

            if (!model.TermConditions) return BadRequest("Accept terms to proceed.");

            // Check if the user already exists
            if (await userManager.FindByEmailAsync(model.email) != null) return BadRequest("Email already in use.");

            // Save Images to local folder (wwwroot/uploads)
            string pharmacyImagePath = await SaveFile(model.nationalId, "National_Id");
            string licenseImagePath = await SaveFile(model.licenseImage, "Pharmacy_License");

            // Create the Identity User
            ApplicationUser appUser = new ApplicationUser
            {
                UserName = model.pharmacyName,
                Email = model.email,
                PhoneNumber = model.pharmacistPhoneNumber,
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
                        PharmacyName = model.pharmacyName,
                        LicenseNumber = model.licenseNumber,
                        Location = model.location,
                        OpeningTime = model.openingTime,
                        ClosingTime = model.closingTime,
                        Is24Hours = model.is24Hours,
                        PharmacistPhoneNumber = model.pharmacistPhoneNumber,
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
                    return BadRequest(new AccountResponseDTO<object> { success = false, message = "Registration failed during profile creation." });
                }
            }

            return BadRequest(new AccountResponseDTO<object> { success = false, message = "Registration failed.", error = result.Errors });
        }

        [HttpPost("registerCustomer")]           //api/Account/registerCustomer
        public async Task<IActionResult> registerCustomer(CustomerRegisterDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);                 // Check if the model state is valid
            // Check if the user has accepted the terms and conditions
            if (!model.TermConditions) return BadRequest("You must accept the terms and conditions to register.");

            // Check for existing Phonenumber
            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == model.phoneNumber))
            {
                return BadRequest(new AccountResponseDTO<object>
                {
                    success = false,
                    code = "phone_already_used",
                    message = "Phone number is already in use."
                });
            }

            ApplicationUser appUser = new ApplicationUser
            {
                UserName = model.userName,
                Email = model.email,
                PhoneNumber = model.phoneNumber,
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

        [HttpPost("verifyOtp")]           //api/Account/verifyotp
        public async Task<IActionResult> verifyOtp([FromBody] VerifyOtpDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Check if the OTP exists, matches, hasn't been used, and isn't expired
            var otpRecord = await otpRepository.FindValidOtpAsync(model.sessionId, model.code);

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

            return Ok(new AccountResponseDTO<object>
            {
                success = true,
                message = "Account verified successfully.",
                data = new
                {
                    token = GenerateToken(user, claims, jwtId, expirationTime),
                    expiration = expirationTime,
                    role = roles.FirstOrDefault()
                }
            });
        }

        [HttpPost("requestNewOtp")]           //api/Account/requestnewotp
        public async Task<IActionResult> requestNewOtp([FromBody] ResendOtpDto model)
        {
            var user = await userManager.FindByEmailAsync(model.email);
            if (user == null) return BadRequest("User not found.");

            return await SendOtpInternal(user);
        }

        [HttpPost("Login")]           //api/PharmacyAccount/login
        public async Task<IActionResult> Login([FromBody]LoginDTO userDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var dbUser = await userManager.FindByEmailAsync(userDto.email);
            if (dbUser == null || !await userManager.CheckPasswordAsync(dbUser, userDto.password))
                return BadRequest("Invalid email or password."); // If the user is not found or the password is incorrect, return an error message

            if (!dbUser.EmailConfirmed)         //User must be verified
                return BadRequest("Email not verified. Please verify your OTP.");

            Guid sessionId = Guid.NewGuid();
            string jwtId = Guid.NewGuid().ToString(); 
            DateTime expirationTime = DateTime.UtcNow.AddHours(1);

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
            string userRole = roles.FirstOrDefault();

            
            return Ok(new AccountResponseDTO<object>
            {
                success = true,
                message = "Login successfully.",
                data = new
                {
                    token = GenerateToken(dbUser, claims, jwtId, expirationTime),
                    expiration = expirationTime,
                    role = roles.FirstOrDefault()
                }
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
            var expirationTime = DateTime.UtcNow.AddMinutes(5);
            await otpRepository.InsertAsync(new OtpCode
            {
                UserId = user.Id,
                Code = otpCode,
                SessionId = otpSessionId,
                ExpiresAt = expirationTime,
                IsUsed = false
            });

            var response = new AccountResponseDTO<object>
            {
                
                success = true,
                code = "next_verify_otp",
                message = "Logged in successfully, please verify using the otp.",
                data = new
                {
                    expiration = expirationTime,
                    sessionId = otpSessionId // Useful for the frontend to verify
                }
            };
            try
            {
                string subject = "Med-Map Verification Code";
                string body = $"<h2>Welcome to Med-Map!</h2><p>Your code is: <b>{otpCode}</b></p>";
                await emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception)
            {
                response.message = "User created, but email failed to send.";
                response.error = true;
            }
            return Ok(response);
        }
    }
}

