using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Index.HPRtree;
using System.Data;
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
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        [HttpPost("register")]              //api/pharmacy/register
        public async Task<IActionResult> registerPharmacy([FromForm] RegisterPharmacyDTO model)
        {
            //check if user exist
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)return ErrorResponse("User not found.", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);
            if (user.IsActive) return ErrorResponse("Already registered", ErrorCodes.InvalidAction);

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
                var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
                if (pharmacy == null)
                {
                    await pharmacyRepository.InsertAsync(new Pharmacy
                    {
                        ApplicationUserId = userId
                    });
                }
                await pharmacyRepository.SaveToPendingAsync(userId, result.Profile!);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pharmacy registration failed for user {UserId}", userId);
                foreach (var path in result.UploadedFiles) await fileService.DeleteFileAsync(path);
                return ErrorResponse("An unexpected error occurred.", ErrorCodes.InternalServerError);
            }
            return SuccessResponse("Registration submitted successfully.", SuccessCodes.RegistrationPending);
        }



        [HttpPatch("update")]               //api/pharmacy/update
        [Authorize(Roles = RoleConstants.Names.Pharmacy)]
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> UpdatePharmacy([FromForm] PharmacyUpdateDTO model)
        {
            //check if user exist
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdWithPendingAsync(userId);
            if (pharmacy == null)
                return ErrorResponse("Pharmacy profile not found. Please register first.", ErrorCodes.UserNotFound);
            
            var existingBase = pharmacy.PendingProfile ?? pharmacy.ActiveProfile;
            if (existingBase == null) return ErrorResponse("No profile found.", ErrorCodes.UserNotFound);

            //Updating user info
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);

            if (model.userInfo != null)
            {
                var (success, errorMessage, errorCode) = await accountService.UpdateUserInfoAsync(user, model.userInfo);
                if (!success) return ErrorResponse(errorMessage!, errorCode!);
            }
            //build profile using the helper
            var result = await BuildUpdatedProfileAsync(model, existingBase);
            if (result.ErrorMessage != null) return ErrorResponse(result.ErrorMessage, result.ErrorCode!);
            try
            {
                await pharmacyRepository.SaveToPendingAsync(userId, result.Profile!);
                foreach (var path in result.OldFilesToDelete) await fileService.DeleteFileAsync(path);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Pharmacy update failed for user {UserId}", userId);
                foreach (var path in result.UploadedFiles) await fileService.DeleteFileAsync(path);
                return ErrorResponse("An unexpected error occurred.", ErrorCodes.ProfileCreationFailed);
            }
            user.IsActive = false;
            await userManager.UpdateAsync(user);
            return SuccessResponse("Update submitted for review successfully.", SuccessCodes.DataUpdated);
        }


        [HttpPost("activateProfile")]         //api/pharmacy/activateProfile?userId={userId}
        //[Authorize(Roles = RoleConstants.Names.Admin)]
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> ActivateProfile([FromQuery] string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return ErrorResponse("User not found", ErrorCodes.UserNotFound);

            var success = await pharmacyRepository.ActivateProfileAsync(userId);
            if (!success)
                return ErrorResponse("No pending profile found to activate.", ErrorCodes.DataNotFound);

            user.IsActive = true;
            await userManager.UpdateAsync(user);

            return SuccessResponse("Pharmacy profile activated successfully.", SuccessCodes.DataUpdated);
        }



        [HttpGet("pharmacyPublicGet")]              //api/pharmacy/pharmacypublicGet?id={id}
        [ProducesResponseType(typeof(SuccessResponseDTO<PublicPharmacyDetailsDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> getPharmacyPublicDetails([FromQuery] string id)
        {
            var phar = await pharmacyRepository.GetByIdAsync(id);
            if (phar == null || phar.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);

            var data = MapToPublicDto(phar);
            if (data == null)
                return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);

            return SuccessResponse(data, "Pharmacy retrieved successfully", SuccessCodes.DataRetrieved);
        }



        [HttpGet("searchPharmacyByName")]           //api/pharmacy/searchPharmacyByName?name={name}
        [ProducesResponseType(typeof(SuccessResponseDTO<PagedDTO<PublicPharmacyDetailsDTO>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> SearchPharmacy([FromQuery] string name, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ErrorResponse("Search term is required.", ErrorCodes.ValidationError);

            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var (pharmacies, totalCount) = await pharmacyRepository.GetByNameAsync(name, page, pageSize);

            var result = pharmacies?.Select(p => MapToPublicDto(p)).ToList() ?? new List<PublicPharmacyDetailsDTO>();

            return SuccessResponse(new PagedDTO<PublicPharmacyDetailsDTO>
            {
                items = result,
                totalCount = totalCount,
                currentPage = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }, "Pharmacies retrieved successfully", SuccessCodes.DataRetrieved);
        }



        [HttpGet("nearestPharmacy")]                //api/pharmacy/nearestPharmacy?latitude={latitude}&longitude={longitude}&radiusInMeters={radiusInMeters}
        [ProducesResponseType(typeof(SuccessResponseDTO<PagedDTO<PublicPharmacyDetailsDTO>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> GetNearestPharmacies([FromQuery] LocationRequest MyLocation, int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;
            var (items, totalCount) = await pharmacyRepository.GetNearestPharmacyAsync(
                MyLocation.latitude,
                MyLocation.longitude,
                MyLocation.radiusInMeters,
                page,
                pageSize);
            var result = items?.Select(p => MapToPublicDto(p)).ToList() ?? new List<PublicPharmacyDetailsDTO>();

            return SuccessResponse(new PagedDTO<PublicPharmacyDetailsDTO>
            {
                items = result,
                totalCount = totalCount,
                currentPage = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }, "Nearby pharmacies retrieved successfully", SuccessCodes.DataRetrieved);
        }


        [HttpGet("pharmacies")]                     //api/pharmacy/pharmacies
        [ProducesResponseType(typeof(SuccessResponseDTO<PagedDTO<PharmacyDetailsDTO>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> GetAllPharmacies([FromQuery] int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var (pharmacies, totalCount) = await pharmacyRepository.GetAllPharmaciesPaginatedAsync(page, pageSize);

            if (totalCount == 0)
                return SuccessResponse(new PagedDTO<PharmacyDetailsDTO> { items = new List<PharmacyDetailsDTO>(), totalCount = 0, currentPage = page, pageSize = pageSize, totalPages = 0 }, "No pharmacies found", SuccessCodes.DataRetrieved);

            // 3. Map the list using the helper
            var itemsDto = pharmacies.Select(MapToDetailedDto).ToList();

            // 4. Return paginated object
            return SuccessResponse(new PagedDTO<PharmacyDetailsDTO>
            {
                items = itemsDto,
                totalCount = totalCount,
                currentPage = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }, "Pharmacies retrieved successfully", SuccessCodes.DataRetrieved);
        }
        #region Helper Methods
        //helper method to convert pharmacy to DTO
        private PublicPharmacyDetailsDTO MapToPublicDto(Pharmacy phar)
        {
            if (phar == null || phar.ActiveProfile == null) return new PublicPharmacyDetailsDTO();
            return new PublicPharmacyDetailsDTO
            {
                role = "Pharmacy",
                id = Guid.Parse(phar.ApplicationUserId),
                userName = phar.User?.UserName ?? "",
                displayName = phar.User?.displayName ?? "",
                pharmacyName = phar.ActiveProfile.PharmacyName,
                pharmacyPhones = phar.ActiveProfile.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new List<string>(),
                address = phar.ActiveProfile.address,
                latitude = phar.ActiveProfile.Location.Y,
                longitude = phar.ActiveProfile.Location.X,
                openingTime = phar.ActiveProfile.OpeningTime,
                closingTime = phar.ActiveProfile.ClosingTime,
                is24Hours = phar.ActiveProfile.Is24Hours,
                deliveryAvailability = phar.ActiveProfile.HaveDelivary
            };
        }

        private PharmacyDetailsDTO MapToDetailedDto(Pharmacy phar)
        {
            return new PharmacyDetailsDTO
            {
                role = RoleConstants.Names.Pharmacy,
                id = Guid.Parse(phar.ApplicationUserId),
                userName = phar.User?.UserName ?? "",
                email = phar.User?.Email ?? "",
                phoneNumber = phar.User?.PhoneNumber ?? "",
                displayName = phar.User?.displayName ?? "",

                activeProfile = MapProfileToDto(phar.ActiveProfile),
                pendingProfile = MapProfileToDto(phar.PendingProfile)
            };
        }

        private pharmacyProfileDTO? MapProfileToDto(PharmacyProfile? profile)
        {
            if (profile == null) return null;

            // Extract documents by type using a clean LINQ query
            var licenseUrls = profile.Documents?
                .Where(d => d.Type == DocumentType.PharmacyLicense)
                .Select(d => d.FileUrl).ToList() ?? new();

            var nationalIdUrls = profile.Documents?
                .Where(d => d.Type == DocumentType.NationalId)
                .Select(d => d.FileUrl).ToList() ?? new();

            return new pharmacyProfileDTO
            {
                pharmacyName = profile.PharmacyName,
                pharmacyPhones = profile.PhoneNumbers?.Select(pn => pn.Number).ToList() ?? new(),
                address = profile.address,
                latitude = profile.Location?.Y ?? 0,
                longitude = profile.Location?.X ?? 0,
                openingTime = profile.OpeningTime,
                closingTime = profile.ClosingTime,
                is24Hours = profile.Is24Hours,
                deliveryAvailability = profile.HaveDelivary,
                licenseNumber = profile.LicenseNumber,
                licenseImageUrls = licenseUrls,
                nationalIdUrls = nationalIdUrls
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

            // This now correctly merges old and new docs
            var (documents, uploadedFiles, oldFilesToDelete, error) = await ProcessDocumentsAsync(model.nationalIds, model.licenseImages, existing.Documents);
            if (error != null) return error;

            Point location = existing.Location;
            if (model.latitude.HasValue && model.longitude.HasValue)
                location = new Point(model.longitude.Value, model.latitude.Value) { SRID = 4326 };

            var profile = new PharmacyProfile
            {
                Id = Guid.Empty,
                Location = location,
                PharmacyName = model.pharmacyName ?? existing.PharmacyName,
                address = model.address ?? existing.address,
                LicenseNumber = model.licenseNumber ?? existing.LicenseNumber,
                HaveDelivary = model.deliveryAvailability ?? existing.HaveDelivary,
                Is24Hours = model.is24Hours ?? existing.Is24Hours,
                OpeningTime = model.openingTime ?? existing.OpeningTime,
                ClosingTime = model.closingTime ?? existing.ClosingTime,
                Documents = documents, // These are now clean instances
                PhoneNumbers = validatedPhones.Count > 0
                    ? validatedPhones.Select(p => new PharmacyPhoneNumbers { Number = p }).ToList()
                    : existing.PhoneNumbers.Select(p => new PharmacyPhoneNumbers { Number = p.Number }).ToList()
            };

            return new BuildProfileResult(profile, uploadedFiles, oldFilesToDelete, null, null);
        }
        //helper method to validate Documents
        private async Task<(List<PharmacyDocument> documents, List<string> uploadedFiles, List<string> oldFilesToDelete, BuildProfileResult? error)>
        ProcessDocumentsAsync(List<IFormFile>? nationalIds, List<IFormFile>? licenseImages, ICollection<PharmacyDocument>? existingDocuments = null)
        {
            var uploadedFiles = new List<string>();
            var oldFilesToDelete = new List<string>();
            // Start with a copy of existing docs
            var documents = existingDocuments?.Select(d => new PharmacyDocument
            {
                FileUrl = d.FileUrl,
                Type = d.Type
            }).ToList() ?? new List<PharmacyDocument>();

            try
            {
                // Only replace National IDs if NEW ones are provided
                if (nationalIds != null && nationalIds.Count > 0)
                {
                    var oldNids = documents.Where(d => d.Type == DocumentType.NationalId).ToList();
                    oldFilesToDelete.AddRange(oldNids.Select(d => d.FileUrl));
                    documents.RemoveAll(d => d.Type == DocumentType.NationalId);

                    foreach (var file in nationalIds)
                    {
                        string path = await fileService.SaveFileAsync(file, "National_Ids");
                        uploadedFiles.Add(path);
                        documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.NationalId });
                    }
                }

                // Only replace License Images if NEW ones are provided
                if (licenseImages != null && licenseImages.Count > 0)
                {
                    var oldLicenses = documents.Where(d => d.Type == DocumentType.PharmacyLicense).ToList();
                    oldFilesToDelete.AddRange(oldLicenses.Select(d => d.FileUrl));
                    documents.RemoveAll(d => d.Type == DocumentType.PharmacyLicense);

                    foreach (var file in licenseImages)
                    {
                        string path = await fileService.SaveFileAsync(file, "Pharmacy_Licenses");
                        uploadedFiles.Add(path);
                        documents.Add(new PharmacyDocument { FileUrl = path, Type = DocumentType.PharmacyLicense });
                    }
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
                if (string.IsNullOrWhiteSpace(phone))
                    continue;
                if (!Regex.IsMatch(phone, @"^(\+201|01)[0125][0-9]{8}$"))
                    return (new List<string>(), "Wrong phone number format.", ErrorCodes.WrongFormat);
                validatedPhones.Add(phone);
            }
            return (validatedPhones, null, null);
        }
        #endregion
    }
}

