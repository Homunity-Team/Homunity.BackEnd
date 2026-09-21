using Homunity_Data_Access.Repositories;
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
        private readonly IServiceRepository _serviceRepository;
        public ServicesController(IServiceRepository serviceRepository) => _serviceRepository = serviceRepository;

        [HttpGet("GetAll", Name = "GetAllServices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceRepository.GetAllAsync();
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