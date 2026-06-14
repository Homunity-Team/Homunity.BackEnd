using Homunity_Buisness_Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Homunity_Business_Logic;// لو موجود بالفعل


namespace Homunity_Web_Api.Controllers
{
    [Route("api/AdminActions")]
    [ApiController]
    public class AdminActionsController : ControllerBase
    {
        

        // =============================================
        // PUT: api/admin/properties/{id}/approve
        // =============================================
        [HttpPut("properties/{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ApproveProperty(int id, int adminId)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            if (adminId <= 0)
                return BadRequest(new { message = "Invalid admin ID" });

            // Check property exists
            var property = clsProperties.FindByID(id);
            if (property == null)
                return NotFound(new { message = $"Property with ID {id} not found" });

            // Check current status
            if (property.PropertyStatusID != 1)
                return BadRequest(new
                {
                    message = "Cannot approve property. Only Pending properties can be approved."
                });

            if (clsProperties.Approve(id, adminId))
            {
                return Ok(new
                {
                    message = "Property approved successfully",
                    propertyID = id,
                    adminId = adminId,
                    approvedAt = DateTime.Now
                });
            }

            return StatusCode(500, new
            {
                message = "Error approving property. Admin may not be valid."
            });
        }





        // =============================================
        // PUT: api/admin/properties/{id}/reject
        // =============================================
        [HttpPut("properties/{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RejectProperty(int id, int adminId, string reason)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            if (adminId <= 0)
                return BadRequest(new { message = "Invalid admin ID" });

            if (string.IsNullOrWhiteSpace(reason))
                return BadRequest(new { message = "Reject reason is required" });

            if (reason.Trim().Length < 10)
                return BadRequest(new { message = "Reject reason must be at least 10 characters" });

            // Check property exists
            var property = clsProperties.FindByID(id);
            if (property == null)
                return NotFound(new { message = $"Property with ID {id} not found" });

            // Check current status
            if (property.PropertyStatusID != 1)
                return BadRequest(new
                {
                    message = "Cannot reject property. Only Pending properties can be rejected."
                });

            if (clsProperties.Reject(id, adminId, reason))
            {
                return Ok(new
                {
                    message = "Property rejected successfully",
                    propertyID = id,
                    adminId = adminId,
                    rejectReason = reason.Trim(),
                    rejectedAt = DateTime.Now
                });
            }

            return StatusCode(500, new
            {
                message = "Error rejecting property. Admin may not be valid."
            });
        }



        // =============================================
        // GET: api/admin/properties/pending
        // =============================================
        [HttpGet("properties/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPendingProperties(int page = 1, int pageSize = 10)
        {
            if (page <= 0)
                return BadRequest(new { message = "Page must be greater than 0" });

            if (pageSize <= 0 || pageSize > 50)
                return BadRequest(new { message = "PageSize must be between 1 and 50" });

            var (properties, totalCount) = clsProperties.GetPendingProperties(page, pageSize);

            if (properties == null || properties.Count == 0)
                return NotFound(new { message = "No pending properties found" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = properties.Select(p =>
            {
                var thumbnail = clsPropertyImages.GetFirstImageByPropertyID(p.PropertyID);

                return new
                {
                    propertyID = p.PropertyID,
                    title = p.Title,
                    ownerName = p.OwnerName,
                    description = p.Description,
                    price = p.Price,
                    rooms = p.Rooms,
                    propertyType = p.PropertyType,
                    statusID = p.PropertyStatusID,
                    createdAt = p.CreatedAt,
                    location = new
                    {
                        locationId = p.LocationID,
                        city = p.City,
                        area = p.Area
                    },
                    thumbnail = thumbnail == null
                        ? null
                        : $"{baseUrl}/{thumbnail.ImagePath}"
                };
            }).ToList();

            return Ok(new
            {
                message = "Pending properties retrieved successfully",
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                properties = result
            });
        }



        // =============================================
        // GET: api/admin/properties/rejected
        // =============================================
        [HttpGet("properties/rejected")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetRejectedProperties()
        {
            var properties = clsProperties.GetRejectedProperties();

            if (properties == null || properties.Count == 0)
                return NotFound(new { message = "No rejected properties found" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = properties.Select(p =>
            {
                var thumbnail = clsPropertyImages.GetFirstImageByPropertyID(p.PropertyID);

                return new
                {
                    propertyID = p.PropertyID,
                    title = p.Title,
                    description = p.Description,
                    price = p.Price,
                    rooms = p.Rooms,
                    propertyType = p.PropertyType,
                    statusID = p.PropertyStatusID,
                    rejectReason = p.RejectReason,
                    createdAt = p.CreatedAt,
                    location = new
                    {
                        locationId = p.LocationID,
                        city = p.City,
                        area = p.Area
                    },
                    thumbnail = thumbnail == null
                        ? null
                        : $"{baseUrl}/{thumbnail.ImagePath}"
                };
            }).ToList();

            return Ok(new
            {
                message = "Rejected properties retrieved successfully",
                count = result.Count,
                properties = result
            });
        }





        // =============================================
        // GET: api/admin/properties/{id}
        // =============================================
        [HttpGet("properties/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPropertyDetails(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid property ID" });

            var property = clsProperties.FindByID(id);

            if (property == null)
                return NotFound(new { message = $"Property with ID {id} not found" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var images = clsPropertyImages.GetImagesByPropertyID(id);
            var video = clsPropertyVideo.GetVideoByPropertyID(id);

            return Ok(new
            {
                message = "Property found",
                property = new
                {
                    propertyID = property.PropertyID,
                    ownerID = property.OwnerID,
                    title = property.Title,
                    description = property.Description,
                    price = property.Price,
                    rooms = property.Rooms,
                    propertyType = property.PropertyType,
                    statusID = property.PropertyStatusID,
                    rejectReason = string.IsNullOrWhiteSpace(property.RejectReason)
                        ? null
                        : property.RejectReason,
                    createdAt = property.CreatedAt,
                    location = new
                    {
                        locationId = property.LocationID,
                        city = property.City,
                        area = property.Area
                    },
                    images = images == null || images.Count == 0
                        ? null
                        : (object)images.Select(img => new
                        {
                            imageId = img.ImageId,
                            imageUrl = $"{baseUrl}/{img.ImagePath}"
                        }).ToList(),
                    video = video == null
                        ? null
                        : (object)new
                        {
                            videoId = video.VideoId,
                            videoUrl = $"{baseUrl}/{video.VideoPath}"
                        }
                }
            });
        }









        // =============================================
        // GET: api/admin/dashboard/stats
        // =============================================
        [HttpGet("dashboard/stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetDashboardStats()
        {
            var stats = clsAdminActions.GetDashboardStats();

            return Ok(new
            {
                message = "Dashboard stats retrieved successfully",
                stats = stats
            });
        }





        // =============================================
        // GET: api/AdminActions/dashboard/recent-actions
        // =============================================
        [HttpGet("dashboard/recent-actions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetRecentActions(int pageSize = 10)
        {
            var properties = clsAdminActions.GetRecentActions(pageSize);

            if (properties == null || properties.Count == 0)
                return NotFound(new { message = "No actions found" });

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = properties.Select(p =>
            {
                var thumbnail = clsPropertyImages.GetFirstImageByPropertyID(p.PropertyID);

                return new
                {
                    propertyID = p.PropertyID,
                    title = p.Title,
                    ownerName = p.OwnerName,
                    location = new
                    {
                        city = p.City,
                        area = p.Area
                    },
                    actionType = p.PropertyStatusID == 2 ? "Approved" : "Rejected",
                    statusID = p.PropertyStatusID,
                    createdAt = p.CreatedAt,
                    thumbnail = thumbnail == null
                        ? null
                        : $"{baseUrl}/{thumbnail.ImagePath}"
                };
            }).ToList();

            return Ok(new
            {
                message = "Recent actions retrieved successfully",
                count = result.Count,
                properties = result
            });
        }
 
        
    }
}
