using Med_Map.DTO;
using Med_Map.DTO.ResponseDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Med_Map.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponceBaseController : ControllerBase
    {
        [NonAction]
        protected IActionResult SuccessResponse<T>(T data, string message = "Success", string code = null)
            => Ok(new SuccessResponseDTO<T> { success = true, message = message, code = code, data = data });

        [NonAction]
        protected IActionResult SuccessResponse(string message, string code)
        {
            return Ok(new { Success = true, Message = message, Code = code });
        }

        [NonAction]
        protected IActionResult ErrorResponse(string message, string code = "Failed", object errors = null)
            => BadRequest(new ErrorResponseDTO<object> { success = false, message = message, code = code, error = errors });

        [NonAction]
        protected IActionResult HandleValidationErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return ErrorResponse("Validation failed", ErrorCodes.ValidationError, errors);
        }
    }
}
