namespace Med_Map.Services
{
    public class FileService: IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
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
            var allowedExtensions = Constant.AllowedExtensions;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new Exception($"Invalid file type. Only {allowedExtensions} are allowed.");

            //Define and Ensure Directory
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            //Secure File Naming
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //Save the file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        public void DeleteFile(string filePath) { /* Delete logic */ }
    }
}
