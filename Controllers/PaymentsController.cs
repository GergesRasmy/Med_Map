using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Med_Map.Controllers
{
    [Route("api/payments")]
    [ApiController]//TODO test
    public class PaymentsController : ResponceBaseController
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IPaymobService paymobService;
        private readonly ILogger<PaymentsController> logger;

        public PaymentsController(IPaymentRepository paymentRepository, IOrderRepository orderRepository,
                                   IPaymobService paymobService, ILogger<PaymentsController> logger)
        {
            this.paymentRepository = paymentRepository;
            this.orderRepository = orderRepository;
            this.paymobService = paymobService;
            this.logger = logger;
        }

        [HttpPost("initiate")]              //api/payments/initiate
        [Authorize]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            // Validate order exists and belongs to user
            var order = await orderRepository.GetOrderByIdAsync(model.orderId.ToString());
            if (order == null) return ErrorResponse("Order not found", ErrorCodes.DataNotFound);
            if (order.CustomerId != userId) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            // Validate order status
            if (order.Status != StatusList.Pending)
                return ErrorResponse("Order is already processed", ErrorCodes.InvalidAction);

            // Check for existing payment
            var existingPayment = await paymentRepository.GetByOrderIdAsync(order.Id);
            if (existingPayment != null)
            {
                if (existingPayment.Status == PaymentStatus.Paid)
                    return ErrorResponse("Order is already paid", ErrorCodes.InvalidAction);
                if (existingPayment.Status == PaymentStatus.Pending)
                    return ErrorResponse("A pending payment already exists for this order", ErrorCodes.InvalidAction);
            }

            // Create payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                UserId = userId,
                Amount = order.TotalAmount,
                Status = PaymentStatus.Pending
            };
            await paymentRepository.AddAsync(payment);

            // Get payment URL from Paymob
            try
            {
                var (paymentUrl, providerOrderId) = await paymobService.CreatePaymentUrlAsync(order.TotalAmount, payment.Id.ToString());
                payment.ProviderOrderId = providerOrderId;
                await paymentRepository.SaveChangesAsync();
                return SuccessResponse(new { paymentUrl }, "Payment initiated successfully", SuccessCodes.DataCreated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Paymob payment initiation failed for order {OrderId}", order.Id);
                return ErrorResponse("Payment initiation failed. Please try again.", ErrorCodes.PaymentFailed);
            }
        }

        [HttpGet("status/{orderId}")]              //api/payments/status/{orderId}
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(Guid orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            // Validate order exists and belongs to user
            var order = await orderRepository.GetOrderByIdAsync(orderId.ToString());
            if (order == null) return ErrorResponse("Order not found", ErrorCodes.DataNotFound);
            if (order.CustomerId != userId) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            // Get payment status
            var payment = await paymentRepository.GetByOrderIdAsync(orderId);
            if (payment == null) return ErrorResponse("No payment found for this order", ErrorCodes.DataNotFound);

            return SuccessResponse(new
            {
                orderId,
                paymentStatus = payment.Status.ToString()
            }, "Payment status retrieved", SuccessCodes.DataRetrieved);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            // Read raw payload
            using var reader = new StreamReader(Request.Body);
            var rawPayload = await reader.ReadToEndAsync();

            // Log immediately
            logger.LogInformation("Paymob webhook received: {Payload}", rawPayload);

            // Verify HMAC signature
            var hmac = Request.Query["hmac"].ToString();
            if (!paymobService.VerifySignature(rawPayload, hmac))
            {
                logger.LogWarning("Paymob webhook signature verification failed");
                return Ok();
            }

            try
            {
                var payload = JsonSerializer.Deserialize<JsonElement>(rawPayload);
                var obj = payload.GetProperty("obj");
                var providerOrderId = obj.GetProperty("order").GetProperty("id").GetInt32().ToString();
                var transactionId = obj.GetProperty("id").GetInt32().ToString();
                var amountCents = obj.GetProperty("amount_cents").GetInt32();
                var success = obj.GetProperty("success").GetBoolean();

                // Find payment
                var payment = await paymentRepository.GetByProviderOrderIdAsync(providerOrderId);
                if (payment == null)
                {
                    logger.LogWarning("Webhook received for unknown provider order {ProviderOrderId}", providerOrderId);
                    return Ok();
                }

                // Log the event
                payment.Logs.Add(new PaymentLog
                {
                    PaymentId = payment.Id,
                    Event = success ? "success" : "failed",
                    Payload = rawPayload
                });

                // Idem potency check to prevent duplicate payments
                if (payment.Status == PaymentStatus.Paid)
                {
                    logger.LogInformation("Duplicate webhook received for payment {PaymentId}", payment.Id);
                    await paymentRepository.SaveChangesAsync();
                    return Ok();
                }

                // Verify amount
                if (amountCents != (int)(payment.Amount * 100))
                {
                    logger.LogWarning("Amount mismatch for payment {PaymentId}. Expected {Expected}, got {Actual}",
                        payment.Id, (int)(payment.Amount * 100), amountCents);
                    await paymentRepository.SaveChangesAsync();
                    return Ok();
                }

                // Update payment and order
                payment.ProviderTransactionId = transactionId;
                payment.UpdatedAt = DateTime.UtcNow;

                var order = await orderRepository.GetOrderByIdAsync(payment.OrderId.ToString());
                if (success)
                {
                    payment.Status = PaymentStatus.Paid;
                    if (order != null) order.Status = StatusList.Confirmed;
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