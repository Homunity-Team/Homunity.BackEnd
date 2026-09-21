using Homunity_Data_Access.Repositories;
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
        private readonly IPropertyRepository _propertyRepository;
        public PropertyImagesController(IPropertyRepository propertyRepository) => _propertyRepository = propertyRepository;

        [HttpGet("GetByPropertyId", Name = "GetByPropertyId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImagesByPropertyId(int propertyId)
        {
            if (propertyId <= 0)
                return Problem(detail: "Invalid property ID.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var images = await _propertyRepository.GetImagesByPropertyIdAsync(propertyId);
            if (images == null || images.Count == 0)
                return Problem(detail: $"No images found for property {propertyId}.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = images.Select(img => new PropertyImageResponse
            {
                ImageId = img.ImageId,
                PropertyId = img.PropertyId,
                ImageUrl = $"{baseUrl}/{img.ImagePath}",
                CreatedAt = img.CreatedAt
            }).ToList();

            return Ok(new { propertyId, count = result.Count, images = result });
        }

        [HttpGet("GetImageById", Name = "GetImageById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyImageResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImageById(int id)
        {
            var image = await _propertyRepository.FindImageByIdAsync(id);
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