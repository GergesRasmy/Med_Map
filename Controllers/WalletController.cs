using Med_Map.DTO.WalletDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Med_Map.Controllers
{
    [Route("api/wallet")]
    [ApiController]
    [Authorize(Roles = RoleConstants.Names.Pharmacy)]
    public class WalletController : ResponceBaseController
    {
        private const int MaxPinAttempts = 5;
        private const int LockoutMinutes = 15;

        #region ctor
        private readonly IWalletRepository walletRepository;
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPasswordHasher<ApplicationUser> passwordHasher;

        public WalletController(
            IWalletRepository walletRepository,
            IPharmacyRepository pharmacyRepository,
            UserManager<ApplicationUser> userManager,
            IPasswordHasher<ApplicationUser> passwordHasher)
        {
            this.walletRepository = walletRepository;
            this.pharmacyRepository = pharmacyRepository;
            this.userManager = userManager;
            this.passwordHasher = passwordHasher;
        }
        #endregion

        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponseDTO<WalletResponseDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> GetWallet()
        {
            var (wallet, error) = await ResolveWalletAsync();
            if (error != null) return error;

            return SuccessResponse(MapToDTO(wallet!), "Wallet retrieved.", SuccessCodes.DataRetrieved);
        }

        [HttpPost("setPin")]
        [ProducesResponseType(typeof(SuccessResponseDTO<WalletResponseDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> SetPin([FromBody] SetPinDTO model)
        {
            var (wallet, error) = await ResolveWalletAsync();
            if (error != null) return error;

            if (wallet!.PinHash != null)
                return ErrorResponse("PIN already set. Use changePin to update it.", ErrorCodes.InvalidAction);

            wallet.PinHash = passwordHasher.HashPassword(null!, model.Pin);
            await walletRepository.SaveChangesAsync();

            return SuccessResponse(MapToDTO(wallet), "PIN set successfully.", SuccessCodes.DataUpdated);
        }

        [HttpPost("changePin")]
        [ProducesResponseType(typeof(SuccessResponseDTO<WalletResponseDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> ChangePin([FromBody] ChangePinDTO model)
        {
            var (wallet, error) = await ResolveWalletAsync();
            if (error != null) return error;

            if (wallet!.PinHash == null)
                return ErrorResponse("No PIN set. Use setPin first.", ErrorCodes.InvalidAction);

            var lockError = CheckLockout(wallet);
            if (lockError != null) return lockError;

            var result = passwordHasher.VerifyHashedPassword(null!, wallet.PinHash, model.CurrentPin);
            if (result == PasswordVerificationResult.Failed)
            {
                wallet.PinFailedAttempts++;
                if (wallet.PinFailedAttempts >= MaxPinAttempts)
                    wallet.PinLockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                await walletRepository.SaveChangesAsync();
                return ErrorResponse("Incorrect PIN.", ErrorCodes.Unauthorized);
            }

            wallet.PinHash = passwordHasher.HashPassword(null!, model.NewPin);
            wallet.PinFailedAttempts = 0;
            wallet.PinLockedUntil = null;
            await walletRepository.SaveChangesAsync();

            return SuccessResponse(MapToDTO(wallet), "PIN changed successfully.", SuccessCodes.DataUpdated);
        }

        #region helpers

        private async Task<(Wallet? wallet, IActionResult? error)> ResolveWalletAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return (null, ErrorResponse("Unauthorized.", ErrorCodes.Unauthorized));

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy?.ActiveProfile == null)
                return (null, ErrorResponse("Pharmacy has no active profile.", ErrorCodes.CompleteRegistration));

            var wallet = await walletRepository.GetByPharmacyProfileIdAsync(pharmacy.ActiveProfile.Id);
            if (wallet == null)
                return (null, ErrorResponse("Wallet not found.", ErrorCodes.DataNotFound));

            return (wallet, null);
        }

        private IActionResult? CheckLockout(Wallet wallet)
        {
            if (wallet.PinLockedUntil.HasValue && wallet.PinLockedUntil.Value > DateTime.UtcNow)
                return ErrorResponse(
                    $"Wallet is locked until {wallet.PinLockedUntil.Value:O}. Too many failed attempts.",
                    ErrorCodes.Unauthorized,
                    new { lockedUntil = wallet.PinLockedUntil.Value });
            return null;
        }

        private static WalletResponseDTO MapToDTO(Wallet wallet) => new()
        {
            Id = wallet.Id,
            CurrentBalance = wallet.CurrentBalance,
            TotalEarnings = wallet.TotalEarnings,
            Currency = wallet.Currency.ToString(),
            PinIsSet = wallet.PinHash != null,
            PinLockedUntil = wallet.PinLockedUntil
        };

        #endregion
    }
}
