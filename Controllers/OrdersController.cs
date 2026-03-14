using Med_Map.DTO.OrdersDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite;
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
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IOrderRepository orderRepository;
        private readonly IMedicineRepository medicineRepository;

        public OrdersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOrderRepository orderRepository, IMedicineRepository medicineRepository)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.orderRepository = orderRepository;
            this.medicineRepository = medicineRepository;
        }
        #endregion
        [HttpPost("place")]                     //api/order/place
        public async Task<IActionResult> createOrder([FromBody] CreateOrderDTO orderDTO)
        {
            if(!ModelState.IsValid) { 
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return ErrorResponse("Validation failed", ErrorCodes.ValidationError, errors);
            }
            //Parse the Enum (Validate Payment Option)
            if (!Enum.TryParse<PaymentOptions>(orderDTO.paymentOption, true, out var paymentType))
            {
                return ErrorResponse("Invalid payment option", ErrorCodes.InvalidInput);
            }

            //Map DTO to Order Model
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var location = geometryFactory.CreatePoint(new Coordinate(orderDTO.longitude, orderDTO.latitude));
            var newOrder = new Orders
            {
                CustomerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                PharmacyId = orderDTO.pharmacyId,
                DeliveryAddress = location,
                PaymentType = paymentType,
                Status = StatusList.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            //Calculate Total and Add Items
            decimal total = 0;
            foreach (var item in orderDTO.items)
            {
                var medicine = await medicineRepository.GetByIdAsync(item.medicineId.ToString());
                if (medicine == null) return ErrorResponse("Medicine not found", ErrorCodes.DataNotFound);

                newOrder.OrderItems.Add(new OrderItem
                {
                    MedicineId = item.medicineId,
                    Quantity = item.quantity
                });
                total += item.quantity * medicine.Price;
            }
            newOrder.TotalAmount = total;

            //Save to Database and Return Response
            await orderRepository.InsertAsync(newOrder);
            var response = new OrderResponseDTO
            {
                Id = newOrder.Id,
                CreatedAt = newOrder.CreatedAt,
                Status = newOrder.Status.ToString(),
                TotalAmount = newOrder.TotalAmount,
                Items = newOrder.OrderItems.Select(oi => new OrderItemResponseDTO
                {
                    MedicineName = oi.Medicine.TradeName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.Medicine.Price
                }).ToList()
            };
            return SuccessResponse(response, "Order created successfully", SuccessCodes.DataCreated);
        }
        [HttpGet("myOrders")]                   //api/order/myOrders
        public async Task<IActionResult> getMyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var orders = await orderRepository.GetAllOrdersAsync(userId,role.ToString());
            if (string.IsNullOrEmpty(role)) return ErrorResponse("Role not found", ErrorCodes.Unauthorized);

            if (orders == null || !orders.Any())
            {
                return SuccessResponse(new List<OrderResponseDTO>(), "No orders found", SuccessCodes.DataRetrieved);
            }
            var response = orders.Select(o => new OrderResponseDTO
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDTO
                {
                    MedicineName = oi.Medicine.TradeName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.Medicine.Price
                }).ToList()
            }).ToList();
            return SuccessResponse(response, "Orders retrieved successfully", SuccessCodes.DataRetrieved);
        }
        [HttpGet]                               // api/order?id=
        public async Task<IActionResult> GetOrderById([FromQuery]string id)
        {
            //get the order from the database
            var order = await orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return ErrorResponse("Order not found", ErrorCodes.DataNotFound);
            }

            //Map to DTO
            var response = new OrderResponseDTO
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemResponseDTO
                {
                    MedicineName = oi.Medicine.TradeName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.Medicine.Price
                }).ToList()
            };

            return SuccessResponse<OrderResponseDTO>(response, "Order retrieved successfully", SuccessCodes.DataRetrieved);
        }
        [HttpPost("cancel/{orderId}")]          // api/order/cancel/{orderId}
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            var success = await orderRepository.CancelOrder(orderId);

            if (!success)
                return ErrorResponse("Order cannot be cancelled at this stage.", ErrorCodes.InvalidAction);

            return SuccessResponse("Order cancelled successfully", SuccessCodes.DataUpdated);
        }
    }
}

