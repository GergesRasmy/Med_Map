using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite;
using System.Security.Claims;

namespace Med_Map.Controllers
{
    [Route("api/Customer")]
    [ApiController]
    public class CustomerController : ResponceBaseController
    {
        #region ctor
        private readonly ICustomerRepository customerRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOtpService otpService;

        public CustomerController(ICustomerRepository customerRepository, UserManager<ApplicationUser> userManager,IOtpService otpService)
        {
            this.customerRepository = customerRepository;
            this.userManager = userManager;
            this.otpService = otpService;
        }
        #endregion
        [HttpPost("registerCustomer")]           //api/customer/registerCustomer
        public async Task<IActionResult> registerCustomer([FromBody] CustomerRegisterDTO model)
        {
            HandleValidationErrors();

            // Check for existing Phonenumber
            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == model.phoneNumber))
                return ErrorResponse("Phone number is already in use.", ErrorCodes.PhoneAlreadyInUse);

            //create user
            ApplicationUser appUser = new ApplicationUser
            {
                UserName = model.userName,
                Email = model.email,
                PhoneNumber = model.phoneNumber,
                EmailConfirmed = false,
                IsActive = true
            };
            IdentityResult result = await userManager.CreateAsync(appUser, model.password);
            if (!result.Succeeded)
                return ErrorResponse("Registration failed during profile creation.", ErrorCodes.ProfileCreationFailed, result.Errors.Select(e => e.Description));

            //Create customer, If role or Profile creation fails we must delete the user
            try
            {
                await userManager.AddToRoleAsync(appUser, "Customer");
                await customerRepository.InsertAsync(new Customer { ApplicationUserId = appUser.Id });
            }
            catch (Exception ex)
            {
                await userManager.DeleteAsync(appUser);
                return ErrorResponse("Registration failed during profile setup.", ErrorCodes.ProfileCreationFailed, ex.Message);
            }
            // Generate and send OTP for email verification, If OTP sending fails profile is created and user asked to resend OTP
            try
            {
                var otp = await otpService.GenerateAndSendOtpAsync(appUser);
                return SuccessResponse(otp, $"Verification code sent, please verify using the otp.", SuccessCodes.RegistrationPending);
            }
            catch (Exception)
            {
                return ErrorResponse("User created, but email failed to send.", ErrorCodes.OtpSendFailed);
            }
        }

        [HttpGet("customerPublicGet")]           //api/customer/customerPublicGet
        public async Task<IActionResult> getCustomerPublicDetails([FromQuery] Guid id)
        {
            // Retrieve customer details by ID
            var customer = await customerRepository.GetByIdAsync(id.ToString());
            if (customer == null)
                return ErrorResponse("Customer profile not found", ErrorCodes.UserNotFound);


            var user = await userManager.FindByIdAsync(customer.ApplicationUserId.ToString());
            var data = new PublicCustomerDetailsDTO { userName = user.UserName ,role ="Customer",id = customer.ApplicationUserId};

            return SuccessResponse(data, "Customer retrieved successfully", SuccessCodes.DataRetrieved);
        }
        [HttpPost("updateCustomer")]           //api/customer/updateCustomer
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> updateCustomer([FromBody] CustomerUpdateDTO model)
        {
            HandleValidationErrors();

            // Get the current user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return ErrorResponse("Unauthorized access", ErrorCodes.Unauthorized);

            // Retrieve customer profile by user ID
            var customer = await customerRepository.GetByIdAsync(userId);
            if (customer == null)
                return ErrorResponse("Customer profile not found", ErrorCodes.UserNotFound);

            // Mark email as unconfirmed if it has changed and unique 
            if (!string.Equals(model.email, customer.User.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await userManager.Users.AnyAsync(u => u.Email == model.email))
                    return ErrorResponse("This email is already registered to another account.", ErrorCodes.ValidationError);

                customer.User.Email = model.email;
                customer.User.EmailConfirmed = false;
            }

            // Update customer details and save changes
            customer.address = model.address;
            customer.BirthDate = model.birthDate;
            customer.MedicalHistory = model.medicalHistory;
            customer.User.UserName = model.userName;
            customer.User.PhoneNumber = model.phoneNumber;
            await customerRepository.SaveChangesAsync();

            //return response
            var data = new CustomerDetailsDTO
            {
                id = customer.ApplicationUserId,
                role = "Customer",
                userName = customer.User.UserName,
                email = customer.User.Email??"",
                phoneNumber = customer.User.PhoneNumber,
                address = customer.address,
                birthDate = customer.BirthDate,
                medicalHistory = customer.MedicalHistory
            }; 
            return SuccessResponse(data,"Customer updated successfully",SuccessCodes.DataUpdated);
        }
    }
}
