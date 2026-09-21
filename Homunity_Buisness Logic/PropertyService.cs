using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using Homunity_Data_Access.Repositories.Models;
using Homunity_Shared_DTOs;
using Homunity_Shared_DTOs.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homunity_Buisness_Logic
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly ILogger<PropertyService> _logger;

        public PropertyService(
            IPropertyRepository propertyRepository,
            ILogger<PropertyService> logger)
        {
            _propertyRepository = propertyRepository;
            _logger = logger;
        }

        // ================= GetByID عبر EF Core ================= //
        public async Task<PropertyResponseDTO> GetPropertyByIdEfAsync(
            int propertyId,
            string baseUrl)
        {
            var entity = await _propertyRepository.GetByIdWithDetailsAsync(propertyId);

            if (entity == null)
                return null;

            var latestVideo = entity.Videos?
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefault();

            string fullAddress = entity.FullAddress;

            if (string.IsNullOrWhiteSpace(fullAddress) &&
                entity.Location != null)
            {
                var parts = new[]
                {
                    entity.Location.City,
                    entity.Location.Area,
                    entity.Location.Street
                };

                fullAddress = string.Join(
                    ", ",
                    parts.Where(p => !string.IsNullOrWhiteSpace(p)));
            }

            return new PropertyResponseDTO
            {
                PropertyID = entity.PropertyId,
                OwnerID = entity.OwnerId,
                Title = entity.Title,
                Description = entity.Description,
                Price = entity.Price,
                Rooms = entity.Rooms,
                PropertyType = entity.PropertyType,
                PropertyStatusID = entity.StatusId,
                RejectReason = entity.RejectReason,
                CreatedAt = entity.CreatedAt,
                FullAddress = fullAddress,

                Location = new
                {
                    locationId = entity.LocationId,
                    city = entity.Location?.City,
                    area = entity.Location?.Area,
                    street = entity.Location?.Street,
                    latitude = entity.Location?.Latitude,
                    longitude = entity.Location?.Longitude,

                    university = entity.University == null
                        ? null
                        : new
                        {
                            universityId = entity.University.UniversityId,
                            name = entity.University.Name
                        }
                },

                Images = entity.Images == null || entity.Images.Count == 0
                    ? null
                    : (object)entity.Images.Select(img => new
                    {
                        imageId = img.ImageId,
                        imageUrl = $"{baseUrl}/{img.ImagePath}"
                    }).ToList(),

                Video = latestVideo == null
                    ? null
                    : (object)new
                    {
                        videoId = latestVideo.VideoId,
                        videoUrl = $"{baseUrl}/{latestVideo.VideoPath}"
                    },

                Services = entity.PropertyServices == null ||
                           entity.PropertyServices.Count == 0
                    ? null
                    : (object)entity.PropertyServices.Select(ps => new
                    {
                        serviceId = ps.Service.ServiceId,
                        name = ps.Service.Name,
                        icon = ps.Service.Icon
                    }).ToList()
            };
        }

        // ================= CreateFullProperty عبر EF Core ================= //
        public async Task<int> CreateFullPropertyAsync(
            CreateFullPropertyDTO dto)
        {
            _logger.LogInformation(
                "CreateProperty attempt for OwnerId {OwnerId}",
                dto.OwnerID);

            if (dto.Images != null &&
                dto.ImageSizes != null &&
                dto.Images.Count != dto.ImageSizes.Count)
            {
                _logger.LogWarning(
                    "CreateProperty failed: Images/ImageSizes count mismatch for OwnerId {OwnerId}",
                    dto.OwnerID);

                return -1;
            }

            if (dto.Services != null &&
                dto.Services.Count > 0)
            {
                var validIds =
                    await _propertyRepository.GetValidServiceIdsAsync(
                        dto.Services);

                var invalid =
                    dto.Services.FirstOrDefault(
                        id => !validIds.Contains(id));

                if (invalid != 0)
                {
                    _logger.LogWarning(
                        "CreateProperty failed: invalid ServiceId {ServiceId}",
                        invalid);

                    return -1;
                }
            }

            var locationEntity = new LocationEntity
            {
                City = string.IsNullOrWhiteSpace(dto.City)
                    ? "Default City"
                    : dto.City,

                Area = string.IsNullOrWhiteSpace(dto.Area)
                    ? "Default Area"
                    : dto.Area,

                Street = dto.Street ?? "",

                Latitude = dto.Latitude != 0
                    ? dto.Latitude
                    : (double?)null,

                Longitude = dto.Longitude != 0
                    ? dto.Longitude
                    : (double?)null
            };

            var propertyEntity = new PropertyEntity
            {
                OwnerId = dto.OwnerID,
                Title = dto.Title,
                Description = dto.Description ?? "",
                Price = dto.Price,
                Rooms = dto.Rooms,
                PropertyType = dto.PropertyType,
                StatusId = 1, // Pending — نفس قيمة الأصل الثابتة
                CreatedAt = DateTime.Now,
                Location = locationEntity,

                FullAddress = string.IsNullOrWhiteSpace(dto.Address)
                    ? null
                    : dto.Address,

                UniversityId = dto.UniversityId > 0
                    ? dto.UniversityId
                    : (int?)null
            };

            if (dto.Images != null)
            {
                for (int i = 0; i < dto.Images.Count; i++)
                {
                    long size =
                        (dto.ImageSizes != null &&
                         i < dto.ImageSizes.Count)
                            ? dto.ImageSizes[i]
                            : 0;

                    if (!IsValidImage(dto.Images[i], size))
                    {
                        _logger.LogWarning(
                            "CreateProperty failed: invalid image for OwnerId {OwnerId}",
                            dto.OwnerID);

                        return -1;
                    }

                    propertyEntity.Images.Add(
                        new PropertyImageEntity
                        {
                            ImagePath = dto.Images[i],
                            CreatedAt = DateTime.Now
                        });
                }
            }

            int propertyId;

            try
            {
                propertyId =
                    await _propertyRepository.CreatePropertyAsync(
                        propertyEntity,
                        dto.Services);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CreateProperty error for OwnerId {OwnerId}",
                    dto.OwnerID);

                return -1;
            }

            // Video — Non-fatal، نفس سلوك الأصل بالظبط
            if (!string.IsNullOrEmpty(dto.VideoUrl))
            {
                try
                {
                    await _propertyRepository.AddVideoAsync(
                        propertyId,
                        dto.VideoUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Video save failed for PropertyId {PropertyId}",
                        propertyId);
                }
            }

            _logger.LogInformation(
                "Property created successfully: PropertyId {PropertyId}, OwnerId {OwnerId}",
                propertyId,
                dto.OwnerID);

            return propertyId;
        }

        // ================= UpdateFullProperty عبر EF Core ================= //
        public async Task<bool> UpdateFullPropertyAsync(
            UpdateFullPropertyDTO dto)
        {
            _logger.LogInformation(
                "UpdateProperty attempt for PropertyId {PropertyId}",
                dto.PropertyID);

            if (dto.NewImages != null &&
                dto.NewImageSizes != null &&
                dto.NewImages.Count != dto.NewImageSizes.Count)
            {
                _logger.LogWarning(
                    "UpdateProperty failed: NewImages/NewImageSizes count mismatch for PropertyId {PropertyId}",
                    dto.PropertyID);

                return false;
            }

            if (dto.Services != null &&
                dto.Services.Count > 0)
            {
                var validIds =
                    await _propertyRepository.GetValidServiceIdsAsync(
                        dto.Services);

                var invalid =
                    dto.Services.FirstOrDefault(
                        id => !validIds.Contains(id));

                if (invalid != 0)
                {
                    _logger.LogWarning(
                        "UpdateProperty failed: invalid ServiceId {ServiceId}",
                        invalid);

                    return false;
                }
            }

            if (dto.NewImages != null)
            {
                for (int i = 0; i < dto.NewImages.Count; i++)
                {
                    long size =
                        (dto.NewImageSizes != null &&
                         i < dto.NewImageSizes.Count)
                            ? dto.NewImageSizes[i]
                            : 0;

                    if (!IsValidImage(dto.NewImages[i], size))
                    {
                        _logger.LogWarning(
                            "UpdateProperty failed: invalid image for PropertyId {PropertyId}",
                            dto.PropertyID);

                        return false;
                    }
                }
            }

            var command = new PropertyUpdateCommand
            {
                PropertyId = dto.PropertyID,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Rooms = dto.Rooms,
                PropertyType = dto.PropertyType,
                FullAddress = dto.Address,

                UniversityId = dto.UniversityId > 0
                    ? dto.UniversityId
                    : (int?)null,

                UpdateLocation =
                    dto.Latitude != 0 &&
                    dto.Longitude != 0,

                Latitude = dto.Latitude,
                Longitude = dto.Longitude,

                ImageIdsToDelete =
                    dto.ImageIdsToDelete ?? new List<int>(),

                NewImagePaths =
                    dto.NewImages ?? new List<string>(),

                ServiceIds = dto.Services
            };

            bool updated;

            try
            {
                updated =
                    await _propertyRepository.UpdatePropertyCoreAsync(
                        command);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "UpdateProperty error for PropertyId {PropertyId}",
                    dto.PropertyID);

                return false;
            }

            if (!updated)
            {
                _logger.LogWarning(
                    "UpdateProperty failed: PropertyId {PropertyId} not found or update rejected",
                    dto.PropertyID);

                return false;
            }

            if (dto.DeleteVideo ||
                !string.IsNullOrEmpty(dto.NewVideoUrl))
            {
                await _propertyRepository.DeleteVideosByPropertyIdAsync(
                    dto.PropertyID);
            }

            if (!string.IsNullOrEmpty(dto.NewVideoUrl))
            {
                try
                {
                    await _propertyRepository.AddVideoAsync(
                        dto.PropertyID,
                        dto.NewVideoUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Video update failed for PropertyId {PropertyId}",
                        dto.PropertyID);
                }
            }

            _logger.LogInformation(
                "Property updated successfully: PropertyId {PropertyId}",
                dto.PropertyID);

            return true;
        }

        // ================= Sprint 5: Paginated List ================= //
        public async Task<PagedResult<PropertyListItemDto>> GetPropertiesPagedAsync(
            PropertyListQuery query,
            string baseUrl)
        {
            var (items, totalCount) =
                await _propertyRepository.GetPagedAsync(query);

            var mapped = items.Select(p => new PropertyListItemDto
            {
                PropertyID = p.PropertyId,
                Title = p.Title,
                Price = p.Price,
                Rooms = p.Rooms,
                PropertyType = p.PropertyType,
                City = p.City,
                Area = p.Area,
                CreatedAt = p.CreatedAt,

                MainImageUrl =
                    string.IsNullOrEmpty(p.MainImagePath)
                        ? null
                        : $"{baseUrl}/{p.MainImagePath}",

                UniversityId = p.UniversityId,
                UniversityName = p.UniversityName
            }).ToList();

            return new PagedResult<PropertyListItemDto>
            {
                PageNumber =
                    query.PageNumber <= 0
                        ? 1
                        : query.PageNumber,

                PageSize =
                    query.PageSize <= 0
                        ? 10
                        : Math.Min(query.PageSize, 50),

                TotalCount = totalCount,
                Items = mapped
            };
        }

        // ================= Sprint 1.5: Ownership ================= //
        public async Task<PropertyOwnershipInfo?> GetOwnershipAsync(
            int propertyId)
        {
            var ownership =
                await _propertyRepository.GetOwnershipAsync(propertyId);

            if (ownership == null)
                return null;

            return new PropertyOwnershipInfo
            {
                OwnerId = ownership.Value.OwnerId,
                LocationId = ownership.Value.LocationId
            };
        }

        // ================= Sprint 1.5: Delete ================= //
        public async Task<PropertyDeleteOutcome> DeletePropertyAsync(
            int propertyId,
            int currentUserId,
            bool isAdmin)
        {
            var ownership =
                await _propertyRepository.GetOwnershipAsync(propertyId);

            if (ownership == null)
            {
                return new PropertyDeleteOutcome
                {
                    Status = PropertyDeleteStatus.NotFound,
                    PropertyId = propertyId
                };
            }

            if (ownership.Value.OwnerId != currentUserId &&
                !isAdmin)
            {
                return new PropertyDeleteOutcome
                {
                    Status = PropertyDeleteStatus.Forbidden,
                    PropertyId = propertyId
                };
            }

            var deleted =
                await _propertyRepository.DeletePropertyCascadeAsync(
                    propertyId);

            return new PropertyDeleteOutcome
            {
                Status = deleted
                    ? PropertyDeleteStatus.Success
                    : PropertyDeleteStatus.Failed,

                PropertyId = propertyId
            };
        }

        // ================= Helper ================= //
        private static bool IsValidImage(
            string imagePath,
            long fileSizeBytes)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return false;

            var ext =
                Path.GetExtension(
                    imagePath.Split('?')[0])
                    .ToLower();

            if (string.IsNullOrEmpty(ext))
            {
                ext =
                    Path.GetExtension(
                        imagePath.Split('/').Last())
                        .ToLower();
            }

            var allowed =
                new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

            if (!allowed.Contains(ext))
                return false;

            const long MAX = 2L * 1024 * 1024;

            if (fileSizeBytes > 0 &&
                fileSizeBytes > MAX)
                return false;

            return true;
        }
        public Task<List<PropertyImageProjection>> GetImagesByPropertyIdAsync(int propertyId)
            => _propertyRepository.GetImagesByPropertyIdAsync(propertyId);

        public Task<PropertyImageProjection?> FindImageByIdAsync(int imageId)
            => _propertyRepository.FindImageByIdAsync(imageId);

        public Task<int> CountImagesAsync(int propertyId)
            => _propertyRepository.CountImagesAsync(propertyId);

        public Task<PropertyVideoProjection?> GetVideoByPropertyIdAsync(int propertyId)
            => _propertyRepository.GetVideoByPropertyIdAsync(propertyId);

        public Task<PropertyVideoProjection?> FindVideoByIdAsync(int videoId)
            => _propertyRepository.FindVideoByIdAsync(videoId);

    }
}
