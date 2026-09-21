using Homunity_Data_Access.Repositories;
using Homunity_Shared_DTOs.Properties;
using Homunity_Web_Api.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/PropertyVideo")]
    [ApiController]
    [Authorize]
    public class PropertyVideoController : AuthorizedControllerBase
    {
        private readonly IPropertyRepository _propertyRepository;
        public PropertyVideoController(IPropertyRepository propertyRepository) => _propertyRepository = propertyRepository;

        [HttpGet("GetVideoById", Name = "GetVideoById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyVideoResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVideoById(int id)
        {
            var video = await _propertyRepository.FindVideoByIdAsync(id);
            if (video == null)
                return Problem(detail: "Video not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var response = new PropertyVideoResponse
            {
                VideoId = video.VideoId,
                PropertyId = video.PropertyId,
                VideoUrl = $"{Request.Scheme}://{Request.Host}/{video.VideoPath}",
                CreatedAt = video.CreatedAt
            };

            return Ok(response);
        }

        [HttpGet("GetVideoByProperty", Name = "GetVideoByProperty")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyVideoResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVideoByProperty(int propertyId)
        {
            if (propertyId <= 0)
                return Problem(detail: "Invalid property ID.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var video = await _propertyRepository.GetVideoByPropertyIdAsync(propertyId);
            if (video == null)
                return Problem(detail: "No video found for this property.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var response = new PropertyVideoResponse
            {
                VideoId = video.VideoId,
                PropertyId = video.PropertyId,
                VideoUrl = $"{Request.Scheme}://{Request.Host}/{video.VideoPath}",
                CreatedAt = video.CreatedAt
            };

            return Ok(response);
        }
    }
}