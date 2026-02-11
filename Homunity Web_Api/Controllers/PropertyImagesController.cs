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
    public class PropertyImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public PropertyImagesController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }



        // =================== UPLOAD IMAGE ===================

        [HttpPost("UploadImage", Name = "UploadImage")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadImage([Required] int propertyId, [Required] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                if (propertyId <= 0)
                    return BadRequest(new { message = "Invalid property ID" });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Invalid file type" });

                const long maxSize = 2 * 1024 * 1024;
                if (file.Length > maxSize)
                    return BadRequest(new { message = "File too large (max 2MB)" });

                // 🔴 التعديل هنا
                string rootPath = _environment.WebRootPath
                                  ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

                var uploadsFolder = Path.Combine(rootPath, "image", "uploads", "properties");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"prop_{propertyId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var relativePath = $"image/uploads/properties/{fileName}";
                var fullPath = Path.Combine(rootPath, relativePath);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var image = new clsPropertyImages
                {
                    PropertyId = propertyId,
                    ImagePath = relativePath,
                    Mode = clsPropertyImages.enMode.AddNew
                };

                bool saveResult = image.Save(file.Length);

                if (!saveResult)
                {
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    return BadRequest(new { message = "Failed to save image" });
                }

                var imageUrl = $"{Request.Scheme}://{Request.Host}/{relativePath}";

                return CreatedAtAction(nameof(GetImageById), new { id = image.ImageId }, new
                {
                    imageId = image.ImageId,
                    propertyId = image.PropertyId,
                    imageUrl = imageUrl,
                    message = "Image uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }





        // ================= UPDATE IMAGE =================
        [HttpPut("UpdateImage", Name = "UpdateImage")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateImage(int id, [Required] IFormFile file)
        {
            try
            {
                // 1. Find existing image (بيزنس لوجيك)
                var existingImage = clsPropertyImages.FindByImageID(id);
                if (existingImage == null)
                    return NotFound(new { message = "Image not found" });

                // 2. Basic validation in Controller
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Invalid file type" });

                const long maxSize = 2 * 1024 * 1024;
                if (file.Length > maxSize)
                    return BadRequest(new { message = "File too large (max 2MB)" });

                // 3. Delete old file
                var oldFilePath = Path.Combine(_environment.WebRootPath, existingImage.ImagePath);
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                // 4. Generate new filename
                var fileName = $"prop_{existingImage.PropertyId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                var relativePath = $"uploads/properties/{fileName}";
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                // 5. Save new file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 6. Update business object
                existingImage.ImagePath = relativePath;
                existingImage.Mode = clsPropertyImages.enMode.Update;

                // 7. Save changes (بيزنس لوجيك بيعمل validation)
                bool updateResult = existingImage.Save(file.Length);

                if (!updateResult)
                {
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    return BadRequest(new { message = "Failed to update image" });
                }

                // 8. Return success
                var imageUrl = $"{Request.Scheme}://{Request.Host}/{relativePath}";

                return Ok(new
                {
                    imageId = existingImage.ImageId,
                    propertyId = existingImage.PropertyId,
                    imageUrl = imageUrl,
                    fileSize = file.Length,
                    message = "Image updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }






        // ================= DELETE IMAGE =================
        [HttpDelete("DeleteImage", Name = "DeleteImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteImage(int id)
        {
            // Find image (بيزنس لوجيك)
            var image = clsPropertyImages.FindByImageID(id);

            if (image == null)
                return NotFound(new { message = "Image not found" });

            try
            {
                // Delete file from server
                var filePath = Path.Combine(_environment.WebRootPath, image.ImagePath);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                // Delete from database (بيزنس لوجيك)
                bool deleted = clsPropertyImages.Delete(id);

                if (!deleted)
                    return BadRequest(new { message = "Failed to delete image" });

                return Ok(new { message = "Image deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting image", error = ex.Message });
            }
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






        // =================== GET IMAGES COUNT ===================
        [HttpGet("count", Name = "propertyId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetImagesCount(int propertyId)
        {
            if (propertyId <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            // بيزنس لوجيك بيعمل كل حاجة
            int count = clsPropertyImages.GetImagesCount(propertyId);

            return Ok(new
            {
                propertyId = propertyId,
                imagesCount = count,
                maxAllowed = 6,
                canAddMore = count < 6
            });
        }



    }
}