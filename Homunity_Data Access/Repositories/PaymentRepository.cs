using Homunity_Data_Access.Data;
using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Homunity_Data_Access.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly HomunityDbContext _db;
        public PaymentRepository(HomunityDbContext db) => _db = db;

        public async Task<BookingForPayment?> GetBookingForPaymentAsync(int bookingId)
        {
            var row = await _db.Bookings.AsNoTracking()
                .Where(b => b.BookingId == bookingId)
                .Select(b => new
                {
                    b.BookingId,
                    b.StudentId,
                    b.PropertyId,
                    b.Property.OwnerId,
                    b.Property.Price,
                    b.Property.Title,
                    StudentName = b.Student.FirstName + " " + b.Student.LastName,
                    StatusName = b.Status.StatusName,
                    ImageUrl = b.Property.Images.OrderBy(i => i.CreatedAt).Select(i => i.ImagePath).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (row == null) return null;

            return new BookingForPayment
            {
                BookingId = row.BookingId,
                StudentId = row.StudentId,
                PropertyId = row.PropertyId,
                OwnerId = row.OwnerId,
                Price = row.Price,
                Title = row.Title,
                StudentName = row.StudentName,
                StatusName = row.StatusName,
                ImageUrl = row.ImageUrl
            };
        }

        public async Task<BookingLockInfo?> GetBookingForPaymentWithLockAsync(int bookingId)
        {
            // SQL خام مُبقى عليه عمدًا: EF Core مفيهوش ترجمة LINQ لـ UPDLOCK/ROWLOCK،
            // وده مطلوب فعليًا هنا لمنع Race Condition لو اتنين حاولوا يعملوا Process لنفس الأوردر في نفس اللحظة.
            var rows = await _db.Database
                .SqlQuery<BookingLockRow>($@"
                    SELECT b.BookingId, b.PropertyId, b.StudentId, b.StatusId,
                           bs.StatusName, b.ConfirmedAt
                    FROM Booking b WITH (UPDLOCK, ROWLOCK)
                    INNER JOIN BookingStatus bs ON b.StatusId = bs.BookingStatusId
                    WHERE b.BookingId = {bookingId}")
                .ToListAsync();

            var row = rows.FirstOrDefault();
            if (row == null) return null;

            return new BookingLockInfo
            {
                BookingId = row.BookingId,
                PropertyId = row.PropertyId,
                StudentId = row.StudentId,
                StatusId = row.StatusId,
                StatusName = row.StatusName,
                ConfirmedAt = row.ConfirmedAt
            };
        }

        public Task<bool> HasPendingPaymentAsync(int bookingId)
        {
            var cutoff = DateTime.Now.AddMinutes(-10);
            return _db.Payments.AsNoTracking().AnyAsync(p =>
                p.BookingId == bookingId && p.Status == "Pending" && p.CreatedAt >= cutoff);
        }

        public async Task<int> CreatePaymentAsync(int bookingId, int studentId, int ownerId, int propertyId, decimal amount, string mockOrderId)
        {
            var payment = new PaymentEntity
            {
                BookingId = bookingId,
                StudentId = studentId,
                OwnerId = ownerId,
                PropertyId = propertyId,
                Amount = amount,
                MockOrderId = mockOrderId,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return payment.PaymentId;
        }

        public async Task<bool> UpdatePaymentStatusAsync(string mockOrderId, string status)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.MockOrderId == mockOrderId);
            if (payment == null) return false;

            payment.Status = status;
            payment.PaidAt = status == "Success" ? DateTime.Now : null;

            return await _db.SaveChangesAsync() > 0;
        }

        public Task<PaymentEntity?> GetPaymentByBookingIdAsync(int bookingId) =>
            _db.Payments.AsNoTracking().Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.CreatedAt).FirstOrDefaultAsync();

        public async Task<bool> UpdateBookingStatusToBookedAsync(int bookingId)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId && b.StatusId == 5);
            if (booking == null) return false;

            booking.StatusId = 3;
            booking.ConfirmedAt = DateTime.Now;
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
