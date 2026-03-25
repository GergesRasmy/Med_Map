
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
        private readonly IMedicineRepository medicineRepository;
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly IPharmacyInventoryRepository pharmacyInventoryRepository;

        public OrdersController(UserManager<ApplicationUser> userManager, IOrderRepository orderRepository, IMedicineRepository medicineRepository
                                ,IPharmacyRepository pharmacyRepository,IPharmacyInventoryRepository pharmacyInventoryRepository)
        {
            this.userManager = userManager;
            this.orderRepository = orderRepository;
            this.medicineRepository = medicineRepository;
            this.pharmacyRepository = pharmacyRepository;
            this.pharmacyInventoryRepository = pharmacyInventoryRepository;
        }
        #endregion
        [Authorize(Roles = RoleConstants.Names.Customer)]
        [HttpPost("place")]                     //api/order/place
        public async Task<IActionResult> createOrder([FromBody] CreateOrderDTO orderDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user.IsActive == false) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration); 

            // Validate Payment Option
            if (!Enum.TryParse<PaymentOptions>(orderDTO.paymentOption, true, out var paymentType))
                return ErrorResponse("Invalid payment option", ErrorCodes.InvalidInput);

            //Map DTO to Order Model
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var location = geometryFactory.CreatePoint(new Coordinate(orderDTO.longitude, orderDTO.latitude));
            var newOrder = new Orders
            {
                CustomerId = userId,
                PharmacyProfileId = orderDTO.pharmacyId,
                DeliveryAddress = location,
                PaymentType = paymentType,
                Status = StatusList.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            //Calculate Total and Add Items
            decimal total = 0;
            var inventoryCache = new Dictionary<Guid, PharmacyInventory>();
            foreach (var item in orderDTO.items)
            {
                var medicine = await medicineRepository.GetByIdAsync(item.medicineId.ToString());
                if (medicine == null) return ErrorResponse("Medicine not found", ErrorCodes.DataNotFound);

                var inventory = await pharmacyInventoryRepository.GetPharmacyMedicineAsync(orderDTO.pharmacyId.ToString(), item.medicineId);
                if (inventory == null)
                    return ErrorResponse("Medicine not in pharmacy inventory", ErrorCodes.DataNotFound);
                if (inventory.StockQuantity < item.quantity)
                    return ErrorResponse($"Not enough stock for {inventory.Medicine.TradeName}", ErrorCodes.InsufficientStock);
                newOrder.OrderItems.Add(new OrderItem
                {
                    MedicineId = item.medicineId,
                    Quantity = item.quantity
                });
                inventoryCache[item.medicineId] = inventory;
                total += item.quantity * medicine.Price;
            }
            newOrder.TotalAmount = total;
            //Save order
            await orderRepository.InsertAsync(newOrder);
            //reduce inventory if order succeeded 
            foreach (var item in orderDTO.items)
            {
                inventoryCache[item.medicineId].StockQuantity -= item.quantity;
            }
            await pharmacyInventoryRepository.SaveChangesAsync();

            var response = MapOrderToResponseDTO(newOrder);
            return SuccessResponse(response, "Order created successfully", SuccessCodes.DataCreated);
        }
        [Authorize]
        [HttpGet("myOrders")]                   //api/order/myOrders
        public async Task<IActionResult> getMyOrders()
        {
            // Extract user ID and role from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var user = await userManager.FindByIdAsync(userId);
            if (user.IsActive == false) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return ErrorResponse("Role not found", ErrorCodes.Unauthorized);

            // Get orders based on user role and user ID
            var orders = await orderRepository.GetAllOrdersAsync(userId,role.ToString());

            if (orders == null || !orders.Any())
                return SuccessResponse(new List<OrderResponseDTO>(), "No orders found", SuccessCodes.DataRetrieved);

            //Map to DTO and return response
            var response = orders.Select(MapOrderToResponseDTO).ToList();
            return SuccessResponse(response, "Orders retrieved successfully", SuccessCodes.DataRetrieved);
        }
        [Authorize]
        [HttpGet]                               // api/order?id=
        public async Task<IActionResult> GetOrderById([FromQuery]string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user.IsActive == false) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            var role = User.FindFirstValue(ClaimTypes.Role);

            var order = await orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return ErrorResponse("Order not found", ErrorCodes.DataNotFound);

            if (role == RoleConstants.Names.Customer && order.CustomerId != userId)
                return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            if (role == RoleConstants.Names.Pharmacy)
            {
                var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
                if (pharmacy?.ActiveProfile?.Id != order.PharmacyProfileId)
                    return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            }
            //Map to DTO and return response
            var response = MapOrderToResponseDTO(order);
            return SuccessResponse<OrderResponseDTO>(response, "Order retrieved successfully", SuccessCodes.DataRetrieved);
        }
        [Authorize]
        [HttpPatch("cancel/{orderId}")]          // api/order/cancel/{orderId}
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return ErrorResponse("Unauthorized access", ErrorCodes.Unauthorized);
            var user = await userManager.FindByIdAsync(userId);
            if (user.IsActive == false) return ErrorResponse("Complete Registration", ErrorCodes.CompleteRegistration);

            var success = await orderRepository.CancelOrder(orderId, userId);

            if (!success)
                return ErrorResponse("Order cannot be cancelled at this stage.", ErrorCodes.InvalidAction);

            return SuccessResponse("Order cancelled successfully", SuccessCodes.DataUpdated);
        }
        // Helper method to map Order to OrderResponseDTO
        private OrderResponseDTO MapOrderToResponseDTO(Orders order)
        {
            return new OrderResponseDTO
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Items = (order.OrderItems ?? new List<OrderItem>()).Select(oi => new OrderItemResponseDTO
                {
                    MedicineName = oi.Medicine?.TradeName ?? "Unknown Medicine",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.Medicine?.Price ?? 0
                }).ToList()
            };
        }


    }
}

