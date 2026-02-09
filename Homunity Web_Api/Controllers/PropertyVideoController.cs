using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Homunity_Buisness_Logic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyVideoController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public PropertyVideoController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // ================= UPLOAD VIDEO =================
        [HttpPost("UploadVideo", Name = "UploadVideo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadVideo([Required] int propertyId, [Required] IFormFile file)
        {
            try
            {
                // 1. التحقق الأساسي
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                if (propertyId <= 0)
                    return BadRequest(new { message = "Invalid property ID" });

                // 2. التحقق من نوع الملف
                var allowedExtensions = new[] { ".mp4", ".webm" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Invalid file type. Allowed: mp4, webm" });

                // 3. التحقق من حجم الملف (30MB حد أقصى)
                const long maxSize = 30 * 1024 * 1024; // 30MB
                if (file.Length > maxSize)
                    return BadRequest(new { message = "File too large (max 30MB)" });

                // 4. إنشاء مجلد التخزين
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "videos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 5. إنشاء اسم فريد للملف
                var fileName = $"video_{propertyId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var relativePath = $"uploads/videos/{fileName}";
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                // 6. حفظ الملف
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 7. إنشاء كائن الفيديو
                var video = new clsPropertyVideo
                {
                    PropertyId = propertyId,
                    VideoPath = relativePath,
                    Mode = clsPropertyVideo.enMode.AddNew
                };

                // 8. حفظ في الداتابيز
                bool saveResult = video.Save(file.Length);

                if (!saveResult)
                {
                    // حذف الملف إذا فشل الحفظ
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    return BadRequest(new
                    {
                        message = "Failed to save video. Check if property exists or property already has a video"
                    });
                }

                // 9. إرجاع النتيجة
                var videoUrl = $"{Request.Scheme}://{Request.Host}/{relativePath}";

                return CreatedAtAction(nameof(GetVideoById), new { id = video.VideoId }, new
                {
                    videoId = video.VideoId,
                    propertyId = video.PropertyId,
                    videoUrl = videoUrl,
                    fileSize = file.Length,
                    createdAt = DateTime.UtcNow,
                    message = "Video uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }





        // ================= UPDATE VIDEO =================
        [HttpPut("UpdateVideo", Name = "UpdateVideo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVideo(int id, [Required] IFormFile file)
        {
            try
            {
                // 1. البحث عن الفيديو الحالي
                var existingVideo = clsPropertyVideo.FindByID(id);
                if (existingVideo == null)
                    return NotFound(new { message = "Video not found" });

                // 2. التحقق الأساسي
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                var allowedExtensions = new[] { ".mp4", ".webm" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Invalid file type. Allowed: mp4, webm" });

                const long maxSize = 30 * 1024 * 1024;
                if (file.Length > maxSize)
                    return BadRequest(new { message = "File too large (max 30MB)" });

                // 3. حذف الملف القديم
                var oldFilePath = Path.Combine(_environment.WebRootPath, existingVideo.VideoPath);
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                // 4. إنشاء اسم جديد للملف
                var fileName = $"video_{existingVideo.PropertyId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var relativePath = $"uploads/videos/{fileName}";
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                // 5. حفظ الملف الجديد
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 6. تحديث كائن الفيديو
                existingVideo.VideoPath = relativePath;
                existingVideo.Mode = clsPropertyVideo.enMode.Update;

                // 7. حفظ التحديث
                bool updateResult = existingVideo.Save(file.Length);

                if (!updateResult)
                {
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    return BadRequest(new { message = "Failed to update video" });
                }

                // 8. إرجاع النتيجة
                var videoUrl = $"{Request.Scheme}://{Request.Host}/{relativePath}";

                return Ok(new
                {
                    videoId = existingVideo.VideoId,
                    propertyId = existingVideo.PropertyId,
                    videoUrl = videoUrl,
                    fileSize = file.Length,
                    message = "Video updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }





        // ================= DELETE VIDEO =================
        [HttpDelete("DeleteVideo", Name = "DeleteVideo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteVideo(int id)
        {
            // البحث عن الفيديو
            var video = clsPropertyVideo.FindByID(id);
            if (video == null)
                return NotFound(new { message = "Video not found" });

            try
            {
                // حذف الملف من السيرفر
                var filePath = Path.Combine(_environment.WebRootPath, video.VideoPath);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                // حذف من الداتابيز
                bool deleted = clsPropertyVideo.Delete(id);
                if (!deleted)
                    return BadRequest(new { message = "Failed to delete video" });

                return Ok(new { message = "Video deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting video", error = ex.Message });
            }
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