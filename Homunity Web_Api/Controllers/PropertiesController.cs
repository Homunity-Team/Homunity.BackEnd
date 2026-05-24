using Homunity_Buisness_Logic;
using Homunity_Data_Access;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Homunity_Buisness_Logic.clsUniversities;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Properties")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public PropertiesController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // ================= Create Full Property =================
        [HttpPost("CreateFullProperty", Name = "CreateFullProperty")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFullProperty([FromForm] CreatePropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                const int MAX_IMAGES = 6;
                if (request.Images != null && request.Images.Count > MAX_IMAGES)
                    return BadRequest(new { message = $"Maximum {MAX_IMAGES} images allowed" });

                const long MAX_IMAGE_SIZE = 2 * 1024 * 1024;
                if (request.Images != null)
                {
                    foreach (var file in request.Images)
                    {
                        if (file.Length > MAX_IMAGE_SIZE)
                            return BadRequest(new { message = $"Image '{file.FileName}' size must be less than 2MB" });
                    }
                }

                var allowedImageExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (request.Images != null)
                {
                    foreach (var file in request.Images)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedImageExt.Contains(ext))
                            return BadRequest(new { message = $"Invalid image type '{file.FileName}'. Allowed: jpg, jpeg, png, webp" });
                    }
                }

                var allowedVideoExt = new[] { ".mp4", ".webm" };
                const long MAX_VIDEO_SIZE = 30 * 1024 * 1024;
                if (request.Video != null)
                {
                    var videoExt = Path.GetExtension(request.Video.FileName).ToLower();
                    if (!allowedVideoExt.Contains(videoExt))
                        return BadRequest(new { message = $"Invalid video type '{request.Video.FileName}'. Allowed: mp4, webm" });

                    if (request.Video.Length > MAX_VIDEO_SIZE)
                        return BadRequest(new { message = "Video size must be less than 30MB" });
                }

                string rootPath = _environment.WebRootPath
                                  ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

                var imageFolder = Path.Combine(rootPath, "image", "uploads", "properties");
                var videoFolder = Path.Combine(rootPath, "video", "uploads", "properties");

                if (!Directory.Exists(imageFolder)) Directory.CreateDirectory(imageFolder);
                if (!Directory.Exists(videoFolder)) Directory.CreateDirectory(videoFolder);

                // 1️⃣ حفظ الصور
                List<string> savedImages = new List<string>();
                List<long> imageSizes = new List<long>();

                if (request.Images != null && request.Images.Count > 0)
                {
                    foreach (var file in request.Images)
                    {
                        if (file.Length == 0) continue;

                        var ext = Path.GetExtension(file.FileName).ToLower();
                        var fileName = $"prop_{Guid.NewGuid():N}{ext}";
                        var relativePath = $"image/uploads/properties/{fileName}";
                        var fullPath = Path.Combine(rootPath, relativePath);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                            await file.CopyToAsync(stream);

                        savedImages.Add(relativePath);
                        imageSizes.Add(file.Length);
                    }
                }

                // 2️⃣ حفظ الفيديو (اختياري)
                string savedVideoPath = null;
                long videoSize = 0;

                if (request.Video != null && request.Video.Length > 0)
                {
                    var ext = Path.GetExtension(request.Video.FileName).ToLower();
                    var videoName = $"prop_video_{Guid.NewGuid():N}{ext}";
                    var relativeVideoPath = $"video/uploads/properties/{videoName}";
                    var fullVideoPath = Path.Combine(rootPath, relativeVideoPath);

                    using (var stream = new FileStream(fullVideoPath, FileMode.Create))
                        await request.Video.CopyToAsync(stream);

                    savedVideoPath = relativeVideoPath;
                    videoSize = request.Video.Length;
                }

                // 3️⃣ تحويل الـ Request إلى DTO
                CreateFullPropertyDTO dto = new CreateFullPropertyDTO
                {
                    OwnerID = request.OwnerID,
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    Rooms = request.Rooms,
                    PropertyType = request.PropertyType,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Address = request.Address ?? "",
                    UniversityId = request.UniversityId,
                    Images = savedImages,
                    ImageSizes = imageSizes,
                    VideoUrl = savedVideoPath,
                    VideoSize = videoSize,
                    Services = request.Services
                };

                // 4️⃣ إنشاء العقار
                int propertyID = PropertyOrchestratorService.CreateFullProperty(dto);
                if (propertyID <= 0)
                    return BadRequest(new { message = "Failed to create property. Check logs for details." });

                return Ok(new
                {
                    propertyID = propertyID,
                    images = savedImages.Select(x => $"{Request.Scheme}://{Request.Host}/{x}"),
                    video = savedVideoPath == null ? null : $"{Request.Scheme}://{Request.Host}/{savedVideoPath}",
                    message = "Property created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Server error",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        // ================= Update Full Property =================
        [HttpPut("UpdateFullProperty", Name = "UpdateFullProperty")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFullProperty([FromForm] UpdatePropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existing = clsProperties.FindByID(request.PropertyID);
                if (existing == null)
                    return NotFound(new { message = "Property not found" });

                int currentCount = clsPropertyImages.GetImagesCount(request.PropertyID);
                int deletedCount = request.ImageIdsToDelete?.Count ?? 0;
                int newCount = request.NewImages?.Count ?? 0;
                int finalCount = currentCount - deletedCount + newCount;

                if (finalCount > 6)
                    return BadRequest(new { message = $"Total images after update would be {finalCount}. Maximum is 6" });

                if (finalCount < 0)
                    return BadRequest(new { message = "ImageIdsToDelete contains more images than the property has" });

                var allowedImageExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (request.NewImages != null)
                {
                    foreach (var file in request.NewImages)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedImageExt.Contains(ext))
                            return BadRequest(new { message = $"Invalid image type '{file.FileName}'. Allowed: jpg, jpeg, png, webp" });

                        if (file.Length > 2 * 1024 * 1024)
                            return BadRequest(new { message = $"Image '{file.FileName}' size must be less than 2MB" });
                    }
                }

                var allowedVideoExt = new[] { ".mp4", ".webm" };
                if (request.NewVideo != null)
                {
                    var videoExt = Path.GetExtension(request.NewVideo.FileName).ToLower();
                    if (!allowedVideoExt.Contains(videoExt))
                        return BadRequest(new { message = "Invalid video type. Allowed: mp4, webm" });

                    if (request.NewVideo.Length > 30 * 1024 * 1024)
                        return BadRequest(new { message = "Video size must be less than 30MB" });
                }

                string rootPath = _environment.WebRootPath
                                  ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

                // 1️⃣ حفظ الصور الجديدة
                List<string> newImagePaths = new List<string>();
                List<long> newImageSizes = new List<long>();

                if (request.NewImages != null && request.NewImages.Count > 0)
                {
                    var imageFolder = Path.Combine(rootPath, "image", "uploads", "properties");
                    if (!Directory.Exists(imageFolder)) Directory.CreateDirectory(imageFolder);

                    foreach (var file in request.NewImages)
                    {
                        if (file.Length == 0) continue;

                        var ext = Path.GetExtension(file.FileName).ToLower();
                        var fileName = $"prop_{Guid.NewGuid():N}{ext}";
                        var relativePath = $"image/uploads/properties/{fileName}";
                        var fullPath = Path.Combine(rootPath, relativePath);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                            await file.CopyToAsync(stream);

                        newImagePaths.Add(relativePath);
                        newImageSizes.Add(file.Length);
                    }
                }

                // 2️⃣ حفظ الفيديو الجديد (اختياري)
                string newVideoPath = null;
                long newVideoSize = 0;

                if (request.NewVideo != null && request.NewVideo.Length > 0)
                {
                    var videoFolder = Path.Combine(rootPath, "video", "uploads", "properties");
                    if (!Directory.Exists(videoFolder)) Directory.CreateDirectory(videoFolder);

                    var ext = Path.GetExtension(request.NewVideo.FileName).ToLower();
                    var videoName = $"prop_video_{Guid.NewGuid():N}{ext}";
                    var relativeVideoPath = $"video/uploads/properties/{videoName}";
                    var fullVideoPath = Path.Combine(rootPath, relativeVideoPath);

                    using (var stream = new FileStream(fullVideoPath, FileMode.Create))
                        await request.NewVideo.CopyToAsync(stream);

                    newVideoPath = relativeVideoPath;
                    newVideoSize = request.NewVideo.Length;
                }

                // 3️⃣ تحويل الـ Request إلى DTO
                var dto = new UpdateFullPropertyDTO
                {
                    PropertyID = request.PropertyID,
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    Rooms = request.Rooms,
                    PropertyType = request.PropertyType,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Address = request.Address ?? "",
                    UniversityId = request.UniversityId,
                    NewImages = newImagePaths,
                    NewImageSizes = newImageSizes,
                    ImageIdsToDelete = request.ImageIdsToDelete,
                    NewVideoUrl = newVideoPath,
                    NewVideoSize = newVideoSize,
                    DeleteVideo = request.DeleteVideo,
                    Services = request.Services
                };

                // 4️⃣ Update العقار
                bool updated = PropertyOrchestratorService.UpdateFullProperty(dto);
                if (!updated)
                    return BadRequest(new { message = "Failed to update property" });

                return Ok(new { message = "Property updated successfully", propertyId = request.PropertyID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }


        // ================= Delete =================
        [HttpDelete("DeleteProperty", Name = "DeleteProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            var existing = clsProperties.FindByID(id);
            if (existing == null)
                return NotFound(new { message = "Property not found" });

            bool deleted = clsProperties.Delete(id);
            if (!deleted)
                return StatusCode(500, new { message = "Delete failed" });

            return Ok(new { message = "Property deleted successfully", propertyID = id });
        }


        // ================= Get All Properties =================
        [HttpGet("GetAll", Name = "GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAll()
        {
            var properties = clsProperties.GetAllProperties();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            if (properties == null || properties.Count == 0)
                return NotFound(new { message = "No available properties at the moment.", count = 0 });

            var result = properties.Select(p =>
                PropertyResponseDTO.FromProperty(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();

            return Ok(new { message = "Properties retrieved successfully", count = result.Count, properties = result });
        }


        // ================= Get Property By ID =================
        [HttpGet("GetByID", Name = "GetByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetByID(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            var property = clsProperties.FindByID(id);
            if (property == null)
                return NotFound(new { message = $"Property with ID {id} not found" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = PropertyResponseDTO.FromProperty(property, baseUrl,
                clsPropertyImages.GetImagesByPropertyID(id),
                clsPropertyVideo.GetVideoByPropertyID(id),
                clsPropertyServices.GetServicesByPropertyID(id));

            return Ok(new { message = "Property found", property = result });
        }


        // ================= Search Properties =================
        [HttpGet("Search", Name = "SearchProperties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Search(string city = null, string area = null,
                                    decimal? minPrice = null, decimal? maxPrice = null)
        {
            if (minPrice.HasValue && minPrice < 0)
                return BadRequest(new { message = "MinPrice cannot be negative" });

            if (maxPrice.HasValue && maxPrice < 0)
                return BadRequest(new { message = "MaxPrice cannot be negative" });

            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                return BadRequest(new { message = "MinPrice cannot be greater than MaxPrice" });

            var properties = clsProperties.Search(city, area, minPrice, maxPrice);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = properties.Select(p =>
                PropertyResponseDTO.FromProperty(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();

            if (result.Count == 0)
                return Ok(new { message = "No available properties at the moment.", count = 0 });

            return Ok(new { message = "Properties found", count = result.Count, properties = result });
        }


        // ================= Get By Owner =================
        [HttpGet("GetByOwner", Name = "GetByOwner")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetByOwner(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest(new { message = "Invalid owner ID" });

            var properties = clsProperties.GetPropertiesByOwnerID(ownerId);
            if (properties == null || properties.Count == 0)
                return NotFound(new { message = $"No properties found for owner {ownerId}" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p =>
                PropertyResponseDTO.FromProperty(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();

            return Ok(new { message = "Properties retrieved successfully", count = result.Count, properties = result });
        }


        // ================= Search By University =================
        [HttpGet("SearchByUniversity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SearchByUniversity(int universityId, decimal? maxPrice = null)
        {
            if (universityId <= 0)
                return BadRequest(new { message = "Invalid universityId" });

            if (maxPrice.HasValue && maxPrice < 0)
                return BadRequest(new { message = "MaxPrice cannot be negative" });

            var results = clsUniversities.SearchByUniversity(universityId, maxPrice);
            return _BuildUniversityResponse(results, universityId);
        }


        // ================= Search By University Nearby =================
        [HttpGet("SearchByUniversityNearby")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SearchByUniversityNearby(int universityId, double maxDistance, decimal? maxPrice = null)
        {
            if (universityId <= 0)
                return BadRequest(new { message = "Invalid universityId" });

            if (maxDistance <= 0)
                return BadRequest(new { message = "MaxDistance must be greater than 0" });

            if (maxPrice.HasValue && maxPrice < 0)
                return BadRequest(new { message = "MaxPrice cannot be negative" });

            var results = clsUniversities.SearchByUniversity(universityId, maxPrice, maxDistance);
            return _BuildUniversityResponse(results, universityId);
        }


        // ================= Helper =================
        private IActionResult _BuildUniversityResponse(List<PropertyWithDistanceDTO> results, int universityId)
        {
            if (results.Count == 0)
                return Ok(new { message = "No available properties found near this university.", count = 0 });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = results.Select(r =>
            {
                var video = clsPropertyVideo.GetVideoByPropertyID(r.Property.PropertyID);
                return new
                {
                    propertyId = r.Property.PropertyID,
                    title = r.Property.Title,
                    description = r.Property.Description,
                    price = r.Property.Price,
                    rooms = r.Property.Rooms,
                    propertyType = r.Property.PropertyType,
                    statusId = r.Property.PropertyStatusID,
                    createdAt = r.Property.CreatedAt,
                    location = new
                    {
                        locationId = r.Property.LocationID,
                        city = r.Property.City,
                        area = r.Property.Area,
                        street = r.Property.Street,
                        latitude = r.Property.Latitude,
                        longitude = r.Property.Longitude
                    },
                    university = new
                    {
                        universityId = r.UniversityId,
                        name = r.UniversityName,
                        latitude = r.UniversityLat,
                        longitude = r.UniversityLon,
                        distance_km = r.DistanceKm
                    },
                    images = clsPropertyImages.GetImagesByPropertyID(r.Property.PropertyID)
                        .Select(img => new { imageId = img.ImageId, imageUrl = $"{baseUrl}/{img.ImagePath}" }).ToList(),
                    video = video == null ? null : (object)new { videoId = video.VideoId, videoUrl = $"{baseUrl}/{video.VideoPath}" },
                    services = clsPropertyServices.GetServicesByPropertyID(r.Property.PropertyID)
                        .Select(s => new { serviceId = s.ServiceId, name = s.Name, icon = s.Icon }).ToList()
                };
            }).ToList();

            return Ok(new
            {
                message = "Properties found",
                count = response.Count,
                university = new { universityId = results[0].UniversityId, name = results[0].UniversityName },
                properties = response
            });
        }


        // ================= Get All V2 =================
        [HttpGet("GetAllV2", Name = "GetAllV2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAllV2()
        {
            var properties = clsProperties.GetAllPropertiesV2();
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            if (properties == null || properties.Count == 0)
                return NotFound(new { message = "No available properties at the moment.", count = 0 });

            var result = properties.Select(p =>
                PropertyResponseDTO.FromPropertyV2(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();

            return Ok(new { message = "Properties retrieved successfully", count = result.Count, properties = result });
        }


        // ================= Get By ID V2 =================
        [HttpGet("GetByIDV2", Name = "GetByIDV2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetByIDV2(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            var property = clsProperties.FindByIDV2(id);
            if (property == null)
                return NotFound(new { message = $"Property with ID {id} not found" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = PropertyResponseDTO.FromPropertyV2(property, baseUrl,
                clsPropertyImages.GetImagesByPropertyID(id),
                clsPropertyVideo.GetVideoByPropertyID(id),
                clsPropertyServices.GetServicesByPropertyID(id));

            return Ok(new { message = "Property found", property = result });
        }


        // ================= Get By Owner V2 =================
        [HttpGet("GetByOwnerV2", Name = "GetByOwnerV2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetByOwnerV2(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest(new { message = "Invalid owner ID" });

            var properties = clsProperties.GetPropertiesByOwnerIDV2(ownerId);
            if (properties == null || properties.Count == 0)
                return NotFound(new { message = $"No properties found for owner {ownerId}" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p =>
                PropertyResponseDTO.FromPropertyV2(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();

            return Ok(new { message = "Properties retrieved successfully", count = result.Count, properties = result });
        }
    }
}