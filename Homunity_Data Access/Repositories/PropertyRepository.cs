using Homunity_Data_Access.Data;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                // Public browse view: المعتمد وغير المحجوز بس (نفس فلتر الأصل الـADO.NET بالظبط)
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
    }

}
