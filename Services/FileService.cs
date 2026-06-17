namespace Med_Map.Services
{
    public class FileService: IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File can't be null");

            //file size validation
            const long MaxFileSize = Constant.MaxFileSize;
            if (file.Length > MaxFileSize)
                throw new Exception($"File size exceeds the {Constant.MaxFileSize / 1024 / 1024}MB limit.");

            //Extension Type Validation
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Constant.AllowedMimeTypes.ContainsKey(extension))
            {
                var allowedList = string.Join(", ", Constant.AllowedMimeTypes.Keys);
                throw new Exception($"Invalid file type. Only {allowedList} are allowed.");
            }

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var apiRoute = Constant.UploadFolders.ToApiRoute[folderName];
            return $"/api/files/{apiRoute}/{uniqueFileName}";
        }
       

        public async Task DeleteFileAsync(string apiUrl)
        {
            // apiUrl format: /api/files/{route}/{filename}
            var segments = apiUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 4)
            {
                _logger.LogWarning("Skipping delete — unexpected URL format: {Url}", apiUrl);
                return;
            }

            var apiRoute = segments[2]; // "avatars" | "licenses" | "national-ids" | "medicine-images"
            var fileName = segments[3];

            if (!Constant.UploadFolders.FromApiRoute.TryGetValue(apiRoute, out var folder))
            {
                _logger.LogWarning("Skipping delete — unknown route segment '{Route}' in URL: {Url}", apiRoute, apiUrl);
                return;
            }

            string absolutePath = Path.Combine(_environment.WebRootPath, "uploads", folder, fileName);
            try
            {
                if (File.Exists(absolutePath))
                {
                    await Task.Run(() => File.Delete(absolutePath));
                    _logger.LogInformation("Successfully deleted file: {Path}", absolutePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup file at {Path}", absolutePath);
            }
        }
    }
}
