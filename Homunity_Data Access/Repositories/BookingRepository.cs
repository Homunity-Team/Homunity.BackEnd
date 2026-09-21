using Homunity_Data_Access.Data;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories.Models;
using System;
using Microsoft.EntityFrameworkCore;

namespace Homunity_Data_Access.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly HomunityDbContext _db;
        public BookingRepository(HomunityDbContext db) => _db = db;

        private static string ResolveAddress(PropertyEntity? property)
        {
            if (property == null) return string.Empty;
            if (!string.IsNullOrWhiteSpace(property.FullAddress)) return property.FullAddress.Trim();
            if (property.Location != null)
            {
                if (!string.IsNullOrWhiteSpace(property.Location.Street)) return property.Location.Street!.Trim();
                return $"{property.Location.City}, {property.Location.Area}";
            }
            return string.Empty;
        }

        public async Task<int> AddAsync(int propertyId, int studentId, int statusId)
        {
            var booking = new BookingEntity { PropertyId = propertyId, StudentId = studentId, StatusId = statusId, CreatedAt = DateTime.Now };
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            return booking.BookingId;
        }

        public async Task<bool> UpdateStatusAsync(int bookingId, int statusId, DateTime? confirmedAt)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null) return false;
            booking.StatusId = statusId;
            booking.ConfirmedAt = confirmedAt;
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> ConfirmWithTransactionAsync(int bookingId, int propertyId, DateTime confirmedAt)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
                if (booking == null) { await transaction.RollbackAsync(); return false; }

                booking.StatusId = 5;
                booking.ConfirmedAt = confirmedAt;

                var others = await _db.Bookings
                    .Where(b => b.PropertyId == propertyId && b.BookingId != bookingId && b.StatusId != 4)
                    .ToListAsync();
                foreach (var o in others) o.StatusId = 4;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<BookingDetails?> FindAsync(int bookingId)
        {
            var b = await _db.Bookings.AsNoTracking()
                .Include(x => x.Property).ThenInclude(p => p.Location)
                .Include(x => x.Property).ThenInclude(p => p.Images)
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);

            if (b == null) return null;

            return new BookingDetails
            {
                BookingId = b.BookingId,
                PropertyId = b.PropertyId,
                StudentId = b.StudentId,
                PropertyOwnerId = b.Property?.OwnerId ?? 0,
                StatusId = b.StatusId,
                StatusName = b.Status?.StatusName,
                CreatedAt = b.CreatedAt,
                ConfirmedAt = b.ConfirmedAt,
                PropertyTitle = b.Property?.Title,
                PropertyPrice = b.Property?.Price ?? 0,
                PropertyAddress = ResolveAddress(b.Property),
                PropertyImagePath = b.Property?.Images?.OrderBy(i => i.CreatedAt).Select(i => i.ImagePath).FirstOrDefault()
            };
        }

        public async Task<List<BookingListItem>> GetByStudentIdAsync(int studentId)
        {
            var bookings = await _db.Bookings.AsNoTracking()
                .Include(b => b.Property).ThenInclude(p => p.Location)
                .Include(b => b.Property).ThenInclude(p => p.Images)
                .Include(b => b.Status)
                .Where(b => b.StudentId == studentId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(b => new BookingListItem
            {
                BookingId = b.BookingId,
                StatusId = b.StatusId,
                StatusName = b.Status?.StatusName,
                CreatedAt = b.CreatedAt,
                ConfirmedAt = b.ConfirmedAt,
                PropertyId = b.PropertyId,
                PropertyTitle = b.Property?.Title,
                PropertyPrice = b.Property?.Price ?? 0,
                PropertyAddress = ResolveAddress(b.Property),
                ImagePath = b.Property?.Images?.OrderBy(i => i.CreatedAt).Select(i => i.ImagePath).FirstOrDefault()
            }).ToList();
        }

        public async Task<List<BookingListItem>> GetByOwnerIdAsync(int ownerId)
        {
            var bookings = await _db.Bookings.AsNoTracking()
                .Include(b => b.Property).ThenInclude(p => p.Location)
                .Include(b => b.Property).ThenInclude(p => p.Images)
                .Include(b => b.Status)
                .Include(b => b.Student)
                .Where(b => b.Property.OwnerId == ownerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(b => new BookingListItem
            {
                BookingId = b.BookingId,
                StatusId = b.StatusId,
                StatusName = b.Status?.StatusName,
                CreatedAt = b.CreatedAt,
                ConfirmedAt = b.ConfirmedAt,
                PropertyId = b.PropertyId,
                PropertyTitle = b.Property?.Title,
                PropertyPrice = b.Property?.Price ?? 0,
                PropertyAddress = ResolveAddress(b.Property),
                ImagePath = b.Property?.Images?.OrderBy(i => i.CreatedAt).Select(i => i.ImagePath).FirstOrDefault(),
                StudentName = b.Student != null ? $"{b.Student.FirstName} {b.Student.LastName}" : null
            }).ToList();
        }

        public async Task<List<BookingByPropertyItem>> GetByPropertyIdAsync(int propertyId)
        {
            var bookings = await _db.Bookings.AsNoTracking()
                .Include(b => b.Student)
                .Include(b => b.Status)
                .Where(b => b.PropertyId == propertyId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // ملاحظة: الأصل كان بيدمج الاسم بدون مسافة (u.FirstName + '' + u.LastName) — محافظين على نفس السلوك هنا
            return bookings.Select(b => new BookingByPropertyItem
            {
                BookingId = b.BookingId,
                CreatedAt = b.CreatedAt,
                StudentName = b.Student != null ? $"{b.Student.FirstName}{b.Student.LastName}" : null,
                StatusName = b.Status?.StatusName
            }).ToList();
        }

        public Task<bool> IsPropertyAlreadyBookedAsync(int propertyId) =>
            _db.Bookings.AsNoTracking().AnyAsync(b => b.PropertyId == propertyId && b.StatusId == 3);

        public Task<bool> IsStudentAlreadyRequestedPropertyAsync(int studentId, int propertyId) =>
            _db.Bookings.AsNoTracking().AnyAsync(b => b.StudentId == studentId && b.PropertyId == propertyId && b.StatusId == 2);

        public Task<bool> IsPropertyExistAsync(int propertyId) =>
            _db.Properties.AsNoTracking().AnyAsync(p => p.PropertyId == propertyId);

        public Task<bool> IsUserInRoleAsync(int userId, string roleName) =>
            _db.Users.AsNoTracking().AnyAsync(u => u.UserId == userId && u.Role.Name == roleName);

        public Task<bool> IsBookingStatusValidAsync(int statusId) =>
            _db.BookingStatuses.AsNoTracking().AnyAsync(s => s.BookingStatusId == statusId);
    }

}
