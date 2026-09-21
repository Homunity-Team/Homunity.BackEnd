using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.Properties;
using Homunity_Web_Api.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/PropertyImages")]
    [ApiController]
    [Authorize]
    public class PropertyImagesController : AuthorizedControllerBase
    {
        private readonly IPropertyService _propertyService;
        public PropertyImagesController(IPropertyService propertyService) => _propertyService = propertyService;

        [HttpGet("GetImagesByProperty", Name = "GetImagesByProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetImagesByProperty(int propertyId)
        {
            if (propertyId <= 0)
                return Problem(detail: "Invalid property ID.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var images = await _propertyService.GetImagesByPropertyIdAsync(propertyId);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = images.Select(img => new PropertyImageResponse
            {
                ImageId = img.ImageId,
                PropertyId = img.PropertyId,
                ImageUrl = $"{baseUrl}/{img.ImagePath}",
                CreatedAt = img.CreatedAt
            }).ToList();

            return Ok(result);
        }

        [HttpGet("GetImageById", Name = "GetImageById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyImageResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImageById(int id)
        {
            var image = await _propertyService.FindImageByIdAsync(id);
            if (image == null)
                return Problem(detail: "Image not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var response = new PropertyImageResponse
            {
                ImageId = image.ImageId,
                PropertyId = image.PropertyId,
                ImageUrl = $"{Request.Scheme}://{Request.Host}/{image.ImagePath}",
                CreatedAt = image.CreatedAt
            };

            return Ok(response);
        }
    }
}
