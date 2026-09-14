using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/PropertyImages")]
    [ApiController]
    [Authorize]
    public class PropertyImagesController : AuthorizedControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        public PropertyImagesController(IWebHostEnvironment environment) => _environment = environment;

        [HttpGet("GetByPropertyId", Name = "GetByPropertyId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetImagesByPropertyId(int propertyId)
        {
            if (propertyId <= 0)
                return Problem(detail: "Invalid property ID.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var images = clsPropertyImages.GetImagesByPropertyID(propertyId);
            if (images == null || images.Count == 0)
                return Problem(detail: $"No images found for property {propertyId}.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = images.Select(img => new
            {
                imageId = img.ImageId,
                propertyId = img.PropertyId,
                imageUrl = $"{baseUrl}/{img.ImagePath}",
                createdAt = img.CreatedAt
            }).ToList();

            return Ok(new { propertyId, count = result.Count, images = result });
        }

        [HttpGet("GetImageById", Name = "GetImageById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetImageById(int id)
        {
            var image = clsPropertyImages.FindByImageID(id);
            if (image == null)
                return Problem(detail: "Image not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var imageUrl = $"{Request.Scheme}://{Request.Host}/{image.ImagePath}";
            return Ok(new { imageId = image.ImageId, propertyId = image.PropertyId, imageUrl, createdAt = image.CreatedAt });
        }
    }
}