
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite;
using NetTopologySuite.Index.HPRtree;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Med_Map.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrdersController : ResponceBaseController
    {
        #region ctor
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOrderRepository orderRepository;
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly IPharmacyInventoryRepository pharmacyInventoryRepository;
        private readonly IPharmacyServiceRepository pharmacyServiceRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IWalletRepository walletRepository;
        private readonly IWalletTransactionRepository walletTransactionRepository;
        private readonly IHubContext<NotificationHub, INotificationClient> hub;

        public OrdersController(UserManager<ApplicationUser> userManager, IOrderRepository orderRepository, IPharmacyRepository pharmacyRepository,
                                IPharmacyInventoryRepository pharmacyInventoryRepository, IPharmacyServiceRepository pharmacyServiceRepository,
                                IUnitOfWork unitOfWork, IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository,
                                IHubContext<NotificationHub, INotificationClient> hub)
        {
            this.userManager = userManager;
            this.orderRepository = orderRepository;
            this.pharmacyRepository = pharmacyRepository;
            this.pharmacyInventoryRepository = pharmacyInventoryRepository;
            this.pharmacyServiceRepository = pharmacyServiceRepository;
            this.unitOfWork = unitOfWork;
            this.walletRepository = walletRepository;
            this.walletTransactionRepository = walletTransactionRepository;
            this.hub = hub;
        }
        #endregion

        [Authorize(Roles = RoleConstants.Names.Customer)]
        [HttpPost("place")]                     //api/order/place
        [ProducesResponseType(typeof(SuccessResponseDTO<OrderResponseDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> createOrder([FromBody] CreateOrderDTO orderDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);
            if (!user.IsActive) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            if (!Enum.TryParse<PaymentOptions>(orderDTO.paymentOption, true, out var paymentType))
                return ErrorResponse("Invalid payment option", ErrorCodes.InvalidInput);
            if (!Enum.TryParse<FulfillmentType>(orderDTO.fulfillmentType, true, out var fulfillment))
                return ErrorResponse("Invalid fulfillment type", ErrorCodes.InvalidInput);

            if (orderDTO.items == null || !orderDTO.items.Any())
                return ErrorResponse("Order must contain at least one item", ErrorCodes.ValidationError);

            // Enforce no mixed orders
            var isServiceOrder = orderDTO.items.All(i => i.type == "service");
            var isMedicineOrder = orderDTO.items.All(i => i.type == "medicine");
            if (!isServiceOrder && !isMedicineOrder)
                return ErrorResponse("An order cannot mix medicine and service items", ErrorCodes.ValidationError);

            if (isServiceOrder && fulfillment != FulfillmentType.ServiceAtPharmacy && fulfillment != FulfillmentType.ServiceAtHome)
                return ErrorResponse("Service orders must use ServiceAtPharmacy or ServiceAtHome fulfillment", ErrorCodes.ValidationError);

            if (isMedicineOrder && (fulfillment == FulfillmentType.ServiceAtPharmacy || fulfillment == FulfillmentType.ServiceAtHome))
                return ErrorResponse("Medicine orders cannot use service fulfillment types", ErrorCodes.ValidationError);

            if (fulfillment == FulfillmentType.Delivery && string.IsNullOrWhiteSpace(orderDTO.deliveryAddress))
                return ErrorResponse("Delivery address is required for delivery orders", ErrorCodes.ValidationError);

            if (fulfillment == FulfillmentType.ServiceAtHome && string.IsNullOrWhiteSpace(orderDTO.deliveryAddress))
                return ErrorResponse("Address is required for home service orders", ErrorCodes.ValidationError);

            var targetPharmacy = await pharmacyRepository.GetByIdAsync(orderDTO.pharmacyId);
            if (targetPharmacy?.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found or has no active profile", ErrorCodes.UserNotFound);

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var location = geometryFactory.CreatePoint(new Coordinate(orderDTO.deliveryLongitude, orderDTO.deliveryLatitude));
            var newOrder = new Orders
            {
                CustomerId = userId,
                PharmacyUserId = orderDTO.pharmacyId,
                DeliveryAddress = location,
                PhoneNumber = orderDTO.phoneNumber,
                DeliveryAddressText = orderDTO.deliveryAddress,
                PaymentType = paymentType,
                Status = paymentType == PaymentOptions.Online ? StatusList.Pending : StatusList.Recorded,
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>(),
                FulfillmentType = fulfillment
            };

            decimal itemsSubtotal = 0;
            var inventoryCache = new Dictionary<Guid, PharmacyInventory>();

            // Validate everything BEFORE opening the transaction
            if (isMedicineOrder)
            {
                foreach (var item in orderDTO.items)
                {
                    if (item.medicineId == null)
                        return ErrorResponse("medicineId is required for medicine items", ErrorCodes.ValidationError);

                    var inventory = await pharmacyInventoryRepository
                        .GetPharmacyMedicineAsync(orderDTO.pharmacyId, item.medicineId.Value);
                    if (inventory == null)
                        return ErrorResponse("Medicine not in pharmacy inventory", ErrorCodes.DataNotFound);
                    if (inventory.StockQuantity < item.quantity)
                        return ErrorResponse($"Not enough stock for {inventory.Medicine.TradeName}", ErrorCodes.InsufficientStock);

                    newOrder.OrderItems.Add(new OrderItem
                    {
                        MedicineId = item.medicineId.Value,
                        Quantity = item.quantity,
                        unitPrice = inventory.Price
                    });
                    inventoryCache[item.medicineId.Value] = inventory;
                    itemsSubtotal += item.quantity * inventory.Price;
                }
            }
            else // service order
            {
                foreach (var item in orderDTO.items)
                {
                    if (item.serviceId == null)
                        return ErrorResponse("serviceId is required for service items", ErrorCodes.ValidationError);

                    var service = await pharmacyServiceRepository.GetByIdForPharmacyAsync(item.serviceId.Value, orderDTO.pharmacyId);
                    if (service == null || !service.IsActive)
                        return ErrorResponse("Service not available at this pharmacy", ErrorCodes.DataNotFound);

                    newOrder.OrderItems.Add(new OrderItem
                    {
                        ServiceId = item.serviceId.Value,
                        Service = service,     // populate nav so the response has the service name
                        Quantity = item.quantity,
                        unitPrice = service.Price
                    });
                    itemsSubtotal += item.quantity * service.Price;
                }
            }

            var deliveryFee = fulfillment == FulfillmentType.Delivery
                ? (targetPharmacy.ActiveProfile.HaveDelivary ? targetPharmacy.ActiveProfile.DeliveryFee : 0)
                : 0;
            var paymentFee = paymentType == PaymentOptions.Cash ? Constant.CashOnDeliveryFee : Constant.OnlineFee;
            var appFee = Constant.AppFee;

            newOrder.ItemsSubtotal = itemsSubtotal;
            newOrder.DeliveryFee = deliveryFee;
            newOrder.PaymentFee = paymentFee;
            newOrder.AppFee = appFee;
            newOrder.TotalAmount = itemsSubtotal + deliveryFee + paymentFee + appFee;

            // Only writes go inside the transaction
            using var transaction = await unitOfWork.BeginTransactionAsync();
            try
            {
                orderRepository.Insert(newOrder);

                if (isMedicineOrder)
                {
                    foreach (var item in orderDTO.items)
                        inventoryCache[item.medicineId!.Value].StockQuantity -= item.quantity;
                    await pharmacyInventoryRepository.SaveChangesAsync();
                }
                else
                {
                    await orderRepository.SaveChangesAsync();
                }

                await unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                return ErrorResponse("Failed to place order", ErrorCodes.DataBaseError, ex.Message);
            }

            if (paymentType == PaymentOptions.Cash)
                await hub.Clients.User(orderDTO.pharmacyId).OrderPlaced(new OrderPlacedPayload(
                    newOrder.Id, userId, newOrder.TotalAmount, orderDTO.items.Count,
                    fulfillment.ToString(), newOrder.CreatedAt));

            if (isMedicineOrder)
            {
                var stockChanges = orderDTO.items
                    .Select(i => new StockChange(i.medicineId!.Value, inventoryCache[i.medicineId.Value].Medicine?.TradeName ?? string.Empty, inventoryCache[i.medicineId.Value].StockQuantity))
                    .ToList();
                await hub.Clients.User(orderDTO.pharmacyId).InventoryStockChanged(new InventoryStockChangedPayload(stockChanges));
            }

            return SuccessResponse(MapOrderToResponseDTO(newOrder), "Order created successfully", SuccessCodes.DataCreated);
        }

        [Authorize(Roles = RoleConstants.Names.Customer)]
        [HttpPost("validate-cart")]             //api/order/validate-cart
        [ProducesResponseType(typeof(SuccessResponseDTO<CartValidationResponseDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> ValidateCart([FromBody] List<CartPharmacyValidationDTO> carts)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);
            if (!user.IsActive) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            if (carts == null || !carts.Any())
                return ErrorResponse("Cart must contain at least one pharmacy", ErrorCodes.ValidationError);

            var response = new CartValidationResponseDTO
            {
                fees = new CartFeesDTO
                {
                    appFee = Constant.AppFee,
                    cashOnDeliveryFee = Constant.CashOnDeliveryFee,
                    onlineFee = Constant.OnlineFee
                }
            };

            foreach (var cart in carts)
            {
                var result = new PharmacyCartValidationResultDTO { pharmacyId = cart.pharmacyId };

                var pharmacy = await pharmacyRepository.GetByIdAsync(cart.pharmacyId);
                if (pharmacy?.ActiveProfile == null)
                {
                    result.found = false;
                    result.isValid = false;
                    foreach (var item in cart.items ?? new List<CartItemValidationDTO>())
                        result.items.Add(new CartItemValidationResultDTO
                        {
                            type = item.type,
                            medicineId = item.medicineId,
                            serviceId = item.serviceId,
                            tradeName = item.tradeName,
                            genericName = item.genericName,
                            serviceName = item.serviceName,
                            previousUnitPrice = item.unitPrice,
                            priceUnitIsoCode = item.priceUnitIsoCode,
                            requestedQuantity = item.quantity,
                            availableQuantity = 0,
                            isAvailable = false,
                            lineTotal = 0,
                            message = "Pharmacy not found or unavailable"
                        });
                    response.pharmacies.Add(result);
                    continue;
                }

                result.found = true;
                result.pharmacyName = pharmacy.ActiveProfile.PharmacyName;
                result.deliveryAvailable = pharmacy.ActiveProfile.HaveDelivary;
                result.deliveryFee = pharmacy.ActiveProfile.HaveDelivary ? pharmacy.ActiveProfile.DeliveryFee : 0;

                bool allItemsOk = true;
                decimal subtotal = 0;

                foreach (var item in cart.items ?? new List<CartItemValidationDTO>())
                {
                    if (item.type == "service")
                    {
                        var itemResult = new CartItemValidationResultDTO
                        {
                            type = "service",
                            serviceId = item.serviceId,
                            requestedQuantity = item.quantity,
                            previousUnitPrice = item.unitPrice,
                            priceUnitIsoCode = item.priceUnitIsoCode,
                            serviceName = item.serviceName
                        };

                        if (item.serviceId == null)
                        {
                            itemResult.isAvailable = false;
                            itemResult.message = "serviceId is required";
                            allItemsOk = false;
                        }
                        else
                        {
                            var service = await pharmacyServiceRepository.GetByIdForPharmacyAsync(item.serviceId.Value, cart.pharmacyId);
                            if (service == null || !service.IsActive)
                            {
                                itemResult.isAvailable = false;
                                itemResult.availableQuantity = 0;
                                itemResult.lineTotal = 0;
                                itemResult.message = "Service no longer available at this pharmacy";
                                allItemsOk = false;
                            }
                            else
                            {
                                itemResult.serviceName = service.Name;
                                itemResult.currentUnitPrice = service.Price;
                                itemResult.priceChanged = item.unitPrice.HasValue && item.unitPrice.Value != service.Price;
                                itemResult.availableQuantity = item.quantity; // services have no stock limit
                                itemResult.isAvailable = true;
                                itemResult.lineTotal = service.Price * item.quantity;
                                subtotal += itemResult.lineTotal;
                                if (itemResult.priceChanged) itemResult.message = "Price updated";
                            }
                        }

                        result.items.Add(itemResult);
                    }
                    else // medicine
                    {
                        var itemResult = new CartItemValidationResultDTO
                        {
                            type = "medicine",
                            medicineId = item.medicineId,
                            requestedQuantity = item.quantity,
                            previousUnitPrice = item.unitPrice,
                            priceUnitIsoCode = item.priceUnitIsoCode,
                            tradeName = item.tradeName,
                            genericName = item.genericName
                        };

                        if (item.medicineId == null)
                        {
                            itemResult.isAvailable = false;
                            itemResult.message = "medicineId is required";
                            allItemsOk = false;
                        }
                        else
                        {
                            var inventory = await pharmacyInventoryRepository
                                .GetPharmacyMedicineWithDetailsAsync(cart.pharmacyId, item.medicineId.Value);

                            if (inventory == null)
                            {
                                itemResult.availableQuantity = 0;
                                itemResult.isAvailable = false;
                                itemResult.lineTotal = 0;
                                itemResult.message = "No longer available at this pharmacy";
                                allItemsOk = false;
                            }
                            else
                            {
                                itemResult.tradeName = inventory.Medicine?.TradeName ?? item.tradeName;
                                itemResult.genericName = inventory.Medicine?.GenericName ?? item.genericName;
                                itemResult.currentUnitPrice = inventory.Price;
                                itemResult.priceChanged = item.unitPrice.HasValue && item.unitPrice.Value != inventory.Price;
                                itemResult.availableQuantity = inventory.StockQuantity;
                                itemResult.isAvailable = inventory.StockQuantity >= item.quantity;
                                itemResult.lineTotal = itemResult.isAvailable ? inventory.Price * item.quantity : 0;

                                if (!itemResult.isAvailable)
                                {
                                    itemResult.message = inventory.StockQuantity == 0
                                        ? "Out of stock"
                                        : $"Only {inventory.StockQuantity} in stock";
                                    allItemsOk = false;
                                }
                                else
                                {
                                    subtotal += itemResult.lineTotal;
                                    if (itemResult.priceChanged) itemResult.message = "Price updated";
                                }
                            }
                        }

                        result.items.Add(itemResult);
                    }
                }

                result.subtotal = subtotal;
                result.isValid = allItemsOk;
                response.pharmacies.Add(result);
            }

            return SuccessResponse(response, "Cart validated", SuccessCodes.DataRetrieved);
        }

        [Authorize(Roles = RoleConstants.Names.Pharmacy)]
        [HttpPatch("update-status")]         //api/order/update-status
        [ProducesResponseType(typeof(SuccessResponseDTO<OrderResponseDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderDTO orderDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy?.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);
            var order = await orderRepository.GetOrderByIdAsync(orderDTO.orderId);
            if (order == null) return ErrorResponse("Order not found", ErrorCodes.DataNotFound);
            if (order.PharmacyUserId != userId)
                return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var transitions = order.FulfillmentType switch
            {
                FulfillmentType.Delivery          => _deliveryTransitions,
                FulfillmentType.Pickup            => _pickupTransitions,
                FulfillmentType.ServiceAtPharmacy => _serviceAtPharmacyTransitions,
                FulfillmentType.ServiceAtHome     => _serviceAtHomeTransitions,
                _                                 => _deliveryTransitions
            };

            if (!Enum.TryParse<StatusList>(orderDTO.nextStatus, true, out var nextStatus))
                return ErrorResponse("Invalid status value", ErrorCodes.InvalidInput);

            if (!transitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(nextStatus))
                return ErrorResponse($"Cannot transition from {order.Status} to {orderDTO.nextStatus} for a {order.FulfillmentType} order", ErrorCodes.InvalidAction);

            order.Status = nextStatus;
            await orderRepository.UpdateStatusAsync(order.Id, nextStatus, nextStatus == StatusList.Delivered ? DateTime.UtcNow : null);

            var statusPayload = new OrderStatusChangedPayload(order.Id, nextStatus.ToString(), order.FulfillmentType.ToString());
            await hub.Clients.User(order.CustomerId).OrderStatusChanged(statusPayload);
            await hub.Clients.User(userId).OrderStatusChanged(statusPayload);

            if (nextStatus == StatusList.Delivered && order.PaymentType == PaymentOptions.Online)
            {
                var wallet = await walletRepository.GetByPharmacyUserIdAsync(userId);
                if (wallet == null)
                    return ErrorResponse("Pharmacy wallet not found. Contact support.", ErrorCodes.DataNotFound);

                var pharmacyShare = order.ItemsSubtotal + order.DeliveryFee;
                await walletTransactionRepository.DepositAsync(wallet.Id, pharmacyShare, wallet.Currency, order.Id);

                await hub.Clients.User(userId).WalletDeposited(
                    new WalletDepositedPayload(wallet.Id, pharmacyShare, wallet.Currency.ToString(), order.Id));
            }

            return SuccessResponse(MapOrderToResponseDTO(order), "Status updated successfully", SuccessCodes.DataUpdated);
        }

        [Authorize]
        [HttpGet("stats")]                      //api/order/stats
        [ProducesResponseType(typeof(SuccessResponseDTO<OrderStatsDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> GetOrderStats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);
            if (!user.IsActive) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return ErrorResponse("Role not found", ErrorCodes.Unauthorized);

            var stats = await orderRepository.GetOrderStatsAsync(userId, role);
            return SuccessResponse(stats, "Stats retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [Authorize]
        [HttpGet("myOrders")]                   //api/order/myOrders?page=1&pageSize=10&status=Recorded
        [ProducesResponseType(typeof(SuccessResponseDTO<PagedDTO<OrderResponseDTO>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> getMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);
            if (user.IsActive == false) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return ErrorResponse("Role not found", ErrorCodes.Unauthorized);

            StatusList? statusFilter = null;
            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse<StatusList>(status, true, out var parsed))
                    return ErrorResponse($"Invalid status value. Valid values: {string.Join(", ", Enum.GetNames<StatusList>())}", ErrorCodes.InvalidInput);
                statusFilter = parsed;
            }

            var (orders, totalCount) = await orderRepository.GetAllOrdersAsync(userId, role, page, pageSize, statusFilter);

            var response = new PagedDTO<OrderResponseDTO>
            {
                currentPage = page,
                pageSize = pageSize,
                totalCount = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                items = orders.Select(MapOrderToResponseDTO).ToList()
            };

            return SuccessResponse(response, "Orders retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [Authorize]
        [HttpGet]                               // api/order?id=
        [ProducesResponseType(typeof(SuccessResponseDTO<OrderResponseDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> GetOrderById([FromQuery] string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);
            if (user.IsActive == false) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            var role = User.FindFirstValue(ClaimTypes.Role);

            var order = await orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return ErrorResponse("Order not found", ErrorCodes.DataNotFound);

            if (role == RoleConstants.Names.Customer && order.CustomerId != userId)
                return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            if (role == RoleConstants.Names.Pharmacy && order.PharmacyUserId != userId)
                return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            return SuccessResponse<OrderResponseDTO>(MapOrderToResponseDTO(order), "Order retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [Authorize]
        [HttpPatch("cancel/{orderId}")]          // api/order/cancel/{orderId}
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return ErrorResponse("Unauthorized access", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return ErrorResponse("User not found", ErrorCodes.UserNotFound);
            if (user.IsActive == false) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            var order = await orderRepository.GetOrderByIdAsync(orderId);
            if (order == null) return ErrorResponse("Order not found", ErrorCodes.DataNotFound);

            var success = await orderRepository.CancelOrder(orderId, userId);

            if (!success)
                return ErrorResponse("Order cannot be cancelled at this stage.", ErrorCodes.InvalidAction);

            await hub.Clients.User(order.PharmacyUserId).OrderCancelled(
                new OrderCancelledPayload(order.Id, userId));

            return SuccessResponse("Order cancelled successfully", SuccessCodes.DataUpdated);
        }

        #region helpers
        private static OrderResponseDTO MapOrderToResponseDTO(Orders order)
        {
            return new OrderResponseDTO
            {
                id = order.Id,
                createdAt = order.CreatedAt,
                status = order.Status.ToString(),
                itemsSubtotal = order.ItemsSubtotal,
                deliveryFee = order.DeliveryFee,
                paymentFee = order.PaymentFee,
                appFee = order.AppFee,
                totalAmount = order.TotalAmount,
                items = (order.OrderItems ?? new List<OrderItem>()).Select(oi => oi.ServiceId.HasValue
                    ? new OrderItemResponseDTO
                    {
                        type = "service",
                        serviceId = oi.ServiceId,
                        serviceName = oi.Service?.Name ?? "Unknown Service",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.unitPrice
                    }
                    : new OrderItemResponseDTO
                    {
                        type = "medicine",
                        medicineId = oi.MedicineId,
                        medicineName = oi.Medicine?.TradeName ?? "Unknown Medicine",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.unitPrice
                    }).ToList(),
                fulfillmentType = order.FulfillmentType.ToString(),
                phoneNumber = order.PhoneNumber,
                deliveryAddress = order.DeliveryAddressText
            };
        }

        private readonly Dictionary<StatusList, List<StatusList>> _deliveryTransitions = new()
        {
            { StatusList.Recorded,        new() { StatusList.Packaged, StatusList.Canceled } },
            { StatusList.Packaged,        new() { StatusList.OutForDelivery, StatusList.Canceled } },
            { StatusList.OutForDelivery,  new() { StatusList.Delivered, StatusList.Canceled } },
        };

        private readonly Dictionary<StatusList, List<StatusList>> _pickupTransitions = new()
        {
            { StatusList.Recorded,        new() { StatusList.Packaged, StatusList.Canceled } },
            { StatusList.Packaged,        new() { StatusList.ReadyForPickup, StatusList.Canceled } },
            { StatusList.ReadyForPickup,  new() { StatusList.Delivered } },
        };

        // Recorded → Confirmed → Delivered
        private readonly Dictionary<StatusList, List<StatusList>> _serviceAtPharmacyTransitions = new()
        {
            { StatusList.Recorded,   new() { StatusList.Confirmed, StatusList.Canceled } },
            { StatusList.Confirmed,  new() { StatusList.Delivered, StatusList.Canceled } },
        };

        // Recorded → Confirmed → OutForDelivery (en route to customer) → Delivered
        private readonly Dictionary<StatusList, List<StatusList>> _serviceAtHomeTransitions = new()
        {
            { StatusList.Recorded,        new() { StatusList.Confirmed, StatusList.Canceled } },
            { StatusList.Confirmed,       new() { StatusList.OutForDelivery, StatusList.Canceled } },
            { StatusList.OutForDelivery,  new() { StatusList.Delivered } },
        };
        #endregion
    }
}
