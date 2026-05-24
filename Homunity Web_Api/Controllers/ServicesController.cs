using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Services")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        [HttpGet("GetAll", Name = "GetAllServices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var services = clsServices.GetAllServices();

            return Ok(new
            {
                message = services.Count == 0 ? "No services found" : "Services retrieved successfully",
                count = services.Count,
                services = services
            });
        }
    }
}
