using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Homunity_Buisness_Logic
{
   
        // مُبقى عليه مؤقتًا كـ utility رياضي بحت (بدون أي Data Access) لأن clsProperties القديمة
        // (FindByIDV2 / GetAllPropertiesV2 / GetPropertiesByOwnerIDV2) لسه بتستخدمه، وهي جزء من
        // موديول Properties المؤجَّل لسبرنت منفصل (خارج نطاق Sprint 1). كل ما يخص GetAllUniversities/SearchByUniversity
        // اتنقل بالكامل لـ IUniversityRepository/IUniversityService.
        public static class clsUniversities
        {
            public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
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

            private static double ToRadians(double degrees) => degrees * Math.PI / 180;
        }
    
}