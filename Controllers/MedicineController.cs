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
        public MedicineController( IMedicineRepository medicineRepository)
        {
            this.medicineRepository = medicineRepository;
        }
        #endregion
        [HttpPost("add")]//api/medicine/add
        [Authorize(Roles = "Pharmacy")]
        public async Task<IActionResult> AddMedicine([FromBody] AddMedicineDTO medicine)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return ErrorResponse("Validation failed", ErrorCodes.ValidationError, errors);
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
                string imageUrl = await SaveFile(medicine.image, "Medicine_Images");
                newMedicine.ImageUrl = imageUrl;
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
        [HttpGet("allMedicine")]//api/order/allMedicine
        public async Task<IActionResult> getAllMedicine()
        {
            var medicine = await medicineRepository.GetAllMedicineAsync();

            if (medicine == null || !medicine.Any())
            {
                return SuccessResponse(new List<MedicineResponseDTO>(), "No medicines found", SuccessCodes.DataRetrieved);
            }
            var response = medicine.Select(MapToDto).ToList();
            return SuccessResponse(response, "Medicines retrieved successfully", SuccessCodes.DataRetrieved);
        }
        [HttpGet] // api/medicine?id=
        public async Task<IActionResult> GetOrderById([FromQuery]string id)
        {
            //get the order from the database
            var medicine = await medicineRepository.GetByIdAsync(id);
            if (medicine == null)
            {
                return ErrorResponse("Order not found", ErrorCodes.DataNotFound);
            }
            //Map to DTO
            var response = MapToDto(medicine);
            return SuccessResponse<MedicineResponseDTO>(response, "Medicine retrieved successfully", SuccessCodes.DataRetrieved);
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

        // Helper method to Save incoming Images
        private async Task<string?> SaveFile(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            //Size Validation (3 MB)
            const long MaxFileSize = 3 * 1024 * 1024;
            if (file.Length > MaxFileSize)
                throw new Exception("File size exceeds the 3MB limit.");

            //Extension/MIME Type Validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new Exception("Invalid file type. Only JPG, PNG, and WebP are allowed.");

            //Define and Ensure Directory
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            //Secure File Naming
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //Save the file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folderName}/{uniqueFileName}";
        }
    }
}

