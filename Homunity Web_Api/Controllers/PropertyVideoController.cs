using Homunity_Business_Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/PropertyVideo")]
    [ApiController]
    [Authorize]
    public class PropertyVideoController : AuthorizedControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        public PropertyVideoController(IWebHostEnvironment environment) => _environment = environment;

        [HttpGet("GetVideoById", Name = "GetVideoById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetVideoById(int id)
        {
            var video = clsPropertyVideo.FindByID(id);
            if (video == null)
                return Problem(detail: "Video not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var videoUrl = $"{Request.Scheme}://{Request.Host}/{video.VideoPath}";
            return Ok(new { videoId = video.VideoId, propertyId = video.PropertyId, videoUrl, createdAt = video.CreatedAt });
        }

        [HttpGet("GetVideoByProperty", Name = "GetVideoByProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetVideoByProperty(int propertyId)
        {
            if (propertyId <= 0)
                return Problem(detail: "Invalid property ID.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var video = clsPropertyVideo.GetVideoByPropertyID(propertyId);
            if (video == null)
                return Problem(detail: "No video found for this property.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var videoUrl = $"{Request.Scheme}://{Request.Host}/{video.VideoPath}";
            return Ok(new { videoId = video.VideoId, propertyId = video.PropertyId, videoUrl, createdAt = video.CreatedAt });
        }
    }
}