namespace Med_Map.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName);
        void DeleteFile(string filePath);
    }
}
