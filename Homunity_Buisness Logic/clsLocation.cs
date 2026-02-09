using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class clsLocation
    {
        public static bool IsValidLocation(int locationId)
        {
            if (locationId <= 0)
                return false;

            return clsLocationData.IsLocationExists(locationId);
        }


        public static List<string> GetCities()
        {
            var dt = clsLocationData.GetAllCities();
            var list = new List<string>();

            foreach (DataRow row in dt.Rows)
                list.Add(row["City"].ToString());

            return list;
        }

        public static List<string> GetAreasByCity(string city)
        {
            var dt = clsLocationData.GetAreasByCity(city);
            var list = new List<string>();

            foreach (DataRow row in dt.Rows)
                list.Add(row["Area"].ToString());

            return list;
        }

    }
}
