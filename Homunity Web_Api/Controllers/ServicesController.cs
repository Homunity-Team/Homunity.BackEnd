using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.Reference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Services")]
    [ApiController]
    [Authorize]
    public class ServicesController : AuthorizedControllerBase
    {
        private readonly IReferenceDataService _referenceData;
        public ServicesController(IReferenceDataService referenceData) => _referenceData = referenceData;

        [HttpGet("GetAll", Name = "GetAllServices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var services = await _referenceData.GetServicesAsync();
            var result = services.Select(s => new ServiceResponse { ServiceId = s.ServiceId, Name = s.Name, Icon = s.Icon }).ToList();

            return Ok(new
            {
                message = services.Count == 0 ? "No services found" : "Services retrieved successfully",
                count = result.Count,
                services = result
            });
        }
    }
}
