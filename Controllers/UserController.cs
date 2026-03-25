using Med_Map.DTO.CustomerDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite;
using System.Security.Claims;

namespace Med_Map.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ResponceBaseController
    {
        #region ctor
        private readonly ICustomerRepository customerRepository;
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public UserController(ICustomerRepository customerRepository, IPharmacyRepository pharmacyRepository, UserManager<ApplicationUser> userManager)
        {
            this.customerRepository = customerRepository;
            this.pharmacyRepository = pharmacyRepository;
            this.userManager = userManager;
        }
        #endregion
        [ProducesResponseType(typeof(SuccessResponseDTO<UserDetailsDTO>), 200)]
        [ProducesResponseType(typeof(SuccessResponseDTO<CustomerDetailsDTO>), 200)]
        [ProducesResponseType(typeof(SuccessResponseDTO<PharmacyDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        [Authorize]
        [HttpGet("privateGet")]         //api/user/privateGet
        public async Task<IActionResult> getPrivateDetails()
        {
            // Extract user ID and role from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return ErrorResponse("Invalid token or user not found", ErrorCodes.InvalidCredentials);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("Invalid token or user not found", ErrorCodes.UserNotFound);

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Fetch and return customer details
            if (role == RoleConstants.Names.Customer)
            {
                // Fetch customer details 
                var customer = await customerRepository.GetByIdAsync(userId,asNoTracking:true);
                if (customer == null)
                {
                    // Map customer details to DTO
                    var Data = new UserDetailsDTO
                    {
                        id = userId,
                        role = role,
                        userName = user.UserName ?? "",
                        email = user.Email ?? "",
                    };
                    return SuccessResponse(Data, "User retrieved successfully", SuccessCodes.DataRetrieved);
                }
                // Map customer details to DTO
                var data = new CustomerDetailsDTO
                {
                    id = customer.ApplicationUserId,
                    role = role,
                    userName = user.UserName ,
                    email = user.Email ,
                    phoneNumber = user.PhoneNumber,
                    address = customer.address ,
                    birthDate = customer.BirthDate,
                    medicalHistory = customer.MedicalHistory 
                };
                return SuccessResponse(data, "Customer retrieved successfully", SuccessCodes.DataRetrieved);
            }
            // Fetch and return pharmacy details
            else if (role == RoleConstants.Names.Pharmacy)
            {
                var phar = await pharmacyRepository.GetByIdWithPendingAsync(userId);
                if (phar == null || (phar.ActiveProfile == null && phar.PendingProfile == null))
                {
                    var Data = new UserDetailsDTO
                    {
                        id = userId,
                        role = role,
                        userName = user.UserName ?? "",
                        email = user.Email ?? "",
                        displayName = user.displayName ?? ""
                    };
                    return SuccessResponse(Data, "User retrieved successfully", SuccessCodes.DataRetrieved);
                }

                // Extract document URLs from ActiveProfile
                List<string> LicenseImageUrls = new List<string>();
                List<string> NationalIdUrls = new List<string>();
                if (phar.ActiveProfile != null)
                {
                    foreach (var doc in phar.ActiveProfile.Documents)
                    {
                        if (doc.Type == DocumentType.PharmacyLicense)
                            LicenseImageUrls.Add(doc.FileUrl);
                        else if (doc.Type == DocumentType.NationalId)
                            NationalIdUrls.Add(doc.FileUrl);
                    }
                }

                // Extract document URLs from PendingProfile
                List<string> PendingLicenseImageUrls = new List<string>();
                List<string> PendingNationalIdUrls = new List<string>();
                if (phar.PendingProfile != null)
                {
                    foreach (var doc in phar.PendingProfile.Documents)
                    {
                        if (doc.Type == DocumentType.PharmacyLicense)
                            PendingLicenseImageUrls.Add(doc.FileUrl);
                        else if (doc.Type == DocumentType.NationalId)
                            PendingNationalIdUrls.Add(doc.FileUrl);
                    }
                }

                var data = new PharmacyDetailsDTO
                {
                    role = role,
                    id = Guid.Parse(phar.ApplicationUserId),
                    userName = user.UserName ?? "",
                    email = user.Email ?? "",
                    phoneNumber = user.PhoneNumber ?? "",
                    displayName = user.displayName ?? "",

                    // Active profile
                    activeProfile = phar.ActiveProfile == null ? null : new pharmacyProfileDTO
                    {
                        pharmacyName = phar.ActiveProfile.PharmacyName,
                        pharmacyPhones = phar.ActiveProfile.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new(),
                        address = phar.ActiveProfile.address,
                        latitude = phar.ActiveProfile.Location.Y, 
                        longitude = phar.ActiveProfile.Location.X,
                        openingTime = phar.ActiveProfile.OpeningTime,
                        closingTime = phar.ActiveProfile.ClosingTime,
                        is24Hours = phar.ActiveProfile.Is24Hours,
                        deliveryAvailability = phar.ActiveProfile.HaveDelivary,
                        licenseNumber = phar.ActiveProfile.LicenseNumber,
                        licenseImageUrls = LicenseImageUrls,
                        nationalIdUrls = NationalIdUrls
                    },

                    // Pending profile
                    pendingProfile = phar.PendingProfile == null ? null : new pharmacyProfileDTO
                    {
                        pharmacyName = phar.PendingProfile.PharmacyName,
                        pharmacyPhones = phar.PendingProfile.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new(),
                        address = phar.PendingProfile.address,
                        latitude = phar.PendingProfile.Location.Y,
                        longitude = phar.PendingProfile.Location.X,
                        openingTime = phar.PendingProfile.OpeningTime,
                        closingTime = phar.PendingProfile.ClosingTime,
                        is24Hours = phar.PendingProfile.Is24Hours,
                        deliveryAvailability = phar.PendingProfile.HaveDelivary,
                        licenseNumber = phar.PendingProfile.LicenseNumber,
                        licenseImageUrls = PendingLicenseImageUrls,
                        nationalIdUrls = PendingNationalIdUrls
                    }
                };
                return SuccessResponse(data, "Pharmacy retrieved successfully", SuccessCodes.DataRetrieved);
            }// Handle invalid role
            else return ErrorResponse("Invalid role", ErrorCodes.Unauthorized);
        }
        
    }
}
