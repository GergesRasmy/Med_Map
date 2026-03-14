using Med_Map.DTO.CustomerDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Med_Map.Controllers
{
    [Route("api/pharmacy")]
    [ApiController]
    public class PharmacyController : ResponceBaseController
    { 
        #region ctor
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly IFileService fileService;
        private readonly IOtpService otpService;
        private readonly IMedicineRepository medicineRepository;

        public PharmacyController(UserManager<ApplicationUser> userManager, IPharmacyRepository pharmacyRepository, IFileService fileService,IOtpService otpService,IMedicineRepository medicineRepository)
        {
            this.userManager = userManager;
            this.pharmacyRepository = pharmacyRepository;
            this.fileService = fileService;
            this.otpService = otpService;
            this.medicineRepository = medicineRepository;
        }
        #endregion
        [HttpPost("registerPharmacy")]              //api/pharmacy/registerPharmacy
        public async Task<IActionResult> registerPharmacy([FromForm] PharmacyRegisterDTO model)
        {
            HandleValidationErrors();

            // Check if the user already exists
            if (await userManager.FindByEmailAsync(model.email) != null) 
                return ErrorResponse("Email already in use.", ErrorCodes.EmailAlreadyInUse);

            // Create the Identity User
            ApplicationUser appUser = new ApplicationUser
            {
                UserName = model.pharmacyName,
                Email = model.email,
                EmailConfirmed = false,
                IsActive = false
            };

            IdentityResult result = await userManager.CreateAsync(appUser, model.password);

            if (!result.Succeeded)
                return ErrorResponse("Registration failed during profile creation.", ErrorCodes.ProfileCreationFailed, result.Errors.Select(e => e.Description));
            
            try
            {
                var location = new Point(model.longitude, model.latitude) { SRID = 4326 };
                await userManager.AddToRoleAsync(appUser, "Pharmacy");      // Assign Role
                var pharmacy = new Pharmacy                                 // Create Pharmacy Profile record
                {
                    ApplicationUserId = appUser.Id,
                    PharmacyName = model.pharmacyName,
                    LicenseNumber = model.licenseNumber,
                    Location = location,
                    address = model.address,
                    OpeningTime = model.openingTime,
                    ClosingTime = model.closingTime,
                    Is24Hours = model.is24Hours,
                    HaveDelivary = model.deliveryAvailability,
                    doctorName = model.doctorName,
                    doctorPhoneNumber = model.doctorPhoneNumber,
                    PhoneNumbers = new List<PharmacyPhoneNumbers>(),
                    Documents = new List<PharmacyDocument>()
                };
                foreach (var phone in model.pharmacyPhones)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(\+201|01)[0125][0-9]{8}$"))
                    {
                        pharmacy.PhoneNumbers.Add(new PharmacyPhoneNumbers
                        {
                            Number = phone,
                            Pharmacy = pharmacy,
                            PharmacyId = appUser.Id
                        });
                    }
                    else
                    {
                        return ErrorResponse("Wrong phone number format.", ErrorCodes.WrongFormat);
                    }
                }
                try
                {
                    //Process National IDs
                    foreach (var file in model.nationalIds)
                    {
                        string path = await fileService.SaveFileAsync(file, "National_Ids");
                        pharmacy.Documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.NationalId });
                    }

                    //Process License Images
                    foreach (var file in model.licenseImages)
                    {
                        string path = await fileService.SaveFileAsync(file, "Pharmacy_Licenses");
                        pharmacy.Documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.PharmacyLicense });
                    }
                }
                catch (Exception ex)
                {
                    // If ANY file fails, the whole process stops here.
                    return ErrorResponse("File processing failed", ErrorCodes.ValidationError, ex.Message);
                }
                await pharmacyRepository.InsertAsync(pharmacy);
            }
            catch (Exception)
            {
                // If profile creation fails, delete the Identity user so they can try again
                await userManager.DeleteAsync(appUser);
                return ErrorResponse("Registration failed during profile creation.", ErrorCodes.ProfileCreationFailed);
            }
            try
            {
                var otp = await otpService.GenerateAndSendOtpAsync(appUser);
                return SuccessResponse(otp, $"Verification code sent, please verify using the OTP.", SuccessCodes.RegistrationPending);
            }
            catch (Exception)
            {
                return ErrorResponse("User created, but email failed to send, Request new OTP.", ErrorCodes.OtpSendFailed);
            }

        }

        [HttpGet("pharmacyPublicGet")]              //api/pharmacy/pharmacypublicGet
        public async Task<IActionResult> getPharmacyPublicDetails([FromQuery] string id)
        {
            var phar = await pharmacyRepository.GetByIdAsync(id);
            if (phar == null)
                return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);

            var data = MapToPublicDto(phar);

            return SuccessResponse(data, "Pharmacy retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [HttpGet("searchPharmacyByName")]           //api/pharmacy/searchPharmacyByName
        public async Task<IActionResult> SearchPharmacy([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ErrorResponse("Search term is required.", ErrorCodes.ValidationError);

            var pharmacies = await pharmacyRepository.GetByNameAsync(name);

            var result = pharmacies.Select(p => MapToPublicDto(p)).ToList();

            return SuccessResponse(result, "Search results retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [HttpGet("nearestPharmacy")]                //api/pharmacy/nearestPharmacy
        public async Task<IActionResult> GetNearestPharmacies([FromQuery] LocationRequest MyLocation)
        {
            var NearestPharmacies = await pharmacyRepository.GetNearestPharmacyAsync(MyLocation.latitude, MyLocation.longitude, MyLocation.radiusInMeters);
            var result = NearestPharmacies.Select(p => MapToPublicDto(p)).ToList();

            return SuccessResponse(result, "Nearby pharmacies retrieved successfully", SuccessCodes.DataRetrieved);
        }
    

        //helper method to convert pharmacy to DTO
        private PublicPharmacyDetailsDTO MapToPublicDto(Pharmacy phar)
        {
            return new PublicPharmacyDetailsDTO
            {
                role = "Pharmacy",
                id = Guid.Parse(phar.ApplicationUserId),
                pharmacyName = phar.PharmacyName,
                pharmacyPhones = phar.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new List<string>(),
                doctorName = phar.doctorName,
                address = phar.address,
                coordinates = phar.Location,
                openingTime = phar.OpeningTime,
                closingTime = phar.ClosingTime,
                is24Hours = phar.Is24Hours,
                deliveryAvailability = phar.HaveDelivary
            };
        }

    }
}

