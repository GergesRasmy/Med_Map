using Med_Map.DTO.CustomerDTOs;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles ="Pharmacy")]
        [HttpPost("register")]              //api/pharmacy/register
        public async Task<IActionResult> registerPharmacy([FromForm] PharmacyUpdateDTO model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)return ErrorResponse("User not found.", ErrorCodes.Unauthorized);
            var existing = await pharmacyRepository.GetByIdAsync(userId);
            if (existing != null) return ErrorResponse("Profile already initialized.", ErrorCodes.ValidationError);

            var uploadedFiles = new List<string>();
            try
            {
                var location = new Point(model.longitude, model.latitude) { SRID = 4326 };
                var pharmacy = new PharmacyProfile               // Create Pharmacy Profile record
                {
                    PharmacyName = model.pharmacyName,
                    LicenseNumber = model.licenseNumber,
                    Location = location,
                    address = model.address,
                    OpeningTime = model.openingTime,
                    ClosingTime = model.closingTime,
                    Is24Hours = model.is24Hours,
                    HaveDelivary = model.deliveryAvailability,
                    PhoneNumbers = new List<PharmacyPhoneNumbers>(),
                    Documents = new List<PharmacyDocument>()
                };
                foreach (var phone in model.pharmacyPhones)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(\+201|01)[0125][0-9]{8}$"))
                    {
                        pharmacy.PhoneNumbers.Add(new PharmacyPhoneNumbers { Number = phone });
                    }
                    else return ErrorResponse("Wrong phone number format.", ErrorCodes.WrongFormat);
                }
                foreach (var file in model.nationalIds)
                {
                    string path = await fileService.SaveFileAsync(file, "National_Ids");
                    uploadedFiles.Add(path);
                    pharmacy.Documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.NationalId });
                }

                foreach (var file in model.licenseImages)
                {
                    string path = await fileService.SaveFileAsync(file, "Pharmacy_Licenses");
                    uploadedFiles.Add(path); 
                    pharmacy.Documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.PharmacyLicense });
                }
                await pharmacyRepository.SaveToPendingAsync(userId,pharmacy);
                return SuccessResponse("Update submitted successfully.", SuccessCodes.RegistrationPending);
            }
            catch (Exception ex)
            {
                // Delete files that were successfully saved before the error occurred
                foreach (var filePath in uploadedFiles)
                {
                    await fileService.DeleteFileAsync(filePath);
                }

                return ErrorResponse("File processing failed. Changes rolled back.", ErrorCodes.ValidationError, ex.Message);
            }

        }

        [HttpPost("update")]
        [Authorize(Roles = "Pharmacy")]
        public async Task<IActionResult> UpdatePharmacy([FromForm] PharmacyUpdateDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy == null) return ErrorResponse("Pharmacy profile not found. Please register first.", ErrorCodes.UserNotFound);

            var uploadedFiles = new List<string>();
            try
            {
                //Update basic fields that doesn't need reviewing
                pharmacy.ActiveProfile.HaveDelivary = model.deliveryAvailability;
                pharmacy.ActiveProfile.Is24Hours = model.is24Hours;
                pharmacy.ActiveProfile.OpeningTime = model.openingTime;
                pharmacy.ActiveProfile.ClosingTime = model.closingTime;

                //Update basic fields that does need reviewing
                if (pharmacy.PendingProfile == null)
                {
                    pharmacy.PendingProfile = new PharmacyProfile();
                }
                var location = new Point(model.longitude, model.latitude) { SRID = 4326 };
                pharmacy.PendingProfile.Location = location;
                pharmacy.PendingProfile.PharmacyName = model.pharmacyName;
                pharmacy.PendingProfile.address = model.address;
                pharmacy.PendingProfile.LicenseNumber = model.licenseNumber;
                //Process Files

                if (model.nationalIds != null)
                {
                    foreach (var file in model.nationalIds)
                    {
                        string path = await fileService.SaveFileAsync(file, "National_Ids");
                        uploadedFiles.Add(path);
                        pharmacy.PendingProfile.Documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.NationalId });
                    }
                }
                if (model.licenseImages != null)
                {
                    foreach (var file in model.licenseImages)
                    {
                        string path = await fileService.SaveFileAsync(file, "Pharmacy_Licenses");
                        uploadedFiles.Add(path);
                        pharmacy.PendingProfile.Documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.PharmacyLicense });
                    }
                }
                foreach (var phone in model.pharmacyPhones)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(\+201|01)[0125][0-9]{8}$"))
                    {
                        pharmacy.PendingProfile.PhoneNumbers.Add(new PharmacyPhoneNumbers { Number = phone });
                    }
                    else return ErrorResponse("Wrong phone number format.", ErrorCodes.WrongFormat);
                }

                await pharmacyRepository.SaveChangesAsync();
                return SuccessResponse("Pharmacy updated successfully.", SuccessCodes.DataUpdated);
            }
            catch (Exception ex)
            {
                // Rollback files if database update fails
                foreach (var path in uploadedFiles) await fileService.DeleteFileAsync(path);
                return ErrorResponse("File processing failed. Changes rolled back.", ErrorCodes.ValidationError, ex.Message);
            }
        }

        [HttpGet("pharmacyPublicGet")]              //api/pharmacy/pharmacypublicGet
        public async Task<IActionResult> getPharmacyPublicDetails([FromQuery] string id)
        {
            var phar = await pharmacyRepository.GetByIdAsync(id);
            if (phar.ActiveProfile == null)
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
                pharmacyName = phar.ActiveProfile.PharmacyName,
                pharmacyPhones = phar.ActiveProfile.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new List<string>(),
                //doctorName = phar.ActiveProfile.doctorName,
                address = phar.ActiveProfile.address,
                coordinates = phar.ActiveProfile.Location,
                openingTime = phar.ActiveProfile.OpeningTime,
                closingTime = phar.ActiveProfile.ClosingTime,
                is24Hours = phar.ActiveProfile.Is24Hours,
                deliveryAvailability = phar.ActiveProfile.HaveDelivary
            };
        }
        
    }
}

