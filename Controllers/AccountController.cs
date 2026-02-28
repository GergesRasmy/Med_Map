using Med_Map.DTO.AccountDTOs;
using Med_Map.Repositories.Account;
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
            if (!ModelState.IsValid) return ErrorResponse("Validation failed",ErrorCodes.ValidationError, ModelState);     // Check if the model state is valid

            // Check if the user already exists
            if (await userManager.FindByEmailAsync(model.email) != null) return ErrorResponse("Email already in use.",ErrorCodes.EmailAlreadyInUse);

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

            IdentityResult result = await userManager.CreateAsync(appUser, model.password);

            if (result.Succeeded)
            {
                try
                {
                    await userManager.AddToRoleAsync(appUser, "Pharmacy");      // Assign Role
                    var pharmacy = new Pharmacy                                 // Create Pharmacy Profile record
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
                    return await SendOtpInternal(appUser);      // OTP Sending and return sessionId for verification
                }
                catch (Exception)
                {
                    // If profile creation fails, delete the Identity user so they can try again
                    await userManager.DeleteAsync(appUser);
                    return ErrorResponse("Registration failed during profile creation.", ErrorCodes.ProfileCreationFailed);
                }
            }
            return ErrorResponse("Registration failed during profile creation.",ErrorCodes.ProfileCreationFailed, result.Errors);
        }

        [HttpPost("registerCustomer")]           //api/Account/registerCustomer
        public async Task<IActionResult> registerCustomer([FromBody]CustomerRegisterDTO model)
        {
            if (!ModelState.IsValid) return ErrorResponse("Validation failed", ErrorCodes.ValidationError, ModelState);   // Check if the model state is valid

            // Check for existing Phonenumber
            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == model.phoneNumber))
                return ErrorResponse("Phone number is already in use.", ErrorCodes.PhoneAlreadyInUse);

            ApplicationUser appUser = new ApplicationUser
            {
                UserName = model.userName,
                Email = model.email,
                PhoneNumber = model.phoneNumber,
                IsActive = false // User is inactive until OTP is verified
            };

            IdentityResult result = await userManager.CreateAsync(appUser, model.password);

            if (result.Succeeded)
            {
                try
                {
                    await userManager.AddToRoleAsync(appUser, "Customer");  // Assign the "Customer" role to the newly created user
                    return await SendOtpInternal(appUser);      // OTP Sending and return sessionId for verification
                }
                catch (Exception)
                {
                    // If profile creation fails, delete the Identity user so they can try again
                    await userManager.DeleteAsync(appUser);
                    return ErrorResponse("Registration failed during profile creation.", ErrorCodes.ProfileCreationFailed);
                }
            }
            return ErrorResponse("Registration failed during profile creation.", ErrorCodes.ProfileCreationFailed,result.Errors); ;
        }

        [HttpPost("verifyOtp")]           //api/Account/verifyotp
        public async Task<IActionResult> verifyOtp([FromBody] VerifyOtpDTO model)
        {
            if (!ModelState.IsValid) return ErrorResponse("Validation failed", ErrorCodes.ValidationError, ModelState);   // Check if the model state is valid

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
            user.IsActive = true;
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return ErrorResponse("Failed to activate user account.", ErrorCodes.ActivitionFailed);

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
            var user = await userManager.FindByEmailAsync(model.email);
            if (user == null) return ErrorResponse("User Not Found",ErrorCodes.UserNotFound);

            return await SendOtpInternal(user);
        }

        [HttpPost("login")]           //api/Account/login
        public async Task<IActionResult> login([FromBody]LoginDTO userDto)
        {
            if (!ModelState.IsValid) return ErrorResponse("Validation failed", ErrorCodes.ValidationError, ModelState);     // Check if the model state is valid

            var dbUser = await userManager.FindByEmailAsync(userDto.email);
            if (dbUser == null || !await userManager.CheckPasswordAsync(dbUser, userDto.password))
                return ErrorResponse("Invaild Email or Password",ErrorCodes.InvalidCredentials); 

            if (!dbUser.EmailConfirmed)         //User must be verified
                return ErrorResponse("Email not verified, Please verify your Account.", ErrorCodes.EmailUnconfirmed);

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

            var otpData = new OtpResponseDataDTO
            {
                sessionId = otpSessionId,
                expiration = expirationTime
            };
            try
            {
                string subject = "Med-Map Verification Code";
                string body = $"<h2>Welcome to Med-Map!</h2><p>Your code is: <b>{otpCode}</b></p>";
                //await emailService.SendEmailAsync(user.Email, subject, body);
                Console.WriteLine($"{subject} \n {body}");
                return SuccessResponse(otpData, "Verification code sent, please verify using the otp.", SuccessCodes.RegistrationPending);
            }
            catch (Exception)
            {
                return ErrorResponse("User created, but email failed to send.", ErrorCodes.OtpSendFailed);
            }
        }
    }
}

