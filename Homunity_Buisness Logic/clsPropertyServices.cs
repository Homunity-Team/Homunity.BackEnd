using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class clsPropertyServices
    {

        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode { get; set; } = enMode.AddNew;

        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int PropertyId { get; set; }

        // Constructor
        public clsPropertyServices()
        {
            ServiceId = -1;
            Name = string.Empty;
            Icon = null;
            PropertyId = -1;
            Mode = enMode.AddNew;
        }

        // ================= VALIDATION =================
        private bool _Validate(int propertyId, int serviceId)
        {
            // Validate IDs
            if (propertyId <= 0 || serviceId <= 0)
                return false;

            // Validate إن الـ Service موجود — clsServices مسؤولة
            if (!clsServices.IsValidService(serviceId))
                return false;

            // Validate إن الـ Service مش مضاف قبل كده
            if (clsPropertyServicesData.IsServiceAddedToProperty(propertyId, serviceId))
                return false;

            return true;
        }

        // ================= ADD NEW SERVICE (مع Transaction) =================
        private bool _AddNewService(int propertyId, int serviceId,
            SqlConnection connection, SqlTransaction transaction)
        {
            if (!_Validate(propertyId, serviceId))
                return false;

            return clsPropertyServicesData.AddServiceToProperty(
                propertyId, serviceId, connection, transaction);
        }

        // ================= SAVE (مع Transaction) =================
        public bool Save(SqlConnection connection, SqlTransaction transaction,
                         int propertyId = 0, int serviceId = 0)
        {
            try
            {
                switch (Mode)
                {
                    case enMode.AddNew:
                        return _AddNewService(propertyId, serviceId, connection, transaction);

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving service: {ex.Message}");
                return false;
            }
        }

        // ================= GET SERVICES BY PROPERTY ID =================
        public static List<clsPropertyServices> GetServicesByPropertyID(int propertyId)
        {
            if (propertyId <= 0)
                return new List<clsPropertyServices>();

            var dt = clsPropertyServicesData.GetServicesByPropertyID(propertyId);
            var list = new List<clsPropertyServices>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new clsPropertyServices
                {
                    ServiceId = Convert.ToInt32(row["ServiceId"]),
                    Name = row["Name"].ToString(),
                    Icon = row["Icon"] == DBNull.Value ? null : row["Icon"].ToString(),
                    PropertyId = propertyId,
                    Mode = enMode.Update
                });
            }

            return list;
        }

        // ================= ADD SERVICE TO PROPERTY (بدون Transaction) =================
        // للاستخدام العادي بره الـ Orchestrator
        public static bool AddServiceToProperty(int propertyId, int serviceId)
        {
            if (propertyId <= 0 || serviceId <= 0)
                return false;

            if (!clsServices.IsValidService(serviceId))
                return false;

            if (clsPropertyServicesData.IsServiceAddedToProperty(propertyId, serviceId))
                return false;

            return clsPropertyServicesData.AddServiceToProperty(propertyId, serviceId);
        }

        public static bool DeleteAllByPropertyID(int propertyId,
    SqlConnection connection, SqlTransaction transaction)
        {
            if (propertyId <= 0)
                return false;

            return clsPropertyServicesData.DeleteAllServicesByPropertyID(
                propertyId, connection, transaction);
        }
    }
}
