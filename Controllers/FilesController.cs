namespace Med_Map.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ResponceBaseController
    {
        private readonly IWebHostEnvironment _env;

        public FilesController(IWebHostEnvironment env) => _env = env;

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

        // Admin only — pharmacy license scans
        [HttpGet("licenses/{fileName}")]
        [Authorize(Roles = RoleConstants.Names.Admin)]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public IActionResult GetLicense(string fileName)
            => ServeFile(Constant.UploadFolders.PharmacyLicenses, fileName);

        // Admin only — pharmacy national ID scans
        [HttpGet("national-ids/{fileName}")]
        [Authorize(Roles = RoleConstants.Names.Admin)]
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public IActionResult GetNationalId(string fileName)
            => ServeFile(Constant.UploadFolders.NationalIds, fileName);

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
