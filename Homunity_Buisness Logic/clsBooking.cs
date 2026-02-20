using Homunity_Data_Access;
using System;
using System.Data;

namespace Homunity_Business_Logic
{
    public class clsBooking
    {
        // =============================================
        // PROPERTIES
        // =============================================
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int StudentId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        // Navigation Property
        public clsBookingStatus BookingStatusInfo { get; set; }

        // Mode
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode = enMode.AddNew;

        // Status Constants - بدل ما نكتب أرقام في الكود
        private const int STATUS_INPROCESS = 2;
        private const int STATUS_BOOKED = 3;
        private const int STATUS_CANCELLED = 6;

        // =============================================
        // CONSTRUCTORS
        // =============================================
        public clsBooking()
        {
            this.BookingId = -1;
            this.PropertyId = -1;
            this.StudentId = -1;
            this.StatusId = -1;
            this.CreatedAt = DateTime.Now;
            this.ConfirmedAt = null;
            Mode = enMode.AddNew;
        }

        private clsBooking(int BookingId, int PropertyId, int StudentId,
                           int StatusId, DateTime CreatedAt, DateTime? ConfirmedAt)
        {
            this.BookingId = BookingId;
            this.PropertyId = PropertyId;
            this.StudentId = StudentId;
            this.StatusId = StatusId;
            this.CreatedAt = CreatedAt;
            this.ConfirmedAt = ConfirmedAt;
            this.BookingStatusInfo = clsBookingStatus.Find(StatusId);
            Mode = enMode.Update;
        }

        // =============================================
        // STATIC FIND
        // =============================================
        public static clsBooking Find(int BookingId)
        {
            int PropertyId = -1, StudentId = -1, StatusId = -1;
            DateTime CreatedAt = DateTime.Now;
            DateTime? ConfirmedAt = null;

            if (clsBookingData.GetBookingByID(BookingId, ref PropertyId, ref StudentId,
                                              ref StatusId, ref CreatedAt, ref ConfirmedAt))
            {
                return new clsBooking(BookingId, PropertyId, StudentId,
                                      StatusId, CreatedAt, ConfirmedAt);
            }

            return null;
        }

        // =============================================
        // GET METHODS
        // =============================================
        public static DataTable GetBookingsByStudentID(int StudentId)
            => clsBookingData.GetBookingsByStudentID(StudentId);

        public static DataTable GetBookingsByOwnerID(int OwnerId)
            => clsBookingData.GetBookingsByOwnerID(OwnerId);

        public static bool IsBookingExist(int BookingId)
            => clsBookingData.IsBookingExist(BookingId);

        // =============================================
        // PRIVATE VALIDATION
        // =============================================
        private bool _ValidateBooking()
        {
            // 1. Property يكون موجود
            if (!clsPropertiesData.IsPropertyExist(this.PropertyId))
                return false;

            // 2. StudentId يكون فعلاً Student (Role Validation)
            if (!clsBookingData.IsUserHasRole(this.StudentId, "Student"))
                return false;

            // 3. Status موجود
            if (!clsBookingStatusData.IsBookingStatusExist(this.StatusId))
                return false;

            // 4. CreatedAt مش في المستقبل
            if (this.CreatedAt > DateTime.Now)
                return false;

            // 5. ConfirmedAt منطقية
            if (this.ConfirmedAt.HasValue)
            {
                if (this.ConfirmedAt.Value < this.CreatedAt)
                    return false;

                if (this.ConfirmedAt.Value > DateTime.Now)
                    return false;
            }

            return true;
        }

        private bool _ValidateNoDoubleBooking()
        {
            // Rule 1: العقار مش Booked أصلاً
            if (clsBookingData.IsPropertyAlreadyBooked(this.PropertyId))
                return false;

            // Rule 2: الطالب ده مش عامل حجز InProcess على نفس العقار
            if (clsBookingData.IsStudentAlreadyRequestedProperty(this.StudentId, this.PropertyId))
                return false;

            return true;
        }

        // =============================================
        // PRIVATE DATA ACCESS WRAPPERS
        // هنا بالظبط الإجابة على سؤالك 
        // Save() بينادي الـ private methods دي
        // وهي اللي بتتكلم مع الداتا اكسس
        // =============================================
        private bool _AddNew()
        {
            this.BookingId = clsBookingData.AddNewBooking(
                this.PropertyId,
                this.StudentId,
                this.StatusId
            );

            if (this.BookingId != -1)
            {
                Mode = enMode.Update;
                return true;
            }

            return false;
        }

        private bool _UpdateStatus()
        {
            return clsBookingData.UpdateBookingStatus(
                this.BookingId,
                this.StatusId,
                this.ConfirmedAt
            );
        }

        // =============================================
        // SAVE - Entry Point (الصح إنه ينادي الـ private methods)
        // =============================================
        public bool Save()
        {
            if (!_ValidateBooking())
                return false;

            switch (Mode)
            {
                case enMode.AddNew:

                    if (!_ValidateNoDoubleBooking())
                        return false;

                    return _AddNew();

                case enMode.Update:
                    return _UpdateStatus();

                default:
                    return false;
            }
        }

        // =============================================
        // CONFIRM BOOKING - Owner Action
        // يشغّل Transaction: Confirm + Cancel Others
        // =============================================
        public bool Confirm(int OwnerId)
        {
            // 1. Role Validation: OwnerId لازم يكون Owner
            if (!clsBookingData.IsUserHasRole(OwnerId, "Owner"))
                return false;

            // 2. الحجز لازم يكون InProcess عشان يتأكد
            if (this.StatusId != STATUS_INPROCESS)
                return false;

            // 3. العقار مش Booked بالفعل
            if (clsBookingData.IsPropertyAlreadyBooked(this.PropertyId))
                return false;

            DateTime confirmedAt = DateTime.Now;

            // 4. نفذ Transaction في الداتا اكسس
            if (clsBookingData.ConfirmBookingWithTransaction(this.BookingId, this.PropertyId, confirmedAt))
            {
                this.StatusId = STATUS_BOOKED;
                this.ConfirmedAt = confirmedAt;
                this.BookingStatusInfo = clsBookingStatus.Find(STATUS_BOOKED);
                return true;
            }

            return false;
        }

        // =============================================
        // CANCEL BOOKING
        // =============================================
        public bool Cancel()
        {
            // ممنوع تلغي Booking Confirmed (Booked)
            if (this.StatusId == STATUS_BOOKED)
                return false;

            // ممنوع تلغي حاجة Cancelled بالفعل
            if (this.StatusId == STATUS_CANCELLED)
                return false;

            this.StatusId = STATUS_CANCELLED;
            this.ConfirmedAt = null;

            return _UpdateStatus();
        }

        // =============================================
        // UPDATE STATUS - General (للـ Owner أو System)
        // =============================================
        public bool UpdateStatus(int NewStatusId, DateTime? ConfirmedAt = null)
        {
            if (!clsBookingStatusData.IsBookingStatusExist(NewStatusId))
                return false;

            if (ConfirmedAt.HasValue)
            {
                if (ConfirmedAt.Value < this.CreatedAt || ConfirmedAt.Value > DateTime.Now)
                    return false;
            }

            this.StatusId = NewStatusId;
            this.ConfirmedAt = ConfirmedAt;

            return _UpdateStatus();
        }
    }
}