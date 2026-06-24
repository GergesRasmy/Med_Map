using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Med_Map.Controllers
{
    [Route("api/pharmacyService")]
    [ApiController]
    public class PharmacyServiceController : ResponceBaseController
    {
        #region ctor
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly IPharmacyServiceRepository pharmacyServiceRepository;

        public PharmacyServiceController(IPharmacyRepository pharmacyRepository, IPharmacyServiceRepository pharmacyServiceRepository)
        {
            this.pharmacyRepository = pharmacyRepository;
            this.pharmacyServiceRepository = pharmacyServiceRepository;
        }
        #endregion

        [Authorize(Roles = RoleConstants.Names.Pharmacy)]
        [HttpPost("add")]                       // POST /api/pharmacyService/add
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> Add([FromBody] AddPharmacyServiceDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy?.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found or inactive", ErrorCodes.UserNotFound);

            var service = new PharmacyService
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                PharmacyUserId = userId
            };

            try
            {
                await pharmacyServiceRepository.AddAsync(service);
                return SuccessResponse("Service added successfully", SuccessCodes.DataCreated);
            }
            catch (Exception ex)
            {
                return ErrorResponse("Failed to add service", ErrorCodes.DataBaseError, ex.Message);
            }
        }

        [Authorize(Roles = RoleConstants.Names.Pharmacy)]
        [HttpPatch("update")]                   // PATCH /api/pharmacyService/update
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> Update([FromBody] UpdatePharmacyServiceDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy?.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found or inactive", ErrorCodes.UserNotFound);

            if (model.Name == null && model.Description == null && !model.Price.HasValue && !model.IsActive.HasValue)
                return ErrorResponse("No fields provided to update.", ErrorCodes.ValidationError);

            var service = await pharmacyServiceRepository.GetByIdForPharmacyAsync(model.Id, userId);
            if (service == null)
                return ErrorResponse("Service not found", ErrorCodes.DataNotFound);

            if (model.Name != null) service.Name = model.Name;
            if (model.Description != null) service.Description = model.Description;
            if (model.Price.HasValue) service.Price = model.Price.Value;
            if (model.IsActive.HasValue) service.IsActive = model.IsActive.Value;

            try
            {
                await pharmacyServiceRepository.SaveChangesAsync();
                return SuccessResponse("Service updated successfully", SuccessCodes.DataUpdated);
            }
            catch (Exception ex)
            {
                return ErrorResponse("Failed to update service", ErrorCodes.DataBaseError, ex.Message);
            }
        }

        [Authorize(Roles = RoleConstants.Names.Pharmacy)]
        [HttpDelete("delete")]                  // DELETE /api/pharmacyService/delete?id=
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> Delete([FromQuery] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy?.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found or inactive", ErrorCodes.UserNotFound);

            var success = await pharmacyServiceRepository.DeleteAsync(id, userId);
            if (!success)
                return ErrorResponse("Service not found", ErrorCodes.DataNotFound);

            return SuccessResponse("Service deleted successfully", SuccessCodes.DataDeleted);
        }

        [Authorize(Roles = RoleConstants.Names.Pharmacy)]
        [HttpGet("myServices")]                 // GET /api/pharmacyService/myServices?page=1&pageSize=10
        [ProducesResponseType(typeof(SuccessResponseDTO<PagedDTO<PharmacyServiceResponseDTO>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> MyServices([FromQuery] int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy?.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found or inactive", ErrorCodes.UserNotFound);

            var (items, totalCount) = await pharmacyServiceRepository.GetByPharmacyAsync(userId, page, pageSize);

            return SuccessResponse(ToPagedDTO(items, totalCount, page, pageSize), "Services retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [AllowAnonymous]
        [HttpGet("search")]                     // GET /api/pharmacyService/search?query=&pharmacyId=&page=1&pageSize=10
        [ProducesResponseType(typeof(SuccessResponseDTO<PagedDTO<PharmacyServiceResponseDTO>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> Search(
            [FromQuery] string? query = null,
            [FromQuery] string? pharmacyId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var (items, totalCount) = await pharmacyServiceRepository.SearchAsync(
                query?.Trim(), pharmacyId, activeOnly: true, page, pageSize);

            return SuccessResponse(ToPagedDTO(items, totalCount, page, pageSize), "Services retrieved successfully", SuccessCodes.DataRetrieved);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]                       // GET /api/pharmacyService/{id}
        [ProducesResponseType(typeof(SuccessResponseDTO<PharmacyServiceResponseDTO>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var service = await pharmacyServiceRepository.GetByIdAsync(id);
            if (service == null)
                return ErrorResponse("Service not found", ErrorCodes.DataNotFound);

            return SuccessResponse(MapToDTO(service), "Service retrieved successfully", SuccessCodes.DataRetrieved);
        }

        #region helpers
        private static PharmacyServiceResponseDTO MapToDTO(PharmacyService s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Price = s.Price,
            IsActive = s.IsActive,
            PharmacyUserId = s.PharmacyUserId,
            PharmacyName = s.Pharmacy?.ActiveProfile?.PharmacyName
        };

        private static PagedDTO<PharmacyServiceResponseDTO> ToPagedDTO(
            List<PharmacyService> items, int totalCount, int page, int pageSize) => new()
        {
            currentPage = page,
            pageSize = pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            totalCount = totalCount,
            items = items.Select(MapToDTO).ToList()
        };
        #endregion
    }
}
