using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Med_Map.Controllers
{
    [Route("api/medicine")]
    [ApiController]
    public class MedicineController : ResponceBaseController
    {
        #region ctor
        private readonly IMedicineRepository medicineRepository;
        private readonly IFileService fileService;

        public MedicineController( IMedicineRepository medicineRepository,IFileService fileService)
        {
            this.medicineRepository = medicineRepository;
            this.fileService = fileService;
        }
        #endregion

        [Authorize(Roles = "Admin")]
        [HttpPost("add")]               //api/medicine/add
        public async Task<IActionResult> AddMedicine([FromForm] AddMedicineDTO medicine)
        {
            if (await medicineRepository.ExistsAsync(medicine.tradeName))
            {
                return ErrorResponse("A medicine with this trade name already exists.", ErrorCodes.ValidationError);
            }
            var newMedicine = new MedicineMaster
            {
                TradeName = medicine.tradeName,
                GenericName = medicine.genericName,
                Price = medicine.price,
                IsRestricted = medicine.isRestricted,
                Manufacturer = medicine.manufacturer
            };
            try
            {
                string? Path = await fileService.SaveFileAsync(medicine.image, "Medicine_Images");
                newMedicine.ImageUrl = Path;
            }
            catch (Exception ex)
            {
                return ErrorResponse("Image didn't save", ErrorCodes.ValidationError,ex.Message);
            }
            //Save to Database and Return Response
            await medicineRepository.InsertAsync(newMedicine);
            var response = MapToDto(newMedicine);
            return SuccessResponse(response, "Medicine added successfully", SuccessCodes.DataCreated);
        }
        [HttpGet("allMedicine")]        //api/medicine/order/allMedicine
        public async Task<IActionResult> getAllMedicine()
        {
            //get the medicine from the database
            var medicine = await medicineRepository.GetAllMedicineAsync();

            if (medicine == null || !medicine.Any())
                return SuccessResponse(new List<MedicineResponseDTO>(), "No medicines found", SuccessCodes.DataRetrieved);

            //Map to DTO and Return Response
            var response = medicine.Select(MapToDto).ToList();
            return SuccessResponse(response, "Medicines retrieved successfully", SuccessCodes.DataRetrieved);
        }
        [HttpGet("getById")]            // api/medicine/getById?id=
        public async Task<IActionResult> GetMedicineById([FromQuery]string id)
        {
            //get the medicine from the database
            var medicine = await medicineRepository.GetByIdAsync(id);
            if (medicine == null)
                return ErrorResponse("Order not found", ErrorCodes.DataNotFound);
            
            //Map to DTO
            var response = MapToDto(medicine);
            return SuccessResponse<MedicineResponseDTO>(response, "Medicine retrieved successfully", SuccessCodes.DataRetrieved);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]            //api/medicine/update
        public async Task<IActionResult> UpdateMedicine([FromForm] UpdateMedicineDTO NewMedicine)
        {
            //Check if the new trade name is already taken by another medicine (excluding the current medicine)
            var isNameTaken = await medicineRepository.ExistsAsync(NewMedicine.tradeName, excludeId: NewMedicine.id);
            if (isNameTaken) return ErrorResponse("This trade name is already assigned to another medicine.", ErrorCodes.ValidationError);

            //Get the existing medicine from the database
            var ExistingMedicine = await medicineRepository.GetByIdAsync(NewMedicine.id);
            if (ExistingMedicine == null)
                return ErrorResponse("Medicine not found", ErrorCodes.DataNotFound);

            //Update the existing medicine with the new values
            ExistingMedicine.TradeName = NewMedicine.tradeName;
            ExistingMedicine.GenericName = NewMedicine.genericName;
            ExistingMedicine.Price = NewMedicine.price;
            ExistingMedicine.IsRestricted = NewMedicine.isRestricted;
            ExistingMedicine.Manufacturer = NewMedicine.manufacturer;
            if (NewMedicine.image != null)
            {
                var oldImageUrl = ExistingMedicine.ImageUrl;
                string imageUrl;
                try
                {
                    imageUrl = await fileService.SaveFileAsync(NewMedicine.image, "Medicine_Images");
                }
                catch (Exception)
                {
                    return ErrorResponse("Image failed to save. Please try again.", ErrorCodes.ValidationError);
                }

                ExistingMedicine.ImageUrl = imageUrl;

                if (oldImageUrl != null)
                    await fileService.DeleteFileAsync(oldImageUrl);
            }
            //Save the updated medicine to the database and Return Response
            await medicineRepository.UpdateAsync(ExistingMedicine);
            var response = MapToDto(ExistingMedicine);
            return SuccessResponse(response, "Medicine updated successfully", SuccessCodes.DataUpdated);
        }
        [HttpDelete("delete")]             // api/medicine/delete?id=
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMedicine([FromQuery]string id)
        {
            //Get the existing medicine from the database
            var medicine = await medicineRepository.GetByIdAsync(id);
            if (medicine == null)
                return ErrorResponse("Medicine not found", ErrorCodes.DataNotFound);

            //Delete the medicine from the database and Return Response
            await medicineRepository.DeleteAsync(id);
            return SuccessResponse(message: "Medicine deleted successfully", code: SuccessCodes.DataDeleted);
        }
        [HttpGet("search")]             // api/medicine/search?query=
        public async Task<IActionResult> SearchMedicine([FromQuery]string query)
        {
            //Search for medicines by trade name in the database
            var medicines = await medicineRepository.GetByTradeNameAsync(query);
            if (medicines == null || !medicines.Any())
                return SuccessResponse(new List<MedicineResponseDTO>(), "No medicines found matching the search criteria", SuccessCodes.DataRetrieved);
           
            //Map to DTO and Return Response
            var response = medicines.Select(MapToDto).ToList();
            return SuccessResponse(response, "Medicines retrieved successfully", SuccessCodes.DataRetrieved);
        }

        // Helper method to Map MedicineMaster to MedicineResponseDTO
        private MedicineResponseDTO MapToDto(MedicineMaster medicine)
        {
            return new MedicineResponseDTO
            {
                id = medicine.Id,
                tradeName = medicine.TradeName,
                genericName = medicine.GenericName,
                price = medicine.Price,
                imageURL = medicine.ImageUrl,
                isRestricted = medicine.IsRestricted,
                manufacturer = medicine.Manufacturer
            };
        }

    }
}

