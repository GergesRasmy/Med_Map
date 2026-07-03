using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Med_Map.Controllers
{
    [Route("api/ai")]
    [ApiController]
    [Authorize(Roles = RoleConstants.Names.Customer)]
    public class AiController : ResponceBaseController
    {
        private readonly IAiService aiService;

        public AiController(IAiService aiService)
        {
            this.aiService = aiService;
        }

        [HttpPost("query")]           //api/ai/query
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> Query([FromBody] QueryAiDTO model)
        {
            var result = await aiService.QueryAsync(model);
            return Relay(result);
        }

        [HttpPost("ocr/medicine")]           //api/ai/ocr/medicine
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> OcrMedicine([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return ErrorResponse("File is required", ErrorCodes.InvalidInput);

            var result = await aiService.OcrMedicineAsync(file);
            return Relay(result);
        }

        [HttpPost("ocr/prescription")]           //api/ai/ocr/prescription
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(ErrorResponseDTO<object>), 400)]
        public async Task<IActionResult> OcrPrescription([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return ErrorResponse("File is required", ErrorCodes.InvalidInput);

            var result = await aiService.OcrPrescriptionAsync(file);
            return Relay(result);
        }

        // Pass the AI service's response straight through — same status code, same body.
        private IActionResult Relay(AiRelayResponse result)
            => new ContentResult
            {
                StatusCode = result.StatusCode,
                Content = result.Content,
                ContentType = "application/json"
            };
    }
}
