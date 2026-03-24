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
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICustomerRepository customerRepository;
        private readonly IOtpService otpService;
        private readonly IAccountService accountService;

        public CustomerController(UserManager<ApplicationUser> userManager, ICustomerRepository customerRepository, IOtpService otpService,IAccountService accountService)
        {
            this.userManager = userManager;
            this.customerRepository = customerRepository;
            this.otpService = otpService;
            this.accountService = accountService;
        }
        #endregion
        [HttpPost("register")]           //api/customer/register
        [Authorize(Roles = RoleConstants.Names.Customer)]
        [ProducesResponseType(typeof(SuccessResponseDTO<CustomerDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found",ErrorCodes.UserNotFound);
            if (user.IsActive == true) return ErrorResponse("User Already Registered", ErrorCodes.InvalidAction);
            if (model.userInfo.phoneNumber == null) return ErrorResponse("Phone number is required", ErrorCodes.ValidationError);

            var (success, errorMessage, errorCode) = await accountService.UpdateUserInfoAsync(user, model.userInfo);
            if (!success) return ErrorResponse(errorMessage!, errorCode!);
            
            var customer = new Customer
            {
                ApplicationUserId = userId,
                User = user,
                address = model.address,
                BirthDate = model.birthDate,
                MedicalHistory = model.medicalHistory
            };

            await customerRepository.InsertAsync(customer);
            var data = MapToCustomerDetails(customer);

            return SuccessResponse(data, "Customer added successfully", SuccessCodes.DataUpdated);
        }
       
        [HttpPatch("update")]           //api/customer/update
        [Authorize(Roles = RoleConstants.Names.Customer)]
        [ProducesResponseType(typeof(SuccessResponseDTO<CustomerDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> UpdateCustomer([FromBody] CustomerUpdateDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);

            var customer = await customerRepository.GetByIdAsync(userId,asNoTracking:false);
            if (customer == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);

            var (success, errorMessage, errorCode) = await accountService.UpdateUserInfoAsync(user, model.userInfo);
            if (!success) return ErrorResponse(errorMessage!, errorCode!);            

            if (model.address != null) customer.address = model.address;
            if (model.birthDate != null) customer.BirthDate = model.birthDate.Value;
            if (model.medicalHistory != null) customer.MedicalHistory = model.medicalHistory;

            await customerRepository.SaveChangesAsync();

            // Use the helper to create the response object
            var data = MapToCustomerDetails(customer);

            return SuccessResponse(data, "Customer updated successfully", SuccessCodes.DataUpdated);
        }

        [HttpGet("customerPublicGet")]           //api/customer/customerPublicGet
        [ProducesResponseType(typeof(SuccessResponseDTO<PublicCustomerDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> getCustomerPublicDetails([FromQuery] string id)
        {
            // Retrieve customer details by ID
            var user = await userManager.FindByIdAsync(id);
            if (user == null||user.IsActive==false)
                return ErrorResponse("User not found", ErrorCodes.UserNotFound);


            var customer = await customerRepository.GetByIdAsync(id, asNoTracking: true);
            if (customer == null)
                return ErrorResponse("Customer profile not found", ErrorCodes.UserNotFound);

            var data = new PublicCustomerDetailsDTO 
            {
                userName = user.UserName ,
                displayName = user.displayName,
                role ="Customer",
                id = id
            };
            return SuccessResponse(data, "Customer retrieved successfully", SuccessCodes.DataRetrieved);
        }
        private CustomerDetailsDTO MapToCustomerDetails(Customer customer)
        {
            return new CustomerDetailsDTO
            {
                id = customer.ApplicationUserId,
                role = "Customer",
                userName = customer.User.UserName??"",
                displayName = customer.User.displayName??"",
                email = customer.User.Email ?? "",
                phoneNumber = customer.User.PhoneNumber,
                address = customer.address,
                birthDate = customer.BirthDate,
                medicalHistory = customer.MedicalHistory
            };
        }
    }
}
