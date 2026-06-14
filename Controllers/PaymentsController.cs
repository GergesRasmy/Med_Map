using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Med_Map.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ResponceBaseController
    {
        #region ctor
        private readonly IPaymentRepository paymentRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IKashierService kashierService;
        private readonly IConfiguration configuration;
        private readonly ILogger<PaymentsController> logger;

        public PaymentsController(
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IKashierService kashierService,
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            this.paymentRepository = paymentRepository;
            this.orderRepository = orderRepository;
            this.kashierService = kashierService;
            this.configuration = configuration;
            this.logger = logger;
        }
        #endregion

        [HttpPost("initiate")]              //api/payments/initiate
        [Authorize]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            if (model.orderIds == null || model.orderIds.Count == 0)
                return ErrorResponse("At least one order ID is required", ErrorCodes.InvalidInput);

            var orders = new List<Orders>();
            decimal totalAmount = 0;

            foreach (var orderId in model.orderIds)
            {
                var order = await orderRepository.GetOrderByIdAsync(orderId.ToString());
                if (order == null)
                    return ErrorResponse($"Order {orderId} not found", ErrorCodes.DataNotFound);
                if (order.CustomerId != userId)
                    return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
                if (order.Status != StatusList.Pending)
                    return ErrorResponse($"Order {orderId} is already processed", ErrorCodes.InvalidAction);
                if (order.PaymentType != PaymentOptions.Online)
                    return ErrorResponse($"Order {orderId} is not an online order", ErrorCodes.InvalidAction);

                var existing = await paymentRepository.GetByOrderIdAsync(orderId);
                if (existing != null)
                {
                    if (existing.Status == PaymentStatus.Paid)
                        return ErrorResponse($"Order {orderId} is already paid", ErrorCodes.InvalidAction);
                    if (existing.Status == PaymentStatus.Pending)
                        return ErrorResponse($"A pending payment already exists for order {orderId}", ErrorCodes.InvalidAction);
                }

                orders.Add(order);
                totalAmount += order.TotalAmount;
            }

            var payment = new Payment
            {
                UserId = userId,
                Amount = totalAmount,
                PaymentProvider = "Kashier",
                Status = PaymentStatus.Pending,
                PaymentOrders = orders.Select(o => new PaymentOrder { OrderId = o.Id }).ToList()
            };
            await paymentRepository.AddAsync(payment);

            try
            {
                // We pass our internal Payment.Id as Kashier's orderId so the webhook can find it again.
                var paymentUrl = kashierService.BuildPaymentUrl(totalAmount, payment.Id.ToString());
                payment.ProviderOrderId = payment.Id.ToString();
                await paymentRepository.SaveChangesAsync();

                return SuccessResponse(new
                {
                    paymentUrl
                }, "Payment initiated successfully", SuccessCodes.DataCreated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Kashier payment URL creation failed for orders {OrderIds}", model.orderIds);
                return ErrorResponse("Payment initiation failed. Please try again.", ErrorCodes.PaymentFailed);
            }
        }

        [HttpGet("status/{orderId}")]              //api/payments/status/{orderId}
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(Guid orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var order = await orderRepository.GetOrderByIdAsync(orderId.ToString());
            if (order == null) return ErrorResponse("Order not found", ErrorCodes.DataNotFound);
            if (order.CustomerId != userId) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var payment = await paymentRepository.GetByOrderIdAsync(orderId);
            if (payment == null) return ErrorResponse("No payment found for this order", ErrorCodes.DataNotFound);

            return SuccessResponse(new
            {
                orderId,
                paymentStatus = payment.Status.ToString(),
                orderIds = payment.PaymentOrders.Select(po => po.OrderId)
            }, "Payment status retrieved", SuccessCodes.DataRetrieved);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var rawPayload = await reader.ReadToEndAsync();

            logger.LogInformation("Kashier webhook received: {Payload}", rawPayload);

            // Kashier posts { "event": "...", "data": { ... , "signatureKeys": [...], "signature": "..." } }
            JsonElement data;
            try
            {
                var root = JsonSerializer.Deserialize<JsonElement>(rawPayload);
                data = root.TryGetProperty("data", out var d) ? d : root;
            }
            catch
            {
                logger.LogWarning("Kashier webhook: could not parse payload");
                return Ok();
            }

            if (!kashierService.VerifyWebhookSignature(data))
            {
                logger.LogWarning("Kashier webhook signature verification failed");
                return Ok();
            }

            try
            {
                // We sent Payment.Id as the orderId; Kashier echoes it back as merchantOrderId.
                var merchantRef = data.TryGetProperty("merchantOrderId", out var m) ? m.GetString()
                                : data.TryGetProperty("orderId", out var o) ? o.GetString()
                                : null;
                if (!Guid.TryParse(merchantRef, out var paymentId))
                {
                    logger.LogWarning("Kashier webhook merchant reference is not a valid GUID: {Ref}", merchantRef);
                    return Ok();
                }

                var payment = await paymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    logger.LogWarning("Webhook received for unknown payment {PaymentId}", paymentId);
                    return Ok();
                }

                var statusStr = data.TryGetProperty("status", out var s) ? s.GetString()
                              : data.TryGetProperty("paymentStatus", out var ps) ? ps.GetString()
                              : null;
                var success = string.Equals(statusStr, "SUCCESS", StringComparison.OrdinalIgnoreCase);

                var transactionId = data.TryGetProperty("transactionId", out var t) ? t.GetString() : null;

                payment.Logs.Add(new PaymentLog
                {
                    PaymentId = payment.Id,
                    Event = success ? "success" : "failed",
                    Payload = rawPayload
                });

                // Idempotency — ignore duplicate webhooks
                if (payment.Status == PaymentStatus.Paid)
                {
                    logger.LogInformation("Duplicate webhook received for payment {PaymentId}", payment.Id);
                    await paymentRepository.SaveChangesAsync();
                    return Ok();
                }

                // Verify the paid amount matches what we recorded.
                decimal paidAmount = 0;
                if (data.TryGetProperty("amount", out var a))
                {
                    paidAmount = a.ValueKind == JsonValueKind.Number
                        ? a.GetDecimal()
                        : decimal.TryParse(a.GetString(), System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var pa) ? pa : 0;
                }

                if (success && paidAmount != payment.Amount)
                {
                    logger.LogWarning("Amount mismatch for payment {PaymentId}. Expected {Expected}, got {Actual}",
                        payment.Id, payment.Amount, paidAmount);
                    await paymentRepository.SaveChangesAsync();
                    return Ok();
                }

                payment.ProviderTransactionId = transactionId;
                payment.UpdatedAt = DateTime.UtcNow;

                if (success)
                {
                    payment.Status = PaymentStatus.Paid;
                    foreach (var po in payment.PaymentOrders)
                    {
                        if (po.Order == null) continue;
                        po.Order.Status = StatusList.Recorded;
                    }
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                }

                await paymentRepository.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Webhook processing failed");
                return Ok();
            }
        }
    }
}
