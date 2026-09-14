using Homunity_Buisness_Logic;
using Homunity_Data_Access.Repositories.Models;
using Homunity_Shared_DTOs;
using Homunity_Web_Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using static Homunity_Buisness_Logic.clsUniversities;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/Properties")]
    [ApiController]
    [Authorize]
    public class PropertiesController : AuthorizedControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IPropertyOrchestratorService _orchestrator;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(
            IWebHostEnvironment environment,
            IPropertyOrchestratorService orchestrator,
            ILogger<PropertiesController> logger)
        {
            _environment = environment;
            _orchestrator = orchestrator;
            _logger = logger;
        }

        // ── helpers ── //
        private string RootPath => _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        private static readonly string[] AllowedImageExt = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedVideoExt = { ".mp4", ".webm" };
        private static readonly string[] AllowedImageContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private static readonly string[] AllowedVideoContentTypes = { "video/mp4", "video/webm" };
        private const long MAX_IMAGE_SIZE = 2L * 1024 * 1024;
        private const long MAX_VIDEO_SIZE = 30L * 1024 * 1024;

        private IActionResult ValidateImages(IList<IFormFile> images)
        {
            if (images == null) return null;

            if (images.Count > 6)
                return Problem(
                    detail: "Maximum 6 images allowed.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            foreach (var f in images)
            {
                if (f.Length > MAX_IMAGE_SIZE)
                    return Problem(
                        detail: $"Image '{f.FileName}' exceeds 2MB.",
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Bad Request");

                var ext = Path.GetExtension(f.FileName).ToLower();

                if (!AllowedImageExt.Contains(ext))
                    return Problem(
                        detail: $"Invalid image extension '{f.FileName}'.",
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Bad Request");

                if (string.IsNullOrEmpty(f.ContentType) ||
                    !AllowedImageContentTypes.Contains(f.ContentType.ToLower()))
                    return Problem(
                        detail: $"Invalid or mismatched content type for '{f.FileName}'.",
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Bad Request");
            }

            return null;
        }

        private IActionResult ValidateVideo(IFormFile video)
        {
            if (video == null) return null;

            if (video.Length > MAX_VIDEO_SIZE)
                return Problem(
                    detail: "Video exceeds 30MB.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            var ext = Path.GetExtension(video.FileName).ToLower();

            if (!AllowedVideoExt.Contains(ext))
                return Problem(
                    detail: $"Invalid video extension '{video.FileName}'.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            if (string.IsNullOrEmpty(video.ContentType) ||
                !AllowedVideoContentTypes.Contains(video.ContentType.ToLower()))
                return Problem(
                    detail: $"Invalid or mismatched content type for '{video.FileName}'.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

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
                var name = $"prop_{Guid.NewGuid():N}{ext}";
                var rel = $"image/uploads/properties/{name}";

                await using var stream = new FileStream(
                    Path.Combine(RootPath, rel),
                    FileMode.Create);

                await file.CopyToAsync(stream);

                paths.Add(rel);
                sizes.Add(file.Length);
            }

            return (paths, sizes);
        }

        private async Task<(string path, long size)> SaveVideoAsync(IFormFile video)
        {
            if (video == null || video.Length == 0)
                return (null, 0);

            var folder = Path.Combine(RootPath, "video", "uploads", "properties");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(video.FileName).ToLower();
            var name = $"prop_video_{Guid.NewGuid():N}{ext}";
            var rel = $"video/uploads/properties/{name}";

            await using var stream = new FileStream(
                Path.Combine(RootPath, rel),
                FileMode.Create);

            await video.CopyToAsync(stream);

            return (rel, video.Length);
        }

        // ── CREATE ── //
        /// <summary>
        /// Creates a complete property listing for the authenticated owner or administrator, including optional images and video.
        /// </summary>
        /// <param name="request">The multipart form data containing property details and media files.</param>
        /// <returns>The newly created property ID and public media URLs.</returns>
        /// <response code="201">The property was created successfully.</response>
        /// <response code="400">The property data or uploaded media is invalid.</response>
        [HttpPost("CreateFullProperty", Name = "CreateFullProperty")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Owner,Admin")]
        [EnableRateLimiting("UploadPolicy")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFullProperty(
            [FromForm] CreatePropertyRequest request)
        {
            var imgErr = ValidateImages(request.Images);
            if (imgErr != null) return imgErr;

            var vidErr = ValidateVideo(request.Video);
            if (vidErr != null) return vidErr;

            var (savedImages, imageSizes) = await SaveImagesAsync(request.Images);
            var (savedVideo, videoSize) = await SaveVideoAsync(request.Video);

            var dto = new CreateFullPropertyDTO
            {
                OwnerID = CurrentUserId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Rooms = request.Rooms,
                PropertyType = request.PropertyType,
                City = "",
                Area = "",
                Street = "",
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

            int propertyID = await _orchestrator.CreateFullPropertyAsync(dto);

            if (propertyID <= 0)
                return Problem(
                    detail: "Failed to create property. Check server logs.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Sprint 6: 201 Created + Location header بدل 200 OK
            return CreatedAtAction(
                nameof(GetByIDV2),
                new { id = propertyID },
                new
                {
                    propertyID,
                    images = savedImages.Select(x => $"{baseUrl}/{x}"),
                    video = savedVideo == null ? null : $"{baseUrl}/{savedVideo}",
                    message = "Property created successfully"
                });
        }

        // ── UPDATE ── //
        [HttpPut("UpdateFullProperty", Name = "UpdateFullProperty")]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting("UploadPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFullProperty(
            [FromForm] UpdatePropertyRequest request)
        {
            var existing = clsProperties.FindByID(request.PropertyID);

            if (existing == null)
                return Problem(
                    detail: $"Property {request.PropertyID} not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found");

            if (existing.OwnerID != CurrentUserId && !IsAdmin)
                return Forbid();

            int currentCount = clsPropertyImages.GetImagesCount(request.PropertyID);
            int finalCount = currentCount
                             - (request.ImageIdsToDelete?.Count ?? 0)
                             + (request.NewImages?.Count ?? 0);

            if (finalCount > 6)
                return Problem(
                    detail: $"Total images would be {finalCount}. Max 6.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            var imgErr = ValidateImages(request.NewImages);
            if (imgErr != null) return imgErr;

            var vidErr = ValidateVideo(request.NewVideo);
            if (vidErr != null) return vidErr;

            var allImages = clsPropertyImages.GetImagesByPropertyID(request.PropertyID);

            var imagesToDeleteFromDisk = allImages
                .Where(img =>
                    request.ImageIdsToDelete != null &&
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

            bool updated = await _orchestrator.UpdateFullPropertyAsync(dto);

            if (!updated)
                return Problem(
                    detail: "Failed to update property.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            foreach (var imgPath in imagesToDeleteFromDisk)
            {
                try
                {
                    var fullPath = Path.Combine(
                        RootPath,
                        imgPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Warning: Could not delete image file {imgPath}: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Property updated successfully",
                propertyId = request.PropertyID
            });
        }

        // ── DELETE ── //
        [HttpDelete("DeleteProperty", Name = "DeleteProperty")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return Problem(
                    detail: "Invalid property ID.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            var existing = clsProperties.FindByID(id);

            if (existing == null)
                return Problem(
                    detail: $"Property {id} not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found");

            if (existing.OwnerID != CurrentUserId && !IsAdmin)
                return Forbid();

            bool deleted = clsProperties.Delete(id);

            if (deleted)
            {
                _logger.LogInformation(
                    "Property deleted: PropertyId {PropertyId} by UserId {UserId}",
                    id,
                    CurrentUserId);

                return Ok(new
                {
                    message = "Deleted",
                    propertyID = id
                });
            }

            return Problem(
                detail: "Delete failed.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error");
        }

        // ── GET ALL V2 ── //
        /// <summary>
        /// Retrieves a paginated list of properties with optional sorting.
        /// </summary>
        /// <param name="pageNumber">The page number to return. Defaults to 1.</param>
        /// <param name="pageSize">The number of properties per page. Defaults to 10.</param>
        /// <param name="sortBy">The property field used for sorting, when supplied.</param>
        /// <param name="sortDescending">Indicates whether sorting should be descending.</param>
        /// <returns>A paginated property list.</returns>
        /// <response code="200">The paginated property list was returned successfully.</response>
        [HttpGet("GetAllV2", Name = "GetAllV2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllV2(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            bool sortDescending = false)
        {
            var query = new PropertyListQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _orchestrator.GetPropertiesPagedAsync(query, baseUrl);

            return Ok(result);
        }

        // ── GET BY ID V2 ── //
        /// <summary>
        /// Retrieves a single property by its ID.
        /// </summary>
        /// <param name="id">The ID of the property to retrieve.</param>
        /// <returns>The requested property details.</returns>
        /// <response code="200">The property was found and returned.</response>
        /// <response code="400">The property ID is invalid.</response>
        /// <response code="404">The property was not found.</response>
        [HttpGet("GetByIDV2", Name = "GetByIDV2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIDV2(int id)
        {
            if (id <= 0)
                return Problem(
                    detail: "Invalid ID.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _orchestrator.GetPropertyByIdEfAsync(id, baseUrl);

            if (result == null)
                return Problem(
                    detail: $"Property {id} not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found");

            return Ok(new
            {
                message = "Found",
                property = result
            });
        }

        // ── GET BY OWNER V2 ── //
        [HttpGet("GetByOwnerV2", Name = "GetByOwnerV2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByOwnerV2(
            int ownerId,
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            bool sortDescending = false)
        {
            if (ownerId <= 0)
                return Problem(
                    detail: "Invalid owner ID.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            var query = new PropertyListQuery
            {
                OwnerId = ownerId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _orchestrator.GetPropertiesPagedAsync(query, baseUrl);

            return Ok(result);
        }

        // ── SEARCH ── //
        [HttpGet("Search", Name = "SearchProperties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            string? city = null,
            string? area = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            bool sortDescending = false)
        {
            if (minPrice < 0)
                return Problem(
                    detail: "MinPrice cannot be negative.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            if (maxPrice < 0)
                return Problem(
                    detail: "MaxPrice cannot be negative.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice > maxPrice)
                return Problem(
                    detail: "MinPrice cannot be greater than MaxPrice.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            var query = new PropertyListQuery
            {
                City = city,
                Area = area,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _orchestrator.GetPropertiesPagedAsync(query, baseUrl);

            return Ok(result);
        }

        // ── SEARCH BY UNIVERSITY ── //
        [HttpGet("SearchByUniversity")]
        public IActionResult SearchByUniversity(
            int universityId,
            decimal? maxPrice = null)
        {
            if (universityId <= 0)
                return Problem(
                    detail: "Invalid universityId.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            if (maxPrice < 0)
                return Problem(
                    detail: "MaxPrice cannot be negative.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            return _BuildUniversityResponse(
                clsUniversities.SearchByUniversity(universityId, maxPrice),
                universityId);
        }

        [HttpGet("SearchByUniversityNearby")]
        public IActionResult SearchByUniversityNearby(
            int universityId,
            double maxDistance,
            decimal? maxPrice = null)
        {
            if (universityId <= 0)
                return Problem(
                    detail: "Invalid universityId.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            if (maxDistance <= 0)
                return Problem(
                    detail: "MaxDistance must be greater than 0.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            if (maxPrice < 0)
                return Problem(
                    detail: "MaxPrice cannot be negative.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request");

            return _BuildUniversityResponse(
                clsUniversities.SearchByUniversity(
                    universityId,
                    maxPrice,
                    maxDistance),
                universityId);
        }

        private IActionResult _BuildUniversityResponse(
            List<PropertyWithDistanceDTO> results,
            int universityId)
        {
            if (results.Count == 0)
                return Ok(new
                {
                    message = "No properties near this university.",
                    count = 0
                });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = results
                .Select(r =>
                    PropertyOrchestratorService.BuildUniversityPropertyResponse(
                        r,
                        baseUrl))
                .ToList();

            return Ok(new
            {
                message = "Properties found",
                count = response.Count,
                university = new
                {
                    universityId = results[0].UniversityId,
                    name = results[0].UniversityName
                },
                properties = response
            });
        }
    }
}
