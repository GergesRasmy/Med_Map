using Med_Map.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Med_Map.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponceBaseController : ControllerBase
    {
        protected IActionResult SuccessResponse<T>(T data, string message = "Success", string code = null)
            => Ok(new ResponseDTO<T> { success = true, message = message, code = code, data = data });

        protected IActionResult ErrorResponse(string message, string code = "Failed", object errors = null)
            => BadRequest(new ResponseDTO<object> { success = false, message = message, code = code, error = errors });
    }
}
