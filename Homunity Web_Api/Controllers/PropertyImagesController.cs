using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Homunity_Buisness_Logic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/PropertyImages")]
    [ApiController]
    public class PropertyImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public PropertyImagesController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
 

        // أضفه في PropertyImagesController
        // =================== GET ALL IMAGES BY PROPERTY ID ===================
        [HttpGet("GetByPropertyId", Name = "GetByPropertyId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetImagesByPropertyId(int propertyId)
        {
            if (propertyId <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            var images = clsPropertyImages.GetImagesByPropertyID(propertyId);

            if (images == null || images.Count == 0)
                return NotFound(new { message = $"No images found for property {propertyId}" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = images.Select(img => new
            {
                imageId = img.ImageId,
                propertyId = img.PropertyId,
                imageUrl = $"{baseUrl}/{img.ImagePath}",
                createdAt = img.CreatedAt
            }).ToList();

            return Ok(new
            {
                propertyId = propertyId,
                count = result.Count,
                images = result
            });
        }
 

        // =================== GET IMAGE BY ID ===================
        [HttpGet("GetImageById", Name = "GetImageById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetImageById(int id)
        {
            // هنا البيزنس لوجيك بيعمل كل حاجة
            var image = clsPropertyImages.FindByImageID(id);

            if (image == null)
                return NotFound(new { message = "Image not found" });

            var imageUrl = $"{Request.Scheme}://{Request.Host}/{image.ImagePath}";

            return Ok(new
            {
                imageId = image.ImageId,
                propertyId = image.PropertyId,
                imageUrl = imageUrl,
                createdAt = image.CreatedAt
            });
        }



         

    }
}