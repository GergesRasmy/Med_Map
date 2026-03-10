using Med_Map.DTO.CustomerDTOs;
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

        [HttpGet("customerPublicGet")]//api/user/customerPublicGet
        public async Task<IActionResult> getCustomerPublicDetails([FromQuery] Guid id)
        {
            var customer = await customerRepository.GetByIdAsync(id.ToString());
            if (customer == null)
                return ErrorResponse("Customer profile not found", ErrorCodes.UserNotFound);

            var user = await userManager.FindByIdAsync(customer.ApplicationUserId.ToString());
            var data = new PublicCustomerDetailsDTO { userName = user.UserName };
            
            return SuccessResponse(data, "Customer retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [HttpGet("pharmacyPublicGet")]//api/user/pharmacypublicGet
        public async Task<IActionResult> getPharmacyPublicDetails([FromQuery] Guid id)
        {
            var phar = await pharmacyRepository.GetByIdAsync(id.ToString());
            if (phar == null)
                return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);

            var data = MapToPublicDto(phar);
              
            return SuccessResponse(data, "Pharmacy retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [HttpGet("privateGet")]//api/user/privateGet
        public async Task<IActionResult> getPrivateDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return ErrorResponse("Invalid token or user not found", ErrorCodes.InvalidCredentials);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("Invalid token or user not found", ErrorCodes.UserNotFound);


            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Customer")
            {
                var customer = await customerRepository.GetByIdAsync(userId);
                if (customer == null)
                    return ErrorResponse("Customer profile not found", ErrorCodes.UserNotFound);

                var data = new CustomerDetailsDTO
                {
                    birthDate = customer.BirthDate,
                    medicalHistory = customer.MedicalHistory,
                    userName = user.UserName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber
                };
                return SuccessResponse(data, "Customer retrieved successfully", SuccessCodes.DataRetrieved);
            }
            else if (role == "Pharmacy")
            {
                var phar = await pharmacyRepository.GetByIdAsync(userId);
                if (phar == null)
                    return ErrorResponse("Customer profile not found", ErrorCodes.UserNotFound);
                List<string> LicenseImageUrls = new List<string>();
                List<string> NationalIdUrls = new List<string>();
                foreach (var doc in phar.Documents)
                {
                    if (doc.Type == DocumentType.PharmacyLicense)
                    {
                        LicenseImageUrls.Add(doc.FileUrl);
                    }
                    NationalIdUrls.Add(doc.FileUrl);
                }
                var data = new PharmacyDetailsDTO
                {
                    email = user.Email,
                    pharmacyName = phar.PharmacyName,
                    pharmacyPhones = phar.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new List<string>(),
                    doctorName = phar.doctorName,
                    doctorPhoneNumber = phar.doctorPhoneNumber,
                    address = phar.address,
                    cordinates = phar.Location,
                    openingTime = phar.OpeningTime,
                    closingTime = phar.ClosingTime,
                    is24Hours = phar.Is24Hours,
                    delivaryAvailability = phar.HaveDelivary,
                    licenseNumber = phar.LicenseNumber,
                    licenseImageUrls = LicenseImageUrls,
                    nationalIdUrls = NationalIdUrls
                };
                return SuccessResponse(data, "Customer retrieved successfully", SuccessCodes.DataRetrieved);
            }
            else
            {
                return ErrorResponse("Invalid role", ErrorCodes.Unauthorized);
            }
        }

        [HttpGet("searchPharmacyByName")]//api/user/searchPharmacyByName
        public async Task<IActionResult> SearchPharmacy([FromQuery]string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ErrorResponse("Search term is required.", ErrorCodes.ValidationError);

            var pharmacies = await pharmacyRepository.GetByNameAsync(name);

            var result = pharmacies.Select(p => new PublicPharmacyDetailsDTO
            {
                pharmacyName = p.PharmacyName,
                pharmacyPhones = p.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new List<string>(),
                doctorName = p.doctorName,
                address = p.address,
                cordinates = p.Location,
                openingTime = p.OpeningTime,
                closingTime = p.ClosingTime,
                is24Hours = p.Is24Hours,
                delivaryAvailability = p.HaveDelivary
            }).ToList();

            return SuccessResponse(result, "Search results retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [HttpGet("nearestPharmacy")]
        public async Task<IActionResult> GetNearestPharmacies([FromQuery] LocationRequest MyLocation)
        {
            var NearestPharmacies = await pharmacyRepository.GetNearestPharmacyAsync(MyLocation.latitude,MyLocation.longitude,MyLocation.radiusInMeters);
            var result = NearestPharmacies.Select(p=>MapToPublicDto(p)).ToList();

            return SuccessResponse(result, "Nearby pharmacies retrieved successfully", SuccessCodes.DataRetrieved);
        }

        //helper method to convert pharmacy to DTO
        private PublicPharmacyDetailsDTO MapToPublicDto(Pharmacy phar)
        {
            return new PublicPharmacyDetailsDTO
            {
                pharmacyName = phar.PharmacyName,
                pharmacyPhones = phar.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new List<string>(),
                doctorName = phar.doctorName,
                address = phar.address,
                cordinates = phar.Location,
                openingTime = phar.OpeningTime,
                closingTime = phar.ClosingTime,
                is24Hours = phar.Is24Hours,
                delivaryAvailability = phar.HaveDelivary
            };
        }

    }
}
