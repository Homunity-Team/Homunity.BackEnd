using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using System.Transactions;
using Homunity_Business_Logic;// لو موجود بالفعل

namespace Homunity_Web_Api.Controllers
{
    [Route("api/PropertyVideo")]
    [ApiController]
    public class PropertyVideoController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public PropertyVideoController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }


        // ================= GET VIDEO BY ID =================
        [HttpGet("GetVideoById", Name = "GetVideoById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetVideoById(int id)
        {
            // البحث عن الفيديو
            var video = clsPropertyVideo.FindByID(id);
            if (video == null)
                return NotFound(new { message = "Video not found" });

            var videoUrl = $"{Request.Scheme}://{Request.Host}/{video.VideoPath}";

            return Ok(new
            {
                videoId = video.VideoId,
                propertyId = video.PropertyId,
                videoUrl = videoUrl,
                createdAt = video.CreatedAt
            });
        }





        // ================= GET VIDEO BY PROPERTY =================
        [HttpGet("GetVideoByProperty", Name = "GetVideoByProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetVideoByProperty(int propertyId)
        {
            if (propertyId <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            // البحث عن أحدث فيديو للعقار
            var video = clsPropertyVideo.GetVideoByPropertyID(propertyId);
            if (video == null)
                return NotFound(new { message = "No video found for this property" });

            var videoUrl = $"{Request.Scheme}://{Request.Host}/{video.VideoPath}";

            return Ok(new
            {
                videoId = video.VideoId,
                propertyId = video.PropertyId,
                videoUrl = videoUrl,
                createdAt = video.CreatedAt
            });
        }
    }
}