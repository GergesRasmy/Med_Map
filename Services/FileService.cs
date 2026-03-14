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
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Constant.AllowedMimeTypes.ContainsKey(extension))
            {
                var allowedList = string.Join(", ", Constant.AllowedMimeTypes.Keys);
                throw new Exception($"Invalid file type. Only {allowedList} are allowed.");
            }

            using (var stream = file.OpenReadStream())
            {
                var buffer = new byte[12]; // Read the first 12 bytes
                await stream.ReadExactlyAsync(buffer, 0, 12);

                if (!IsImage(buffer, extension))
                    throw new Exception("File content does not match the allowed image types.");
            }

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
        private bool IsImage(byte[] buffer, string extension)
        {
            // JPEG: FF D8 FF
            if (extension == ".jpg" || extension == ".jpeg")
                return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;

            // PNG: 89 50 4E 47
            if (extension == ".png")
                return buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;

            // HEIC: Start with 'ftypheic' (found at byte 4-11)
            if (extension == ".heic" || extension == ".heif")
                return buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70;
            
            return false;
        }

        public void DeleteFile(string filePath) { /* Delete logic */ }
    }
}
