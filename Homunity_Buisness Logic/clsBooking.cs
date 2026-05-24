using Homunity_Data_Access;
using System;
using System.Data;

namespace Homunity_Business_Logic
{
    public class clsBooking
    {
        // =============================================
        // PROPERTIES
        // ====================ش=========================
        public int BookingId { get; set; }
        public int PropertyId { get; set; }
        public int StudentId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }


        public string PropertyTitle { get; set; }
        public decimal PropertyPrice { get; set; }
        public string PropertyCity { get; set; }
        public string PropertyArea { get; set; }
        public string PropertyImagePath { get; set; }
        public string PropertyAddress { get; set; }




        // Navigation Property
        public clsBookingStatus BookingStatusInfo { get; set; }

        // Mode
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode = enMode.AddNew;

        // Status Constants - بدل ما نكتب أرقام في الكود
        private const int STATUS_INPROCESS = 2;
        private const int STATUS_BOOKED = 3;
        private const int STATUS_CANCELLED = 4;



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
           int StatusId, string StatusName,
           DateTime CreatedAt, DateTime? ConfirmedAt,
           string PropertyTitle, decimal PropertyPrice,
           string PropertyCity, string PropertyArea,
           string PropertyImagePath,
           string PropertyAddress)  // ✅ NEW PARAM
        {
            this.BookingId = BookingId;
            this.PropertyId = PropertyId;
            this.StudentId = StudentId;
            this.StatusId = StatusId;
            this.CreatedAt = CreatedAt;
            this.ConfirmedAt = ConfirmedAt;
            this.PropertyTitle = PropertyTitle;
            this.PropertyPrice = PropertyPrice;
            this.PropertyCity = PropertyCity;
            this.PropertyArea = PropertyArea;
            this.PropertyImagePath = PropertyImagePath;
            this.PropertyAddress = PropertyAddress;  // ✅
            this.BookingStatusInfo = clsBookingStatus.Find(StatusId);
            Mode = enMode.Update;
        }




        public static clsBooking Find(int BookingId)
        {
            int PropertyId = -1;
            int StudentId = -1;
            int StatusId = -1;
            string StatusName = string.Empty;
            DateTime CreatedAt = DateTime.Now;
            DateTime? ConfirmedAt = null;
            string PropertyTitle = string.Empty;
            string PropertyCity = string.Empty;
            string PropertyArea = string.Empty;
            decimal PropertyPrice = 0;
            string PropertyImagePath = null;
            string PropertyAddress = string.Empty;  // ✅ NEW

            if (clsBookingData.GetBookingByID(
                    BookingId,
                    ref PropertyId, ref StudentId,
                    ref StatusId, ref StatusName,
                    ref CreatedAt, ref ConfirmedAt,
                    ref PropertyTitle, ref PropertyCity,
                    ref PropertyArea, ref PropertyPrice,
                    ref PropertyImagePath,
                    ref PropertyAddress))  // ✅ NEW
            {
                return new clsBooking(
                    BookingId, PropertyId, StudentId,
                    StatusId, StatusName,
                    CreatedAt, ConfirmedAt,
                    PropertyTitle, PropertyPrice,
                    PropertyCity, PropertyArea,
                    PropertyImagePath,
                    PropertyAddress  // ✅ NEW
                );
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
        // _Add New Status
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


        // =============================================
        // _Update Status 
        // =============================================
        private bool _UpdateStatus()
        {
            return clsBookingData.UpdateBookingStatus(
                this.BookingId,
                this.StatusId,
                this.ConfirmedAt
            );
        }



        // =============================================
        // SAVE - Entry Point 
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
        public bool Confirm(int ownerId)
        {
            if (!clsBookingData.IsUserHasRole(ownerId, "Owner"))
                return false;

            if (this.StatusId != 2) // InProcess
                return false;

            if (clsBookingData.IsPropertyAlreadyBooked(this.PropertyId))
                return false;

            DateTime now = DateTime.Now;

            bool result = clsBookingData.ConfirmBookingWithTransaction(
                this.BookingId,
                this.PropertyId,
                now
            );

            if (result)
            {
                this.StatusId = 5; // Confirmed
                this.ConfirmedAt = now;
            }

            return result;
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


        public static DataTable GetBookingsByPropertyID(int propertyId)
        {
            return clsBookingData.GetBookingsByPropertyID(propertyId);
        }

    }
}