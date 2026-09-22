using Homunity_Shared_DTOs.Properties;
using Homunity_Data_Access.Data;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly HomunityDbContext _db;
        public PropertyRepository(HomunityDbContext db) => _db = db;

        public Task<PropertyEntity> GetByIdWithDetailsAsync(int propertyId) =>
            _db.Properties.AsNoTracking()
                .Include(p => p.Location)
                .Include(p => p.University)
                .Include(p => p.Images)
                .Include(p => p.Videos)
                .Include(p => p.PropertyServices).ThenInclude(ps => ps.Service)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

        public async Task<HashSet<int>> GetValidServiceIdsAsync(IEnumerable<int> serviceIds)
        {
            var ids = serviceIds.Distinct().ToList();
            if (ids.Count == 0) return new HashSet<int>();

            var valid = await _db.Services.AsNoTracking()
                .Where(s => ids.Contains(s.ServiceId))
                .Select(s => s.ServiceId)
                .ToListAsync();

            return valid.ToHashSet();
        }

        public async Task<int> CreatePropertyAsync(PropertyEntity property, List<int> serviceIds)
        {
            _db.Properties.Add(property);

            if (serviceIds != null && serviceIds.Count > 0)
            {
                foreach (var sid in serviceIds.Distinct())
                {
                    property.PropertyServices.Add(new PropertyServiceEntity
                    {
                        ServiceId = sid,
                        Property = property
                    });
                }
            }

            // Atomic: Location + Property + Images + Services في SaveChanges واحدة (Transaction ضمنية)
            await _db.SaveChangesAsync();
            return property.PropertyId;
        }

        public async Task<bool> UpdatePropertyCoreAsync(PropertyUpdateCommand cmd)
        {
            var property = await _db.Properties
                .Include(p => p.Location)
                .Include(p => p.Images)
                .Include(p => p.PropertyServices)
                .FirstOrDefaultAsync(p => p.PropertyId == cmd.PropertyId);

            if (property == null) return false;

            property.Title = cmd.Title;
            property.Description = cmd.Description;
            property.Price = cmd.Price;
            property.Rooms = cmd.Rooms;
            property.PropertyType = cmd.PropertyType;

            if (!string.IsNullOrWhiteSpace(cmd.FullAddress))
                property.FullAddress = cmd.FullAddress;

            if (cmd.UniversityId.HasValue && cmd.UniversityId.Value > 0)
                property.UniversityId = cmd.UniversityId;

            if (cmd.UpdateLocation && property.Location != null)
            {
                property.Location.Latitude = cmd.Latitude;
                property.Location.Longitude = cmd.Longitude;
            }

            if (cmd.ImageIdsToDelete != null && cmd.ImageIdsToDelete.Count > 0)
            {
                var toRemove = property.Images.Where(i => cmd.ImageIdsToDelete.Contains(i.ImageId)).ToList();
                _db.PropertyImages.RemoveRange(toRemove);
            }

            if (cmd.NewImagePaths != null && cmd.NewImagePaths.Count > 0)
            {
                foreach (var path in cmd.NewImagePaths)
                {
                    property.Images.Add(new PropertyImageEntity
                    {
                        PropertyId = property.PropertyId,
                        ImagePath = path,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            if (cmd.ServiceIds != null)
            {
                _db.PropertyServices.RemoveRange(property.PropertyServices);
                foreach (var sid in cmd.ServiceIds.Distinct())
                {
                    _db.PropertyServices.Add(new PropertyServiceEntity
                    {
                        PropertyId = property.PropertyId,
                        ServiceId = sid
                    });
                }
            }

            // Atomic: كل التعديلات فوق في SaveChanges واحدة
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddVideoAsync(int propertyId, string videoPath)
        {
            _db.PropertyVideos.Add(new PropertyVideoEntity
            {
                PropertyId = propertyId,
                VideoPath = videoPath,
                CreatedAt = DateTime.Now
            });
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task DeleteVideosByPropertyIdAsync(int propertyId)
        {
            var videos = await _db.PropertyVideos.Where(v => v.PropertyId == propertyId).ToListAsync();
            if (videos.Count == 0) return;
            _db.PropertyVideos.RemoveRange(videos);
            await _db.SaveChangesAsync();
        }

        public async Task<(List<PropertyListProjection> Items, int TotalCount)> GetPagedAsync(PropertyListQuery query)
        {
            var baseQuery = _db.Properties.AsNoTracking().AsQueryable();

            if (query.OwnerId.HasValue)
            {
                // Owner view: كل عقاراته بغض النظر عن الحالة (Pending/Approved/Rejected)
                baseQuery = baseQuery.Where(p => p.OwnerId == query.OwnerId.Value);
            }
            else
            {
                // Public browse view: المعتمد وغير المحجوز بس
                baseQuery = baseQuery.Where(p => p.StatusId == 2
                    && !_db.Bookings.Any(b => b.PropertyId == p.PropertyId && b.StatusId == 3));
            }

            if (!string.IsNullOrWhiteSpace(query.City))
                baseQuery = baseQuery.Where(p => p.Location.City == query.City);

            if (!string.IsNullOrWhiteSpace(query.Area))
                baseQuery = baseQuery.Where(p => p.Location.Area == query.Area);

            if (query.MinPrice.HasValue)
                baseQuery = baseQuery.Where(p => p.Price >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue)
                baseQuery = baseQuery.Where(p => p.Price <= query.MaxPrice.Value);

            var totalCount = await baseQuery.CountAsync();

            baseQuery = query.SortBy?.ToLower() switch
            {
                "price" => query.SortDescending ? baseQuery.OrderByDescending(p => p.Price) : baseQuery.OrderBy(p => p.Price),
                "date" => query.SortDescending ? baseQuery.OrderByDescending(p => p.CreatedAt) : baseQuery.OrderBy(p => p.CreatedAt),
                _ => baseQuery.OrderByDescending(p => p.CreatedAt)
            };

            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 10 : Math.Min(query.PageSize, 50);

            var items = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PropertyListProjection
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    Price = p.Price,
                    Rooms = p.Rooms,
                    PropertyType = p.PropertyType,
                    City = p.Location.City,
                    Area = p.Location.Area,
                    CreatedAt = p.CreatedAt,
                    MainImagePath = p.Images.OrderBy(i => i.CreatedAt).Select(i => i.ImagePath).FirstOrDefault(),
                    UniversityId = p.UniversityId,
                    UniversityName = p.University != null ? p.University.Name : null
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<PropertyChatProjection>> GetActiveForChatAsync(int maxCount)
        {
            return await _db.Properties.AsNoTracking()
                .Where(p => p.StatusId == 2 && !_db.Bookings.Any(b => b.PropertyId == p.PropertyId && b.StatusId == 3))
                .OrderByDescending(p => p.CreatedAt)
                .Take(maxCount)
                .Select(p => new PropertyChatProjection
                {
                    PropertyId = p.PropertyId,
                    Title = p.Title,
                    Price = p.Price,
                    Rooms = p.Rooms,
                    PropertyType = p.PropertyType,
                    Address = p.FullAddress,
                    UniversityId = p.UniversityId,
                    UniversityName = p.University != null ? p.University.Name : null,
                    MainImagePath = p.Images.OrderBy(i => i.CreatedAt).Select(i => i.ImagePath).FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<(int OwnerId, int LocationId)?> GetOwnershipAsync(int propertyId)
        {
            var result = await _db.Properties.AsNoTracking()
                .Where(p => p.PropertyId == propertyId)
                .Select(p => new { p.OwnerId, p.LocationId })
                .FirstOrDefaultAsync();

            return result == null ? null : (result.OwnerId, result.LocationId);
        }

        public Task<List<PropertyImageProjection>> GetImagesByPropertyIdAsync(int propertyId) =>
    _db.PropertyImages.AsNoTracking()
        .Where(i => i.PropertyId == propertyId)
        .OrderBy(i => i.CreatedAt)
        .Select(i => new PropertyImageProjection { ImageId = i.ImageId, PropertyId = i.PropertyId, ImagePath = i.ImagePath, CreatedAt = i.CreatedAt })
        .ToListAsync();

        public Task<PropertyImageProjection?> FindImageByIdAsync(int imageId) =>
            _db.PropertyImages.AsNoTracking()
                .Where(i => i.ImageId == imageId)
                .Select(i => new PropertyImageProjection { ImageId = i.ImageId, PropertyId = i.PropertyId, ImagePath = i.ImagePath, CreatedAt = i.CreatedAt })
                .FirstOrDefaultAsync();

        public Task<int> CountImagesAsync(int propertyId) =>
            _db.PropertyImages.AsNoTracking().CountAsync(i => i.PropertyId == propertyId);

        public Task<PropertyVideoProjection?> GetVideoByPropertyIdAsync(int propertyId) =>
            _db.PropertyVideos.AsNoTracking()
                .Where(v => v.PropertyId == propertyId)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new PropertyVideoProjection { VideoId = v.VideoId, PropertyId = v.PropertyId, VideoPath = v.VideoPath, CreatedAt = v.CreatedAt })
                .FirstOrDefaultAsync();

        public Task<PropertyVideoProjection?> FindVideoByIdAsync(int videoId) =>
            _db.PropertyVideos.AsNoTracking()
                .Where(v => v.VideoId == videoId)
                .Select(v => new PropertyVideoProjection { VideoId = v.VideoId, PropertyId = v.PropertyId, VideoPath = v.VideoPath, CreatedAt = v.CreatedAt })
                .FirstOrDefaultAsync();

        public async Task<bool> DeletePropertyCascadeAsync(int propertyId)
        {
            var exists = await _db.Properties.AsNoTracking().AnyAsync(p => p.PropertyId == propertyId);
            if (!exists) return false;

            // Explicit transaction is required here: six independent ExecuteDeleteAsync bulk statements
            // (not covered by a single SaveChangesAsync/change tracker) must succeed or fail as one unit,
            // otherwise a partial cascade leaves orphaned rows. This mirrors the exact table order and
            // scope of the original clsPropertiesData.DeleteProperty implementation.
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.PropertyImages.Where(i => i.PropertyId == propertyId).ExecuteDeleteAsync();
                await _db.PropertyVideos.Where(v => v.PropertyId == propertyId).ExecuteDeleteAsync();
                await _db.PropertyServices.Where(ps => ps.PropertyId == propertyId).ExecuteDeleteAsync();

                // Payments reference Booking (Restrict) and Property — must delete before Bookings/Property
                await _db.Payments.Where(pay => pay.PropertyId == propertyId).ExecuteDeleteAsync();

                await _db.Bookings.Where(b => b.PropertyId == propertyId).ExecuteDeleteAsync();

                // AdminActions has no EF entity (table confirmed write-dead across the whole solution —
                // see Sprint 1 verification report — no INSERT path exists anywhere). Kept as one
                // parameterized raw statement for schema-safety, same precedent as the Payment
                // UPDLOCK exception from Sprint 1, rather than modeling a whole new entity for a
                // single defensive cleanup line on a table nothing else touches.
                await _db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM AdminActions WHERE PropertyId = {propertyId}");

                var rows = await _db.Properties.Where(p => p.PropertyId == propertyId).ExecuteDeleteAsync();

                await transaction.CommitAsync();
                return rows > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}