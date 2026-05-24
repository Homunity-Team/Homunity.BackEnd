using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class clsServices
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        //public string Icon { get; set; }

        // Constructor
        public clsServices()
        {
            ServiceId = -1;
            Name = string.Empty;
            //Icon = null;
        }

        // ================= FIND BY ID =================
        public static clsServices FindByID(int serviceId)
        {
            if (serviceId <= 0)
                return null;

            string name, icon;
            bool found = clsServicesData.GetServiceByID(serviceId, out name, out icon);

            if (!found)
                return null;

            return new clsServices
            {
                ServiceId = serviceId,
                Name = name,
               // Icon = icon
            };
        }

        // ================= GET ALL SERVICES =================
        public static List<clsServices> GetAllServices()
        {
            var dt = clsServicesData.GetAllServices();
            var list = new List<clsServices>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new clsServices
                {
                    ServiceId = Convert.ToInt32(row["ServiceId"]),
                    Name = row["Name"].ToString(),
                   // Icon = row["Icon"] == DBNull.Value ? null : row["Icon"].ToString()
                });
            }

            return list;
        }

        // ================= IS VALID SERVICE =================
        public static bool IsValidService(int serviceId)
        {
            if (serviceId <= 0)
                return false;

            return clsServicesData.IsServiceExists(serviceId);
        }
    }
}
