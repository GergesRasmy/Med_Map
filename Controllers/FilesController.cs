using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Med_Map.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ResponceBaseController
    {
        private readonly IWebHostEnvironment _env;
        private readonly IPharmacyRepository pharmacyRepository;

        public FilesController(IWebHostEnvironment env, IPharmacyRepository pharmacyRepository)
        {
            _env = env;
            this.pharmacyRepository = pharmacyRepository;
        }

        // Public — customer profile pictures
        [HttpGet("avatars/{fileName}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public IActionResult GetAvatar(string fileName)
            => ServeFile(Constant.UploadFolders.CustomerAvatars, fileName);

        // Public — medicine catalog images
        [HttpGet("medicine-images/{fileName}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public IActionResult GetMedicineImage(string fileName)
            => ServeFile(Constant.UploadFolders.MedicineImages, fileName);

        // Admin, or the owning pharmacy — pharmacy license scans
        [HttpGet("licenses/{fileName}")]
        [Authorize(Roles = $"{RoleConstants.Names.Admin},{RoleConstants.Names.Pharmacy}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> GetLicense(string fileName)
        {
            if (!User.IsInRole(RoleConstants.Names.Admin) && !await OwnsDocumentAsync(fileName, "licenses"))
                return ErrorResponse("File not found", ErrorCodes.DataNotFound);

            return ServeFile(Constant.UploadFolders.PharmacyLicenses, fileName);
        }

        // Admin, or the owning pharmacy — pharmacy national ID scans
        [HttpGet("national-ids/{fileName}")]
        [Authorize(Roles = $"{RoleConstants.Names.Admin},{RoleConstants.Names.Pharmacy}")]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> GetNationalId(string fileName)
        {
            if (!User.IsInRole(RoleConstants.Names.Admin) && !await OwnsDocumentAsync(fileName, "national-ids"))
                return ErrorResponse("File not found", ErrorCodes.DataNotFound);

            return ServeFile(Constant.UploadFolders.NationalIds, fileName);
        }

        // Confirms the requested document belongs to the caller's own active or pending profile
        private async Task<bool> OwnsDocumentAsync(string fileName, string apiRoute)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return false;

            var pharmacy = await pharmacyRepository.GetByIdWithPendingAsync(userId);
            if (pharmacy == null) return false;

            var expectedUrl = $"/api/files/{apiRoute}/{fileName}";
            bool ownsActive = pharmacy.ActiveProfile?.Documents?.Any(d => d.FileUrl == expectedUrl) ?? false;
            bool ownsPending = pharmacy.PendingProfile?.Documents?.Any(d => d.FileUrl == expectedUrl) ?? false;
            return ownsActive || ownsPending;
        }

        private IActionResult ServeFile(string folder, string fileName)
        {
            if (fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains(".."))
                return ErrorResponse("Invalid file name", ErrorCodes.ValidationError);

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (!Constant.AllowedMimeTypes.TryGetValue(ext, out var contentType))
                return ErrorResponse("Invalid file type", ErrorCodes.ValidationError);

            var physicalPath = Path.Combine(_env.WebRootPath, "uploads", folder, fileName);
            if (!System.IO.File.Exists(physicalPath))
                return ErrorResponse("File not found", ErrorCodes.DataNotFound);

            return PhysicalFile(physicalPath, contentType);
        }
    }
}
