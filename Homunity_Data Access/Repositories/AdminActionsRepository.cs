using Homunity_Data_Access.Data;
using Homunity_Data_Access.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Homunity_Data_Access.Repositories
{
    public class AdminActionsRepository : IAdminActionsRepository
    {
        private readonly HomunityDbContext _db;
        public AdminActionsRepository(HomunityDbContext db) => _db = db;

        public async Task<DashboardStatsResult> GetDashboardStatsAsync() => new()
        {
            TotalProperties = await _db.Properties.CountAsync(),
            PendingProperties = await _db.Properties.CountAsync(p => p.StatusId == 1),
            ApprovedProperties = await _db.Properties.CountAsync(p => p.StatusId == 2),
            RejectedProperties = await _db.Properties.CountAsync(p => p.StatusId == 3),
            TotalBookings = await _db.Bookings.CountAsync(),
            TotalUsers = await _db.Users.CountAsync()
        };

        private IQueryable<AdminPropertyProjection> BaseProjection() =>
            _db.Properties.AsNoTracking().Select(p => new AdminPropertyProjection
            {
                PropertyId = p.PropertyId,
                Title = p.Title,
                OwnerName = p.Owner != null ? p.Owner.FirstName + " " + p.Owner.LastName : null,
                Description = p.Description,
                Price = p.Price,
                Rooms = p.Rooms,
                PropertyType = p.PropertyType,
                StatusId = p.StatusId,
                RejectReason = p.RejectReason,
                CreatedAt = p.CreatedAt,
                LocationId = p.LocationId,
                City = p.Location.City,
                Area = p.Location.Area,
                Thumbnail = p.Images.OrderBy(i => i.CreatedAt).Select(i => i.ImagePath).FirstOrDefault()
            });

        public async Task<(List<AdminPropertyProjection> Items, int TotalCount)> GetPendingPropertiesAsync(int page, int pageSize)
        {
            var query = BaseProjection().Where(p => p.StatusId == 1);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public Task<List<AdminPropertyProjection>> GetRejectedPropertiesAsync() =>
            BaseProjection().Where(p => p.StatusId == 3).OrderByDescending(p => p.CreatedAt).ToListAsync();

        public Task<List<AdminPropertyProjection>> GetRecentActionsAsync(int pageSize) =>
            BaseProjection().Where(p => p.StatusId == 2 || p.StatusId == 3)
                .OrderByDescending(p => p.CreatedAt).Take(pageSize).ToListAsync();

        public async Task<AdminPropertyDetail?> GetPropertyDetailAsync(int propertyId)
        {
            var p = await _db.Properties.AsNoTracking()
                .Include(x => x.Location).Include(x => x.Images).Include(x => x.Videos)
                .FirstOrDefaultAsync(x => x.PropertyId == propertyId);

            if (p == null) return null;

            var latestVideo = p.Videos?.OrderByDescending(v => v.CreatedAt).FirstOrDefault();

            return new AdminPropertyDetail
            {
                PropertyId = p.PropertyId,
                OwnerId = p.OwnerId,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Rooms = p.Rooms,
                PropertyType = p.PropertyType,
                StatusId = p.StatusId,
                RejectReason = p.RejectReason,
                CreatedAt = p.CreatedAt,
                LocationId = p.LocationId,
                City = p.Location?.City ?? "",
                Area = p.Location?.Area ?? "",
                Images = p.Images?.Select(i => (i.ImageId, i.ImagePath)).ToList() ?? new(),
                Video = latestVideo != null ? (latestVideo.VideoId, latestVideo.VideoPath) : null
            };
        }

        public Task<bool> IsAdminValidAsync(int adminId) =>
            _db.Users.AsNoTracking().AnyAsync(u => u.UserId == adminId && u.Role.Name == "Admin");

        public async Task<bool> UpdatePropertyStatusAsync(int propertyId, int statusId)
        {
            var property = await _db.Properties.FirstOrDefaultAsync(p => p.PropertyId == propertyId);
            if (property == null) return false;
            property.StatusId = statusId;
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveRejectReasonAsync(int propertyId, string reason)
        {
            var property = await _db.Properties.FirstOrDefaultAsync(p => p.PropertyId == propertyId);
            if (property == null) return false;
            property.RejectReason = reason;
            return await _db.SaveChangesAsync() > 0;
        }
    }

}
