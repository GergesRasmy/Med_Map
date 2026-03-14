using Med_Map.DTO.CustomerDTOs;
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
            if (!ModelState.IsValid)// Check if the model state is valid
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return ErrorResponse("Validation failed", ErrorCodes.ValidationError, errors);
            }    

            // Check for existing Phonenumber
            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == model.phoneNumber))
                return ErrorResponse("Phone number is already in use.", ErrorCodes.PhoneAlreadyInUse);

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
                return ErrorResponse("Registration failed during profile creation.", ErrorCodes.ProfileCreationFailed, result.Errors.Select(e => e.Description)); ;
            try
            {
                await userManager.AddToRoleAsync(appUser, "Customer");
                await customerRepository.InsertAsync(new Customer { ApplicationUserId = appUser.Id });
            }
            catch (Exception ex)
            {
                // Hard failure: If Role or Profile creation fails, we must delete the user
                await userManager.DeleteAsync(appUser);
                return ErrorResponse("Registration failed during profile setup.", ErrorCodes.ProfileCreationFailed, ex.Message);
            }
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
            var customer = await customerRepository.GetByIdAsync(id.ToString());
            if (customer == null)
                return ErrorResponse("Customer profile not found", ErrorCodes.UserNotFound);

            var user = await userManager.FindByIdAsync(customer.ApplicationUserId.ToString());
            var data = new PublicCustomerDetailsDTO { userName = user.UserName ,role ="Customer",id = Guid.Parse(customer.ApplicationUserId) };

            return SuccessResponse(data, "Customer retrieved successfully", SuccessCodes.DataRetrieved);
        }

    }
}
