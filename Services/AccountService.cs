namespace Med_Map.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountService> _logger;

        public AccountService(UserManager<ApplicationUser> userManager, ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<(bool success, string? errorMessage, string? errorCode)> UpdateUserInfoAsync(ApplicationUser user, UpdateUserInfoDTO model)
        {
            if (model.phoneNumber != null)
            {
                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.phoneNumber && u.Id != user.Id))
                    return (false, "Phone number is already in use.", ErrorCodes.PhoneAlreadyInUse);
                user.PhoneNumber = model.phoneNumber;
            }

            if (model.email != null)
            {
                var existingEmail = await _userManager.FindByEmailAsync(model.email);
                if (existingEmail != null && existingEmail.Id != user.Id)
                    return (false, "Email is already in use.", ErrorCodes.EmailAlreadyInUse);
                user.Email = model.email;
                user.EmailConfirmed = false;
            }

            if (model.userName != null)
            {
                var existingName = await _userManager.FindByNameAsync(model.userName);
                if (existingName != null && existingName.Id != user.Id)
                    return (false, "Username is already in use.", ErrorCodes.DuplicateEntry);
                user.UserName = model.userName;
            }


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, "Failed to update user info.", ErrorCodes.InternalServerError);

            return (true, null, null);
        }
    }
}
