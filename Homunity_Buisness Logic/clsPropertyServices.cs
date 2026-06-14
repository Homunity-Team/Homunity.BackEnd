using Homunity_Buisness_Logic;
using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Homunity_Business_Logic
{
    public class clsPropertyServices
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode { get; set; } = enMode.AddNew;

        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int PropertyId { get; set; }

        public clsPropertyServices()
        {
            ServiceId = -1;
            Name = string.Empty;
            Icon = null;
            PropertyId = -1;
            Mode = enMode.AddNew;
        }

        // ================= VALIDATION (Sync) =================
        private bool _Validate(int propertyId, int serviceId)
        {
            if (propertyId <= 0 || serviceId <= 0)
                return false;
            if (!clsServices.IsValidService(serviceId))
                return false;
            if (clsPropertyServicesData.IsServiceAddedToProperty(propertyId, serviceId))
                return false;
            return true;
        }

        // ================= ADD NEW SERVICE (Async with transaction) =================
        private async Task<bool> _AddNewServiceAsync(int propertyId, int serviceId,
            SqlConnection connection, SqlTransaction transaction)
        {
            if (!_Validate(propertyId, serviceId))
                return false;

            return await clsPropertyServicesData.AddServiceToPropertyAsync(
                propertyId, serviceId, connection, transaction);
        }

        // ================= SAVE (Async) =================
        public async Task<bool> SaveAsync(SqlConnection connection, SqlTransaction transaction,
            int propertyId = 0, int serviceId = 0)
        {
            try
            {
                switch (Mode)
                {
                    case enMode.AddNew:
                        return await _AddNewServiceAsync(propertyId, serviceId, connection, transaction);
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

        // Sync wrapper for Save (for backward compatibility)
        public bool Save(SqlConnection connection, SqlTransaction transaction,
                         int propertyId = 0, int serviceId = 0)
        {
            return SaveAsync(connection, transaction, propertyId, serviceId).GetAwaiter().GetResult();
        }

        // ================= GET SERVICES BY PROPERTY ID (Async) =================
        public static async Task<List<clsPropertyServices>> GetServicesByPropertyIDAsync(int propertyId)
        {
            if (propertyId <= 0)
                return new List<clsPropertyServices>();

            var dt = await clsPropertyServicesData.GetServicesByPropertyIDAsync(propertyId);
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

        // Sync wrapper for backward compatibility
        public static List<clsPropertyServices> GetServicesByPropertyID(int propertyId)
        {
            return GetServicesByPropertyIDAsync(propertyId).GetAwaiter().GetResult();
        }

        // ================= ADD SERVICE TO PROPERTY (Async standalone, no transaction) =================
        public static async Task<bool> AddServiceToPropertyAsync(int propertyId, int serviceId)
        {
            if (propertyId <= 0 || serviceId <= 0)
                return false;
            if (!clsServices.IsValidService(serviceId))
                return false;
            if (clsPropertyServicesData.IsServiceAddedToProperty(propertyId, serviceId))
                return false;

            return await clsPropertyServicesData.AddServiceToPropertyAsync(propertyId, serviceId);
        }

        // Sync wrapper
        public static bool AddServiceToProperty(int propertyId, int serviceId)
        {
            return AddServiceToPropertyAsync(propertyId, serviceId).GetAwaiter().GetResult();
        }

        // ================= DELETE ALL SERVICES BY PROPERTY ID (Async with transaction) =================
        public static async Task<bool> DeleteAllByPropertyIDAsync(int propertyId,
            SqlConnection connection, SqlTransaction transaction)
        {
            if (propertyId <= 0)
                return false;

            try
            {
                await clsPropertyServicesData.DeleteAllByPropertyIDAsync(propertyId, connection, transaction);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Sync wrapper
        public static bool DeleteAllByPropertyID(int propertyId,
            SqlConnection connection, SqlTransaction transaction)
        {
            return DeleteAllByPropertyIDAsync(propertyId, connection, transaction).GetAwaiter().GetResult();
        }
    }
}