using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

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
        private readonly ILogger<PharmacyController> logger;
        private readonly IAccountService accountService;

        public PharmacyController(UserManager<ApplicationUser> userManager, IPharmacyRepository pharmacyRepository, IFileService fileService
                                 ,IOtpService otpService,IMedicineRepository medicineRepository, ILogger<PharmacyController> logger,IAccountService accountService)
        {
            this.userManager = userManager;
            this.pharmacyRepository = pharmacyRepository;
            this.fileService = fileService;
            this.otpService = otpService;
            this.medicineRepository = medicineRepository;
            this.logger = logger;
            this.accountService = accountService;
        }
        #endregion
        [Authorize(Roles = RoleConstants.Names.Pharmacy)]
        [HttpPost("register")]              //api/pharmacy/register
        public async Task<IActionResult> registerPharmacy([FromForm] RegisterPharmacyDTO model)
        {
            //check if user exist
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)return ErrorResponse("User not found.", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);

            //change user's info
            if (model.userInfo != null)
            {
                var (success, errorMessage, errorCode) = await accountService.UpdateUserInfoAsync(user, model.userInfo);
                if (!success) return ErrorResponse(errorMessage!, errorCode!);
            }

            //build profile using the helper
            var result = await BuildPharmacyProfileAsync(model);
            if (result.ErrorMessage != null) return ErrorResponse(result.ErrorMessage, result.ErrorCode!);
            try
            {
                await pharmacyRepository.SaveToPendingAsync(userId, result.Profile!);
                return SuccessResponse("Registration submitted successfully.", SuccessCodes.RegistrationPending);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pharmacy registration failed for user {UserId}", userId);
                foreach (var path in result.UploadedFiles) await fileService.DeleteFileAsync(path);
                return ErrorResponse("An unexpected error occurred.", ErrorCodes.InternalServerError);
            }
        }

        [HttpPost("update")]
        [Authorize(Roles = RoleConstants.Names.Pharmacy)]
        public async Task<IActionResult> UpdatePharmacy([FromForm] PharmacyUpdateDTO model)
        {
            //check if user exist
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy == null) return ErrorResponse("Pharmacy profile not found. Please register first.", ErrorCodes.UserNotFound);

            //Updating user info
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);

            if (model.userInfo != null)
            {
                var (success, errorMessage, errorCode) = await accountService.UpdateUserInfoAsync(user, model.userInfo);
                if (!success) return ErrorResponse(errorMessage!, errorCode!);
            }
            //build profile using the helper
            var existing = pharmacy.PendingProfile ?? pharmacy.ActiveProfile;
            if (existing == null)
                return ErrorResponse("No profile found to update.", ErrorCodes.UserNotFound);

            var result = await BuildUpdatedProfileAsync(model, existing);
            if (result.ErrorMessage != null) return ErrorResponse(result.ErrorMessage, result.ErrorCode!);
            try
            {
                if (pharmacy.ActiveProfile != null)
                    await pharmacyRepository.UpdateInstantFieldsAsync(userId, model);

                await pharmacyRepository.SaveToPendingAsync(userId, result.Profile!);
                return SuccessResponse("Update submitted successfully.", SuccessCodes.DataUpdated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pharmacy update failed for user {UserId}", userId);
                foreach (var path in result.UploadedFiles) await fileService.DeleteFileAsync(path);
                return ErrorResponse("An unexpected error occurred.", ErrorCodes.InternalServerError);
            }
        }

        [HttpGet("pharmacyPublicGet")]              //api/pharmacy/pharmacypublicGet
        public async Task<IActionResult> getPharmacyPublicDetails([FromQuery] string id)
        {
            var phar = await pharmacyRepository.GetByIdAsync(id);
            if (phar == null || phar.ActiveProfile == null)
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

        #region Helper Methods
        //helper method to convert pharmacy to DTO
        private PublicPharmacyDetailsDTO MapToPublicDto(Pharmacy phar)
        {
            return new PublicPharmacyDetailsDTO
            {
                role = "Pharmacy",
                id = Guid.Parse(phar.ApplicationUserId),
                pharmacyName = phar.ActiveProfile.PharmacyName,
                pharmacyPhones = phar.ActiveProfile.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new List<string>(),
                address = phar.ActiveProfile.address,
                coordinates = phar.ActiveProfile.Location,
                openingTime = phar.ActiveProfile.OpeningTime,
                closingTime = phar.ActiveProfile.ClosingTime,
                is24Hours = phar.ActiveProfile.Is24Hours,
                deliveryAvailability = phar.ActiveProfile.HaveDelivary
            };
        }

        //record for creating pharmacy profile
        private record BuildProfileResult(PharmacyProfile? Profile, List<string> UploadedFiles, List<string> OldFilesToDelete, string? ErrorMessage, string? ErrorCode);

        // For register — all fields required, no fallback needed
        private async Task<BuildProfileResult> BuildPharmacyProfileAsync(RegisterPharmacyDTO model)
        {
            var (validatedPhones, phoneError, phoneErrorCode) = ValidatePhones(model.pharmacyPhones);
            if (phoneError != null) return new BuildProfileResult(null, new List<string>(), new List<string>(), phoneError, phoneErrorCode);

            var (documents, uploadedFiles, _, error) = await ProcessDocumentsAsync(model.nationalIds, model.licenseImages);
            if (error != null) return error;

            var location = new Point(model.longitude, model.latitude) { SRID = 4326 };
            var profile = new PharmacyProfile
            {
                Location = location,
                PharmacyName = model.pharmacyName,
                address = model.address,
                LicenseNumber = model.licenseNumber,
                HaveDelivary = model.deliveryAvailability,
                Is24Hours = model.is24Hours,
                OpeningTime = model.openingTime,
                ClosingTime = model.closingTime,
                Documents = documents,
                PhoneNumbers = validatedPhones.Select(p => new PharmacyPhoneNumbers { Number = p }).ToList()
            };

            return new BuildProfileResult(profile, uploadedFiles, new List<string>(), null, null);
        }

        // For update — all fields optional, falls back to existing profile values
        private async Task<BuildProfileResult> BuildUpdatedProfileAsync(PharmacyUpdateDTO model, PharmacyProfile existing)
        {
            var (validatedPhones, phoneError, phoneErrorCode) = ValidatePhones(model.pharmacyPhones);
            if (phoneError != null) return new BuildProfileResult(null, new List<string>(), new List<string>(), phoneError, phoneErrorCode);
            var (documents, uploadedFiles, oldFilesToDelete, error) = await ProcessDocumentsAsync(model.nationalIds, model.licenseImages, existing.Documents);
            if (error != null) return error;

            Point location = existing.Location;
            if (model.latitude.HasValue && model.longitude.HasValue)
                location = new Point(model.longitude.Value, model.latitude.Value) { SRID = 4326 };

            var profile = new PharmacyProfile
            {
                Location = location,
                PharmacyName = model.pharmacyName ?? existing.PharmacyName,
                address = model.address ?? existing.address,
                LicenseNumber = model.licenseNumber ?? existing.LicenseNumber,
                HaveDelivary = model.deliveryAvailability ?? existing.HaveDelivary,
                Is24Hours = model.is24Hours ?? existing.Is24Hours,
                OpeningTime = model.openingTime ?? existing.OpeningTime,
                ClosingTime = model.closingTime ?? existing.ClosingTime,
                Documents = documents,
                PhoneNumbers = validatedPhones.Count > 0
                    ? validatedPhones.Select(p => new PharmacyPhoneNumbers { Number = p }).ToList()
                    : existing.PhoneNumbers.ToList()
            };

            return new BuildProfileResult(profile, uploadedFiles, oldFilesToDelete, null, null);
        }

        //helper method to validate Documents
        private async Task<(List<PharmacyDocument> documents, List<string> uploadedFiles, List<string> oldFilesToDelete, BuildProfileResult? error)>
        ProcessDocumentsAsync(List<IFormFile>? nationalIds, List<IFormFile>? licenseImages, ICollection<PharmacyDocument>? existingDocuments = null)
        {
            var uploadedFiles = new List<string>();
            var oldFilesToDelete = new List<string>();
            var documents = existingDocuments?.ToList() ?? new List<PharmacyDocument>();

            try
            {
                if ((nationalIds != null || licenseImages != null) && existingDocuments != null)
                {
                    oldFilesToDelete = documents.Select(d => d.FileUrl).ToList();
                    documents.Clear();
                }

                if (nationalIds != null)
                    foreach (var file in nationalIds)
                    {
                        string path = await fileService.SaveFileAsync(file, "National_Ids");
                        uploadedFiles.Add(path);
                        documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.NationalId });
                    }

                if (licenseImages != null)
                    foreach (var file in licenseImages)
                    {
                        string path = await fileService.SaveFileAsync(file, "Pharmacy_Licenses");
                        uploadedFiles.Add(path);
                        documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.PharmacyLicense });
                    }

                return (documents, uploadedFiles, oldFilesToDelete, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "File upload failed");
                foreach (var path in uploadedFiles) await fileService.DeleteFileAsync(path);
                return (new List<PharmacyDocument>(), uploadedFiles, oldFilesToDelete,
                    new BuildProfileResult(null, uploadedFiles, new List<string>(), "File upload failed.", ErrorCodes.InternalServerError));
            }
        }
        //helper method to validate Phone Numbers
        private (List<string> phones, string? errorMessage, string? errorCode) ValidatePhones(List<string>? phones)
        {
            var validatedPhones = new List<string>();
            if (phones == null) return (validatedPhones, null, null);

            foreach (var phone in phones)
            {
                if (!Regex.IsMatch(phone, @"^(\+201|01)[0125][0-9]{8}$"))
                    return (new List<string>(), "Wrong phone number format.", ErrorCodes.WrongFormat);
                validatedPhones.Add(phone);
            }
            return (validatedPhones, null, null);
        }
        #endregion
    }
}

