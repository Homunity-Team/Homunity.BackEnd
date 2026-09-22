using Homunity_Data_Access.Repositories;
using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class BookingService : IBookingService
    {
        private const int STATUS_INPROCESS = 2;
        private const int STATUS_BOOKED = 3;
        private const int STATUS_CANCELLED = 4;
        private const int STATUS_CONFIRMED = 5;
private readonly IBookingRepository _repo;
        public BookingService(IBookingRepository repo) => _repo = repo;

        public async Task<BookingCreateResult> CreateAsync(int propertyId, int studentId)
        {
            if (!await _repo.IsPropertyExistAsync(propertyId)) return BookingCreateResult.Fail();
            if (!await _repo.IsPropertyApprovedAsync(propertyId)) return BookingCreateResult.Fail();
            if (!await _repo.IsUserInRoleAsync(studentId, "Student")) return BookingCreateResult.Fail();
            if (!await _repo.IsBookingStatusValidAsync(STATUS_INPROCESS)) return BookingCreateResult.Fail();
            if (await _repo.IsPropertyAlreadyBookedAsync(propertyId)) return BookingCreateResult.Fail();
            if (await _repo.IsStudentAlreadyRequestedPropertyAsync(studentId, propertyId)) return BookingCreateResult.Fail();

            var bookingId = await _repo.AddAsync(propertyId, studentId, STATUS_INPROCESS);
            if (bookingId <= 0) return BookingCreateResult.Fail();

            return new BookingCreateResult
            {
                Success = true,
                BookingId = bookingId,
                PropertyId = propertyId,
                StudentId = studentId,
                StatusId = STATUS_INPROCESS,
                CreatedAt = DateTime.UtcNow
            };
        }

        public Task<BookingDetails?> GetByIdAsync(int bookingId) => _repo.FindAsync(bookingId);
        public Task<List<BookingListItem>> GetByStudentIdAsync(int studentId) => _repo.GetByStudentIdAsync(studentId);
        public Task<List<BookingListItem>> GetByOwnerIdAsync(int ownerId) => _repo.GetByOwnerIdAsync(ownerId);
        public Task<List<BookingByPropertyItem>> GetByPropertyIdAsync(int propertyId) => _repo.GetByPropertyIdAsync(propertyId);

        public async Task<BookingActionResult> ConfirmAsync(int bookingId, int ownerId)
        {
            var booking = await _repo.FindAsync(bookingId);
            if (booking == null) return BookingActionResult.NotFound();

            if (!await _repo.IsUserInRoleAsync(ownerId, "Owner")) return BookingActionResult.Fail();
            // Ownership: acting owner must own the property on this booking
            if (booking.PropertyOwnerId != ownerId) return BookingActionResult.Fail();
            if (booking.StatusId != STATUS_INPROCESS) return BookingActionResult.Fail();
            if (await _repo.IsPropertyAlreadyBookedAsync(booking.PropertyId)) return BookingActionResult.Fail();

            var confirmedAt = DateTime.UtcNow;
            var ok = await _repo.ConfirmWithTransactionAsync(bookingId, booking.PropertyId, confirmedAt);
            return ok ? BookingActionResult.Ok(bookingId, STATUS_CONFIRMED, confirmedAt) : BookingActionResult.Fail();
        }

        public async Task<BookingActionResult> CancelAsync(int bookingId)
        {
            var booking = await _repo.FindAsync(bookingId);
            if (booking == null) return BookingActionResult.NotFound();

            // Cannot cancel when already Booked (paid) or already Cancelled
            if (booking.StatusId == STATUS_BOOKED || booking.StatusId == STATUS_CANCELLED)
                return BookingActionResult.Fail();

            var ok = await _repo.UpdateStatusAsync(bookingId, STATUS_CANCELLED, null);
            return ok ? BookingActionResult.Ok(bookingId, STATUS_CANCELLED, null) : BookingActionResult.Fail();
        }
    }
}
