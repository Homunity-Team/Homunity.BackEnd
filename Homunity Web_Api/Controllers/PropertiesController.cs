using Homunity_Buisness_Logic;
using Homunity_Business_Logic;
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
            => _environment = environment;

        // ── helpers ──
        private string RootPath => _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        private static readonly string[] AllowedImageExt = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedVideoExt = { ".mp4", ".webm" };
        private const long MAX_IMAGE_SIZE = 2L * 1024 * 1024;
        private const long MAX_VIDEO_SIZE = 30L * 1024 * 1024;

        private IActionResult ValidateImages(IList<IFormFile> images)
        {
            if (images == null) return null;
            if (images.Count > 6) return BadRequest(new { message = "Maximum 6 images" });
            foreach (var f in images)
            {
                if (f.Length > MAX_IMAGE_SIZE)
                    return BadRequest(new { message = $"Image '{f.FileName}' > 2MB" });
                if (!AllowedImageExt.Contains(Path.GetExtension(f.FileName).ToLower()))
                    return BadRequest(new { message = $"Invalid image type '{f.FileName}'" });
            }
            return null;
        }

        private IActionResult ValidateVideo(IFormFile video)
        {
            if (video == null) return null;
            if (video.Length > MAX_VIDEO_SIZE)
                return BadRequest(new { message = "Video > 30MB" });
            if (!AllowedVideoExt.Contains(Path.GetExtension(video.FileName).ToLower()))
                return BadRequest(new { message = $"Invalid video type '{video.FileName}'" });
            return null;
        }

        private async Task<(List<string> paths, List<long> sizes)> SaveImagesAsync(IList<IFormFile> images)
        {
            var paths = new List<string>();
            var sizes = new List<long>();
            if (images == null) return (paths, sizes);

            var folder = Path.Combine(RootPath, "image", "uploads", "properties");
            Directory.CreateDirectory(folder);

            foreach (var file in images)
            {
                if (file.Length == 0) continue;
                var ext = Path.GetExtension(file.FileName).ToLower();
                var name = $"prop_{System.Guid.NewGuid():N}{ext}";
                var rel = $"image/uploads/properties/{name}";
                await using var stream = new FileStream(Path.Combine(RootPath, rel), FileMode.Create);
                await file.CopyToAsync(stream);
                paths.Add(rel);
                sizes.Add(file.Length);
            }
            return (paths, sizes);
        }

        private async Task<(string path, long size)> SaveVideoAsync(IFormFile video)
        {
            if (video == null || video.Length == 0) return (null, 0);
            var folder = Path.Combine(RootPath, "video", "uploads", "properties");
            Directory.CreateDirectory(folder);
            var ext = Path.GetExtension(video.FileName).ToLower();
            var name = $"prop_video_{System.Guid.NewGuid():N}{ext}";
            var rel = $"video/uploads/properties/{name}";
            await using var stream = new FileStream(Path.Combine(RootPath, rel), FileMode.Create);
            await video.CopyToAsync(stream);
            return (rel, video.Length);
        }

        // ── CREATE ──
        [HttpPost("CreateFullProperty", Name = "CreateFullProperty")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateFullProperty([FromForm] CreatePropertyRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var imgErr = ValidateImages(request.Images);
            if (imgErr != null) return imgErr;

            var vidErr = ValidateVideo(request.Video);
            if (vidErr != null) return vidErr;

            var (savedImages, imageSizes) = await SaveImagesAsync(request.Images);
            var (savedVideo, videoSize) = await SaveVideoAsync(request.Video);

            var dto = new CreateFullPropertyDTO
            {
                OwnerID = request.OwnerID,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Rooms = request.Rooms,
                PropertyType = request.PropertyType,
                City = request.City ?? "",
                Area = request.Area ?? "",
                Street = request.Street ?? "",
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Address = request.Address ?? "",
                UniversityId = request.UniversityId,
                Images = savedImages,
                ImageSizes = imageSizes,
                VideoUrl = savedVideo,
                VideoSize = videoSize,
                Services = request.Services
            };

            int propertyID = await PropertyOrchestratorService.CreateFullPropertyAsync(dto);
            if (propertyID <= 0)
                return BadRequest(new { message = "Failed to create property. Check logs." });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return Ok(new
            {
                propertyID,
                images = savedImages.Select(x => $"{baseUrl}/{x}"),
                video = savedVideo == null ? null : $"{baseUrl}/{savedVideo}",
                message = "Property created successfully"
            });
        }

        // ── UPDATE ──
        [HttpPut("UpdateFullProperty", Name = "UpdateFullProperty")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateFullProperty([FromForm] UpdatePropertyRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = clsProperties.FindByID(request.PropertyID);
            if (existing == null) return NotFound(new { message = "Property not found" });

            int currentCount = clsPropertyImages.GetImagesCount(request.PropertyID);
            int finalCount = currentCount - (request.ImageIdsToDelete?.Count ?? 0)
                                            + (request.NewImages?.Count ?? 0);
            if (finalCount > 6)
                return BadRequest(new { message = $"Total images would be {finalCount}. Max 6." });

            var imgErr = ValidateImages(request.NewImages);
            if (imgErr != null) return imgErr;

            var vidErr = ValidateVideo(request.NewVideo);
            if (vidErr != null) return vidErr;

            // =====================================================
            // FIX: جمع مسارات الصور التي سيتم حذفها (قبل حفظ الصور الجديدة)
            // =====================================================
            var allImages = clsPropertyImages.GetImagesByPropertyID(request.PropertyID);
            var imagesToDeleteFromDisk = allImages
                .Where(img => request.ImageIdsToDelete != null &&
                              request.ImageIdsToDelete.Contains(img.ImageId))
                .Select(img => img.ImagePath)
                .ToList();


            var (newImages, newSizes) = await SaveImagesAsync(request.NewImages);
            var (newVideo, newVideoSize) = await SaveVideoAsync(request.NewVideo);

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
                NewImages = newImages,
                NewImageSizes = newSizes,
                ImageIdsToDelete = request.ImageIdsToDelete,
                NewVideoUrl = newVideo,
                NewVideoSize = newVideoSize,
                DeleteVideo = request.DeleteVideo,
                Services = request.Services
            };

            bool updated = await PropertyOrchestratorService.UpdateFullPropertyAsync(dto);
            if (!updated)
                return BadRequest(new { message = "Failed to update property" });

            // =====================================================
            // بعد نجاح التحديث، قم بحذف ملفات الصور القديمة من القرص
            // =====================================================
            foreach (var imgPath in imagesToDeleteFromDisk)
            {
                try
                {
                    var fullPath = Path.Combine(RootPath, imgPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not delete image file {imgPath}: {ex.Message}");
                }
            }

            return Ok(new { message = "Property updated successfully", propertyId = request.PropertyID });
        }

        // ── DELETE ──
        [HttpDelete("DeleteProperty", Name = "DeleteProperty")]
        public IActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid property ID" });
            var existing = clsProperties.FindByID(id);
            if (existing == null) return NotFound(new { message = "Property not found" });
            return clsProperties.Delete(id)
                ? Ok(new { message = "Deleted", propertyID = id })
                : StatusCode(500, new { message = "Delete failed" });
        }

        // ── GET ALL V2 ──
        [HttpGet("GetAllV2", Name = "GetAllV2")]
        public IActionResult GetAllV2()
        {
            var properties = clsProperties.GetAllPropertiesV2();
            if (properties == null || properties.Count == 0)
                return NotFound(new { message = "No properties.", count = 0 });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p =>
                PropertyResponseDTO.FromPropertyV2(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();

            return Ok(new { message = "OK", count = result.Count, properties = result });
        }

        // ── GET BY ID V2 ──
        [HttpGet("GetByIDV2", Name = "GetByIDV2")]
        public IActionResult GetByIDV2(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid ID" });
            var property = clsProperties.FindByIDV2(id);
            if (property == null) return NotFound(new { message = $"Property {id} not found" });
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = PropertyResponseDTO.FromPropertyV2(property, baseUrl,
                clsPropertyImages.GetImagesByPropertyID(id),
                clsPropertyVideo.GetVideoByPropertyID(id),
                clsPropertyServices.GetServicesByPropertyID(id));
            return Ok(new { message = "Found", property = result });
        }

        // ── GET BY OWNER V2 ──
        [HttpGet("GetByOwnerV2", Name = "GetByOwnerV2")]
        public IActionResult GetByOwnerV2(int ownerId)
        {
            if (ownerId <= 0) return BadRequest(new { message = "Invalid owner ID" });
            var properties = clsProperties.GetPropertiesByOwnerIDV2(ownerId);
            if (properties == null || properties.Count == 0)
                return NotFound(new { message = $"No properties for owner {ownerId}" });
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p =>
                PropertyResponseDTO.FromPropertyV2(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();
            return Ok(new { message = "OK", count = result.Count, properties = result });
        }

        // ── GET ALL (V1) ──
        [HttpGet("GetAll", Name = "GetAll")]
        public IActionResult GetAll()
        {
            var properties = clsProperties.GetAllProperties();
            if (properties == null || properties.Count == 0)
                return NotFound(new { message = "No properties.", count = 0 });
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p =>
                PropertyResponseDTO.FromProperty(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();
            return Ok(new { message = "OK", count = result.Count, properties = result });
        }

        // ── GET BY ID (V1) ──
        [HttpGet("GetByID", Name = "GetByID")]
        public IActionResult GetByID(int id)
        {
            if (id <= 0) return BadRequest(new { message = "Invalid ID" });
            var property = clsProperties.FindByID(id);
            if (property == null) return NotFound(new { message = $"Property {id} not found" });
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = PropertyResponseDTO.FromProperty(property, baseUrl,
                clsPropertyImages.GetImagesByPropertyID(id),
                clsPropertyVideo.GetVideoByPropertyID(id),
                clsPropertyServices.GetServicesByPropertyID(id));
            return Ok(new { message = "Found", property = result });
        }

        // ── SEARCH ──
        [HttpGet("Search", Name = "SearchProperties")]
        public IActionResult Search(string city = null, string area = null,
            decimal? minPrice = null, decimal? maxPrice = null)
        {
            if (minPrice < 0) return BadRequest(new { message = "MinPrice cannot be negative" });
            if (maxPrice < 0) return BadRequest(new { message = "MaxPrice cannot be negative" });
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                return BadRequest(new { message = "MinPrice > MaxPrice" });

            var properties = clsProperties.Search(city, area, minPrice, maxPrice);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p =>
                PropertyResponseDTO.FromProperty(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();

            return result.Count == 0
                ? Ok(new { message = "No properties found.", count = 0 })
                : Ok(new { message = "Found", count = result.Count, properties = result });
        }

        // ── GET BY OWNER (V1) ──
        [HttpGet("GetByOwner", Name = "GetByOwner")]
        public IActionResult GetByOwner(int ownerId)
        {
            if (ownerId <= 0) return BadRequest(new { message = "Invalid owner ID" });
            var properties = clsProperties.GetPropertiesByOwnerID(ownerId);
            if (properties == null || properties.Count == 0)
                return NotFound(new { message = $"No properties for owner {ownerId}" });
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p =>
                PropertyResponseDTO.FromProperty(p, baseUrl,
                    clsPropertyImages.GetImagesByPropertyID(p.PropertyID),
                    clsPropertyVideo.GetVideoByPropertyID(p.PropertyID),
                    clsPropertyServices.GetServicesByPropertyID(p.PropertyID))
            ).ToList();
            return Ok(new { message = "OK", count = result.Count, properties = result });
        }

        // ── SEARCH BY UNIVERSITY ──
        [HttpGet("SearchByUniversity")]
        public IActionResult SearchByUniversity(int universityId, decimal? maxPrice = null)
        {
            if (universityId <= 0) return BadRequest(new { message = "Invalid universityId" });
            if (maxPrice < 0) return BadRequest(new { message = "MaxPrice cannot be negative" });
            return _BuildUniversityResponse(clsUniversities.SearchByUniversity(universityId, maxPrice), universityId);
        }

        [HttpGet("SearchByUniversityNearby")]
        public IActionResult SearchByUniversityNearby(int universityId, double maxDistance, decimal? maxPrice = null)
        {
            if (universityId <= 0) return BadRequest(new { message = "Invalid universityId" });
            if (maxDistance <= 0) return BadRequest(new { message = "MaxDistance must be > 0" });
            if (maxPrice < 0) return BadRequest(new { message = "MaxPrice cannot be negative" });
            return _BuildUniversityResponse(clsUniversities.SearchByUniversity(universityId, maxPrice, maxDistance), universityId);
        }

        private IActionResult _BuildUniversityResponse(List<PropertyWithDistanceDTO> results, int universityId)
        {
            if (results.Count == 0)
                return Ok(new { message = "No properties near this university.", count = 0 });

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
                        r.Property.LocationID,
                        r.Property.City,
                        r.Property.Area,
                        r.Property.Street,
                        r.Property.Latitude,
                        r.Property.Longitude
                    },
                    university = new
                    {
                        r.UniversityId,
                        r.UniversityName,
                        lat = r.UniversityLat,
                        lon = r.UniversityLon,
                        distance_km = r.DistanceKm
                    },
                    images = clsPropertyImages.GetImagesByPropertyID(r.Property.PropertyID)
                                   .Select(img => new { img.ImageId, imageUrl = $"{baseUrl}/{img.ImagePath}" }).ToList(),
                    video = video == null ? null : (object)new { video.VideoId, videoUrl = $"{baseUrl}/{video.VideoPath}" },
                    services = clsPropertyServices.GetServicesByPropertyID(r.Property.PropertyID)
                                   .Select(s => new { s.ServiceId, s.Name, s.Icon }).ToList()
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
    }
}