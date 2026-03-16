namespace Med_Map.Services
{
    public interface IAccountService
    {
        Task<(bool success, string? errorMessage, string? errorCode)> UpdateUserInfoAsync(ApplicationUser user, UpdateUserInfoDTO model);
    }
}
