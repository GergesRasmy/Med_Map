using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Med_Map.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponceBaseController : ControllerBase
    {
        [NonAction]
        protected IActionResult SuccessResponse<T>(T data, string message = "Success", string code = SuccessCodes.OK)
            => Ok(new SuccessResponseDTO<T> { success = true, message = message, code = code, data = data });

        [NonAction]
        protected IActionResult SuccessResponse(string message, string code)
            => Ok(new SuccessResponseDTO<object> { success = true, message = message, code = code, data = null });

        [NonAction]
        protected IActionResult ErrorResponse(string message, string code = ErrorCodes.InternalServerError, object? errors = null)
            => BadRequest(new ErrorResponseDTO<object> { success = false, message = message, code = code, error = errors });
    }
}
