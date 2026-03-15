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
        [HttpPost("register")]           //api/customer/register
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerUpdateDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found",ErrorCodes.UserNotFound);

            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == model.phoneNumber))
                return ErrorResponse("Phone number is already in use.", ErrorCodes.PhoneAlreadyInUse);
            else user.PhoneNumber = model.phoneNumber;

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
        [HttpPost("update")]           //api/customer/update
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] CustomerUpdateDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
           
            var customer = await customerRepository.GetByIdAsync(userId);
            if (customer == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);
            
            if (!string.Equals(model.email, customer.User.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await userManager.Users.AnyAsync(u => u.Email == model.email))
                    return ErrorResponse("Email already in use.", ErrorCodes.ValidationError);
                customer.User.Email = model.email;
                customer.User.EmailConfirmed = false;
            }
            if (await userManager.Users.AnyAsync(u => u.PhoneNumber == model.phoneNumber) && model.phoneNumber != null)
                return ErrorResponse("Phone number is already in use.", ErrorCodes.PhoneAlreadyInUse);
            
            customer.address = model.address;
            customer.BirthDate = model.birthDate;
            customer.MedicalHistory = model.medicalHistory;
            customer.User.UserName = model.userName;
            customer.User.PhoneNumber = model.phoneNumber;


            await customerRepository.SaveChangesAsync();

            // Use the helper to create the response object
            var data = MapToCustomerDetails(customer);

            return SuccessResponse(data, "Customer updated successfully", SuccessCodes.DataUpdated);
        }
        private CustomerDetailsDTO MapToCustomerDetails(Customer customer)
        {
            return new CustomerDetailsDTO
            {
                id = customer.ApplicationUserId,
                role = "Customer",
                userName = customer.User.UserName??"",
                email = customer.User.Email ?? "",
                phoneNumber = customer.User.PhoneNumber,
                address = customer.address,
                birthDate = customer.BirthDate,
                medicalHistory = customer.MedicalHistory
            };
        }
    }
}
