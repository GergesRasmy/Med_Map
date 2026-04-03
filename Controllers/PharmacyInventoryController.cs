using Med_Map.DTO.CustomerDTOs;
using Med_Map.DTO.PharmacyInventoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Med_Map.Controllers
{
    [Route("api/pharmacyInventory")]
    [ApiController]
    [Authorize(Roles =RoleConstants.Names.Pharmacy)]
    public class PharmacyInventoryController : ResponceBaseController
    { 
        #region ctor
        private readonly IPharmacyRepository pharmacyRepository;
        private readonly IMedicineRepository medicineRepository;
        private readonly IPharmacyInventoryRepository pharmacyInventoryRepository;

        public PharmacyInventoryController(IPharmacyRepository pharmacyRepository,IMedicineRepository medicineRepository,IPharmacyInventoryRepository pharmacyInventoryRepository)
        {
            this.pharmacyRepository = pharmacyRepository;
            this.medicineRepository = medicineRepository;
            this.pharmacyInventoryRepository = pharmacyInventoryRepository;
        }
        #endregion
        [HttpPost("insertMedicine")]                //api/pharmacyInventory/insertMedicine
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> InsertMedicine([FromBody] PharmacyInvetoryDTO model)
        {
            // Verify the Pharmacy exists and has an active profile
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy == null) return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);
            if (pharmacy.ActiveProfile == null) return ErrorResponse("Pharmacy has no active profile", ErrorCodes.UserNotFound);

            // Verify the Medicine exists in the Master list
            var medicineMaster = await medicineRepository.GetByIdAsync(model.medicineId.ToString());
            if (medicineMaster == null) return ErrorResponse("Medicine not found in master database", ErrorCodes.DataNotFound);

            // Prevent duplicate entry (Check if already in inventory)
            var existingEntry = await pharmacyInventoryRepository
                .GetPharmacyMedicineAsync(pharmacy.ActiveProfile.Id.ToString(), model.medicineId);
            if (existingEntry != null)
                return ErrorResponse("Medicine already exists in this pharmacy inventory", ErrorCodes.DuplicateEntry);
            if (model.expiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
                return ErrorResponse("Medicine has expired.", ErrorCodes.ValidationError);

            // Map and Insert then return response
            var inventoryItem = new PharmacyInventory
            {
                PharmacyProfileId = pharmacy.ActiveProfile.Id,
                MedicineId = model.medicineId,
                StockQuantity = model.quantity,
                ExpiryDate = model.expiryDate,
                Price = model.price
            };

            try
            {
                await pharmacyInventoryRepository.AddMedicineAsync(inventoryItem);
                return SuccessResponse("Medicine added to pharmacy successfully", SuccessCodes.DataCreated);
            }
            catch (Exception ex)
            {
                return ErrorResponse("Failed to add medicine to pharmacy", ErrorCodes.DataBaseError, ex.Message);
            }
        }
        [HttpPost("updateInventory")]                // api/pharmacyInventory/updateInventory
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> UpdateInventory([FromBody] UpdateInventoryDTO model)
        {
            //verify the inventory belongs to the requesting pharmacy
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy == null) return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);
            if (pharmacy.ActiveProfile == null) return ErrorResponse("Pharmacy has no active profile", ErrorCodes.UserNotFound);

            var existingItem = await pharmacyInventoryRepository
                .GetPharmacyMedicineAsync(pharmacy.ActiveProfile.Id.ToString(), model.medicineId);
            if (existingItem == null)
                return ErrorResponse("Medicine not found in pharmacy inventory", ErrorCodes.DataNotFound);

            //Apply updates
            if (model.quantity.HasValue) existingItem.StockQuantity = model.quantity.Value;
            if (model.expiryDate.HasValue) existingItem.ExpiryDate = model.expiryDate.Value;
            if (model.price.HasValue) existingItem.Price = model.price.Value;

            //Save changes
            try
            {
                await pharmacyInventoryRepository.SaveChangesAsync();
                return SuccessResponse("Inventory updated successfully", SuccessCodes.DataUpdated);
            }
            catch (Exception ex)
            {
                return ErrorResponse("Failed to update inventory", ErrorCodes.DataBaseError, ex.Message);
            }
        }
        [HttpDelete("removeMedicine")] // api/pharmacy/removeMedicine
        [ProducesResponseType(typeof(SuccessResponseDTO<object?>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> RemoveMedicine([FromBody] InventoryReference model)
        {
            //verify the inventory belongs to the requesting pharmacy
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);
            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy?.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);

            //remove medicine from inventory
            var success = await pharmacyInventoryRepository.RemoveMedicineAsync(pharmacy.ActiveProfile.Id.ToString(), model.MedicineId);
            if (!success)
                return ErrorResponse("Medicine not found.", ErrorCodes.DataNotFound);

            return SuccessResponse("Medicine removed successfully", SuccessCodes.DataDeleted);
        }
        [HttpGet("viewInventory")]              // api/pharmacyInventory/viewInventory?page=1&pageSize=10
        [ProducesResponseType(typeof(SuccessResponseDTO<PagedDTO<InventoryItemResponseDTO>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> ViewInventory([FromQuery] int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return ErrorResponse("Unauthorized", ErrorCodes.Unauthorized);

            var pharmacy = await pharmacyRepository.GetByIdAsync(userId);
            if (pharmacy?.ActiveProfile == null)
                return ErrorResponse("Pharmacy not found", ErrorCodes.UserNotFound);

            var (items, totalCount) = await pharmacyInventoryRepository
                .GetPharmacyInventoryAsync(pharmacy.ActiveProfile.Id.ToString(), page, pageSize);

            var response = new PagedDTO<InventoryItemResponseDTO>
            {
                currentPage = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                totalCount = totalCount,
                items = items.Select(pi => new InventoryItemResponseDTO
                {
                    medicineId = pi.MedicineId,
                    tradeName = pi.Medicine?.TradeName,
                    genericName = pi.Medicine?.GenericName,
                    quantity = pi.StockQuantity,
                    expiryDate = pi.ExpiryDate,
                    price = pi.Price
                }).ToList()
            };

            return SuccessResponse(response, "Inventory retrieved successfully", SuccessCodes.DataRetrieved);
        }

    }
}

