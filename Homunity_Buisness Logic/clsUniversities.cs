using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Homunity_Buisness_Logic
{
    public class clsUniversities
    {
        public int UniversityId { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // ================= Haversine Formula =================
        public static double CalculateDistance(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371;

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Math.Round(R * c, 2);
        }

        private static double ToRadians(double degrees)
            => degrees * Math.PI / 180;

        // ================= GET ALL =================
        public static List<clsUniversities> GetAllUniversities()
        {
            var dt = clsUniversitiesData.GetAllUniversities();
            var list = new List<clsUniversities>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new clsUniversities
                {
                    UniversityId = Convert.ToInt32(row["UniversityId"]),
                    Name = row["Name"].ToString(),
                    Latitude = Convert.ToDouble(row["Latitude"]),
                    Longitude = Convert.ToDouble(row["Longitude"])
                });
            }
            return list;
        }

        // ================= SEARCH BY UNIVERSITY =================
        public static List<PropertyWithDistanceDTO> SearchByUniversity(
            int universityId, decimal? maxPrice, double? maxDistanceKm = null)
        {
            // 1. جيب بيانات الجامعة
            bool found = clsUniversitiesData.GetUniversityByID(
                universityId,
                out string uniName,
                out double uniLat,
                out double uniLon);

            if (!found) return new List<PropertyWithDistanceDTO>();

            // 2. جيب العقارات التابعة للجامعة
            var dt = clsUniversitiesData.SearchPropertiesByUniversityId(universityId, maxPrice);
            var list = new List<PropertyWithDistanceDTO>();

            foreach (DataRow row in dt.Rows)
            {
                var property = new clsProperties();
                property.LoadFromDataRow(row);

                // 3. احسب المسافة في الـ Backend
                double? distance = null;
                if (property.Latitude.HasValue && property.Longitude.HasValue)
                {
                    distance = CalculateDistance(
                        property.Latitude.Value, property.Longitude.Value,
                        uniLat, uniLon);
                }

                // 4. فلتر بالمسافة لو موجود
                if (maxDistanceKm.HasValue && distance.HasValue &&
                    distance > maxDistanceKm.Value)
                    continue;

                list.Add(new PropertyWithDistanceDTO
                {
                    Property = property,
                    UniversityId = universityId,
                    UniversityName = uniName,
                    UniversityLat = uniLat,
                    UniversityLon = uniLon,
                    DistanceKm = distance
                });
            }

            // 5. رتب من الأقرب للأبعد
            return list.OrderBy(x => x.DistanceKm ?? double.MaxValue).ToList();
        }

        // ================= DTO =================
        public class PropertyWithDistanceDTO
    {
        public clsProperties Property { get; set; }
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public double UniversityLat { get; set; }
        public double UniversityLon { get; set; }
        public double? DistanceKm { get; set; }
    }

}

}