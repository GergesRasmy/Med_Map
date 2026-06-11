using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Med_Map.Controllers
{
    [Route("api/admin/wallet")]
    [ApiController]
    [Authorize(Roles = RoleConstants.Names.Admin)]
    public class AdminWalletController : ResponceBaseController
    {
        private readonly IWalletTransactionRepository transactionRepository;
        private readonly IWalletRepository walletRepository;
        private readonly IUnitOfWork unitOfWork;

        public AdminWalletController(
            IWalletTransactionRepository transactionRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork)
        {
            this.transactionRepository = transactionRepository;
            this.walletRepository = walletRepository;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("withdrawals")]
        [ProducesResponseType(typeof(SuccessResponseDTO<List<WalletTransactionDTO>>), 200)]
        public async Task<IActionResult> GetPendingWithdrawals([FromQuery] int page = 1)
        {
            var transactions = await transactionRepository.GetPendingWithdrawalsAsync(page);
            return SuccessResponse(transactions.Select(MapToDTO).ToList(), "Pending withdrawals retrieved.", SuccessCodes.DataRetrieved);
        }

        [HttpPatch("withdrawals/{id}/complete")]
        [ProducesResponseType(typeof(SuccessResponseDTO<WalletTransactionDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> CompleteWithdrawal(Guid id)
        {
            var transaction = await transactionRepository.GetByIdAsync(id, asNoTracking: false);
            if (transaction == null)
                return ErrorResponse("Transaction not found.", ErrorCodes.DataNotFound);

            if (transaction.Type != TransactionType.Withdrawal || transaction.Status != TransactionStatus.Pending)
                return ErrorResponse("Transaction is not a pending withdrawal.", ErrorCodes.InvalidAction);

            using var tx = await unitOfWork.BeginTransactionAsync();
            try
            {
                transaction.Status = TransactionStatus.Completed;
                transaction.ResolvedAt = DateTime.UtcNow;
                await transactionRepository.SaveChangesAsync();
                await unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                return ErrorResponse("Failed to complete withdrawal.", ErrorCodes.InternalServerError);
            }

            return SuccessResponse(MapToDTO(transaction), "Withdrawal marked as complete.", SuccessCodes.DataUpdated);
        }

        [HttpPatch("withdrawals/{id}/cancel")]
        [ProducesResponseType(typeof(SuccessResponseDTO<WalletTransactionDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> CancelWithdrawal(Guid id, [FromBody] AdminCancelWithdrawalDTO model)
        {
            var transaction = await transactionRepository.GetByIdAsync(id, asNoTracking: false);
            if (transaction == null)
                return ErrorResponse("Transaction not found.", ErrorCodes.DataNotFound);

            if (transaction.Type != TransactionType.Withdrawal || transaction.Status != TransactionStatus.Pending)
                return ErrorResponse("Transaction is not a pending withdrawal.", ErrorCodes.InvalidAction);

            var wallet = await walletRepository.GetByIdAsync(transaction.WalletId);
            if (wallet == null)
                return ErrorResponse("Wallet not found.", ErrorCodes.DataNotFound);

            using var tx = await unitOfWork.BeginTransactionAsync();
            try
            {
                transaction.Status = TransactionStatus.Cancelled;
                transaction.ResolvedAt = DateTime.UtcNow;
                transaction.AdminNote = model.Reason;
                wallet.CurrentBalance += transaction.Amount;

                await transactionRepository.SaveChangesAsync();
                await unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                return ErrorResponse("Failed to cancel withdrawal.", ErrorCodes.InternalServerError);
            }

            return SuccessResponse(MapToDTO(transaction), "Withdrawal cancelled and balance refunded.", SuccessCodes.DataUpdated);
        }

        private static WalletTransactionDTO MapToDTO(WalletTransaction t) => new()
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            Amount = t.Amount,
            Currency = t.Currency.ToString(),
            CreatedAt = t.CreatedAt,
            ResolvedAt = t.ResolvedAt,
            OrderId = t.OrderId,
            AdminNote = t.AdminNote,
            CashoutMethod = t.CashoutMethodJson != null
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(t.CashoutMethodJson)
                : null,
        };
    }
}
