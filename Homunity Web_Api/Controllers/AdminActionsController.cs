using Homunity_Buisness_Logic;
using Homunity_Shared_DTOs.AdminActions;
using Homunity_Web_Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homunity_Web_Api.Controllers
{
    [Route("api/AdminActions")]
    [ApiController]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public class AdminActionsController : AuthorizedControllerBase
    {
        private readonly IAdminActionsService _adminService;
        public AdminActionsController(IAdminActionsService adminService) => _adminService = adminService;

        [HttpPut("properties/{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminActionResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var adminId = CurrentUserId;
            if (adminId <= 0) return Unauthorized();

            var property = await _adminService.GetPropertyDetailAsync(id);
            if (property == null) return Problem(detail: $"Property with ID {id} not found", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            if (property.StatusId != 1)
                return Problem(detail: "Cannot approve property. Only Pending properties can be approved.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            if (await _adminService.ApproveAsync(id, adminId))
            {
                return Ok(new AdminActionResponse
                {
                    PropertyId = id,
                    AdminId = adminId,
                    Action = "Approved",
                    RejectReason = null,
                    Timestamp = DateTime.Now,
                    Message = "Property approved successfully"
                });
            }

            return Problem(detail: "Error approving property. Admin may not be valid.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");
        }

        [HttpPut("properties/{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminActionResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectProperty(int id, string reason)
        {
            if (id <= 0) return Problem(detail: "Invalid property ID", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            var adminId = CurrentUserId;
            if (adminId <= 0) return Unauthorized();
            if (string.IsNullOrWhiteSpace(reason)) return Problem(detail: "Reject reason is required", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (reason.Trim().Length < 10) return Problem(detail: "Reject reason must be at least 10 characters", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var property = await _adminService.GetPropertyDetailAsync(id);
            if (property == null) return Problem(detail: $"Property with ID {id} not found", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            if (property.StatusId != 1)
                return Problem(detail: "Cannot reject property. Only Pending properties can be rejected.", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            if (await _adminService.RejectAsync(id, adminId, reason))
            {
                return Ok(new AdminActionResponse
                {
                    PropertyId = id,
                    AdminId = adminId,
                    Action = "Rejected",
                    RejectReason = reason.Trim(),
                    Timestamp = DateTime.Now,
                    Message = "Property rejected successfully"
                });
            }

            return Problem(detail: "Error rejecting property. Admin may not be valid.", statusCode: StatusCodes.Status500InternalServerError, title: "Server Error");
        }

        [HttpGet("properties/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPendingProperties(int page = 1, int pageSize = 10)
        {
            if (page <= 0) return Problem(detail: "Page must be greater than 0", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");
            if (pageSize <= 0 || pageSize > 50) return Problem(detail: "PageSize must be between 1 and 50", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var (properties, totalCount) = await _adminService.GetPendingPropertiesAsync(page, pageSize);
            if (properties.Count == 0) return Problem(detail: "No pending properties found", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var items = properties.Select(p => new AdminPendingPropertyResponse
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                OwnerName = p.OwnerName,
                Description = p.Description,
                Price = p.Price,
                Rooms = p.Rooms,
                PropertyType = p.PropertyType,
                StatusId = p.StatusId,
                CreatedAt = p.CreatedAt,
                Location = new AdminPropertyLocationInfo { LocationId = p.LocationId, City = p.City, Area = p.Area },
                Thumbnail = p.Thumbnail == null ? null : $"{baseUrl}/{p.Thumbnail}"
            }).ToList();

            // Sprint 3 (بند 7 — Pagination responses): استخدام PagedResult<T> الموحّدة الموجودة
            // أصلًا في المشروع بدل الغلاف اليدوي (page/pageSize/totalPages) المختلف عن باقي الـAPI.
            var pagination = new Homunity_Shared_DTOs.PagedResult<AdminPendingPropertyResponse>
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };

            return Ok(new { message = "Pending properties retrieved successfully", pagination });
        }

        [HttpGet("properties/rejected")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRejectedProperties()
        {
            var properties = await _adminService.GetRejectedPropertiesAsync();
            if (properties.Count == 0) return Problem(detail: "No rejected properties found", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p => new AdminRejectedPropertyResponse
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Rooms = p.Rooms,
                PropertyType = p.PropertyType,
                StatusId = p.StatusId,
                RejectReason = p.RejectReason,
                CreatedAt = p.CreatedAt,
                Location = new AdminPropertyLocationInfo { LocationId = p.LocationId, City = p.City, Area = p.Area },
                Thumbnail = p.Thumbnail == null ? null : $"{baseUrl}/{p.Thumbnail}"
            }).ToList();

            return Ok(new { message = "Rejected properties retrieved successfully", count = result.Count, properties = result });
        }

        [HttpGet("properties/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPropertyDetails(int id)
        {
            if (id <= 0) return Problem(detail: "Invalid property ID", statusCode: StatusCodes.Status400BadRequest, title: "Bad Request");

            var property = await _adminService.GetPropertyDetailAsync(id);
            if (property == null) return Problem(detail: $"Property with ID {id} not found", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var response = new AdminPropertyDetailResponse
            {
                PropertyId = property.PropertyId,
                OwnerId = property.OwnerId,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                Rooms = property.Rooms,
                PropertyType = property.PropertyType,
                StatusId = property.StatusId,
                RejectReason = string.IsNullOrWhiteSpace(property.RejectReason) ? null : property.RejectReason,
                CreatedAt = property.CreatedAt,
                Location = new AdminPropertyLocationInfo { LocationId = property.LocationId, City = property.City, Area = property.Area },
                Images = property.Images.Count == 0 ? null : property.Images.Select(img => new AdminPropertyImageInfo { ImageId = img.ImageId, ImageUrl = $"{baseUrl}/{img.ImagePath}" }).ToList(),
                Video = property.Video == null ? null : new AdminPropertyVideoInfo { VideoId = property.Video.Value.VideoId, VideoUrl = $"{baseUrl}/{property.Video.Value.VideoPath}" }
            };

            return Ok(new { message = "Property found", property = response });
        }

        [HttpGet("dashboard/stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            var response = new DashboardStatsResponse
            {
                TotalProperties = stats.TotalProperties,
                PendingProperties = stats.PendingProperties,
                ApprovedProperties = stats.ApprovedProperties,
                RejectedProperties = stats.RejectedProperties,
                TotalBookings = stats.TotalBookings,
                TotalUsers = stats.TotalUsers
            };

            return Ok(new { message = "Dashboard stats retrieved successfully", stats = response });
        }

        [HttpGet("dashboard/recent-actions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRecentActions(int pageSize = 10)
        {
            var properties = await _adminService.GetRecentActionsAsync(pageSize);
            if (properties.Count == 0) return Problem(detail: "No actions found", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = properties.Select(p => new AdminRecentActionResponse
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                OwnerName = p.OwnerName,
                Location = new AdminPropertyLocationInfo { City = p.City, Area = p.Area },
                ActionType = p.StatusId == 2 ? "Approved" : "Rejected",
                StatusId = p.StatusId,
                CreatedAt = p.CreatedAt,
                Thumbnail = p.Thumbnail == null ? null : $"{baseUrl}/{p.Thumbnail}"
            }).ToList();

            return Ok(new { message = "Recent actions retrieved successfully", count = result.Count, properties = result });
        }
    }
}