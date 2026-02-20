using Homunity_Data_Access;
using System.Data;

namespace Homunity_Business_Logic
{
    public class clsBookingStatus
    {
        public int BookingStatusId { get; set; }
        public string StatusName { get; set; }

        public clsBookingStatus()
        {
            this.BookingStatusId = -1;
            this.StatusName = string.Empty;
        }

        private clsBookingStatus(int BookingStatusId, string StatusName)
        {
            this.BookingStatusId = BookingStatusId;
            this.StatusName = StatusName;
        }

        // =============================================
        // STATIC FIND METHODS
        // =============================================
        public static clsBookingStatus Find(int BookingStatusId)
        {
            string StatusName = string.Empty;

            if (clsBookingStatusData.GetBookingStatusByID(BookingStatusId, ref StatusName))
                return new clsBookingStatus(BookingStatusId, StatusName);

            return null;
        }

        public static clsBookingStatus FindByName(string StatusName)
        {
            int BookingStatusId = -1;

            if (clsBookingStatusData.GetBookingStatusByName(StatusName, ref BookingStatusId))
                return new clsBookingStatus(BookingStatusId, StatusName);

            return null;
        }

        // =============================================
        // GET ALL - للـ Dropdowns والـ Reference
        // =============================================
        public static DataTable GetAllBookingStatuses()
            => clsBookingStatusData.GetAllBookingStatuses();

        // =============================================
        // VALIDATION
        // =============================================
        public static bool IsBookingStatusExist(int BookingStatusId)
            => clsBookingStatusData.IsBookingStatusExist(BookingStatusId);

     }
}