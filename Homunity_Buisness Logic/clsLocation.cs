using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Data;

namespace Homunity_Buisness_Logic
{
    public class clsLocation
    {
        // ================= IS VALID =================
        public static bool IsValidLocation(int locationId)
        {
            if (locationId <= 0) return false;
            return clsLocationData.IsLocationExists(locationId);
        }

        // ================= GET CITIES =================
        public static List<string> GetCities()
        {
            var dt = clsLocationData.GetAllCities();
            var list = new List<string>();
            foreach (DataRow row in dt.Rows)
                list.Add(row["City"].ToString());
            return list;
        }

        // ================= GET AREAS BY CITY =================
        public static List<object> GetAreasByCity(string city)
        {
            var dt = clsLocationData.GetAreasByCity(city);
            var list = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    locationId = Convert.ToInt32(row["LocationId"]),
                    area = row["Area"].ToString(),
                    street = row["Street"] == DBNull.Value ? null : row["Street"].ToString(),
                    latitude = row["Latitude"] == DBNull.Value ? (double?)null : Convert.ToDouble(row["Latitude"]),
                    longitude = row["Longitude"] == DBNull.Value ? (double?)null : Convert.ToDouble(row["Longitude"])
                });
            }
            return list;
        }

        // ================= ADD LOCATION =================
        public static int AddLocation(string city, string area, string street,
            double? latitude, double? longitude)
        {
            if (string.IsNullOrWhiteSpace(city)) return -1;
            if (string.IsNullOrWhiteSpace(area)) return -1;

            if (latitude.HasValue && (latitude < -90 || latitude > 90)) return -1;
            if (longitude.HasValue && (longitude < -180 || longitude > 180)) return -1;

            return clsLocationData.AddLocation(city, area, street, latitude, longitude);
        }

        // ✅ NEW: تحديث العنوان الكامل للعقار (في جدول Properties)
        public static bool UpdatePropertyFullAddress(int propertyId, string fullAddress)
        {
            if (propertyId <= 0) return false;
            return clsLocationData.UpdatePropertyFullAddress(propertyId, fullAddress);
        }

        // ✅ NEW: تحديث الجامعة المرتبطة بالعقار
        public static bool UpdatePropertyUniversity(int propertyId, int universityId)
        {
            if (propertyId <= 0 || universityId <= 0) return false;
            return clsLocationData.UpdatePropertyUniversity(propertyId, universityId);
        }

        // ✅ NEW: تحديث الموقع والجامعة معاً
        public static bool UpdatePropertyLocationAndUniversity(int propertyId, string fullAddress, int universityId)
        {
            bool addrUpdated = UpdatePropertyFullAddress(propertyId, fullAddress);
            bool uniUpdated = UpdatePropertyUniversity(propertyId, universityId);
            return addrUpdated && uniUpdated;
        }
    }
}