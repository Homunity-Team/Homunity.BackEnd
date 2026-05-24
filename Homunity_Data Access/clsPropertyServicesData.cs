using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsPropertyServicesData
    {

        // ================= GET SERVICES BY PROPERTY ID =================
        public static DataTable GetServicesByPropertyID(int propertyId)
        {
            var dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT S.ServiceId, S.Name, S.Icon
                                     FROM PropertyServices PS
                                     INNER JOIN Services S ON PS.ServiceId = S.ServiceId
                                     WHERE PS.PropertyId = @PropertyId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        connection.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting services by property: {ex.Message}");
            }

            return dt;
        }


        // ================= ADD SERVICE TO PROPERTY (بدون Transaction) =================
        public static bool AddServiceToProperty(int propertyId, int serviceId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"INSERT INTO PropertyServices (PropertyId, ServiceId)
                                     VALUES (@PropertyId, @ServiceId)";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        cmd.Parameters.AddWithValue("@ServiceId", serviceId);

                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding service to property: {ex.Message}");
                return false;
            }
        }


        // ================= ADD SERVICE TO PROPERTY (مع Transaction) =================
        public static bool AddServiceToProperty(int propertyId, int serviceId,
            SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string query = @"INSERT INTO PropertyServices (PropertyId, ServiceId)
                                 VALUES (@PropertyId, @ServiceId)";

                using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    cmd.Parameters.AddWithValue("@ServiceId", serviceId);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding service to property (transaction): {ex.Message}");
                return false;
            }
        }


        // ================= DELETE ALL SERVICES FROM PROPERTY =================
        public static bool DeleteAllServicesByPropertyID(int propertyId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"DELETE FROM PropertyServices WHERE PropertyId = @PropertyId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting services: {ex.Message}");
                return false;
            }
        }


        // ================= IS SERVICE ALREADY ADDED =================
        public static bool IsServiceAddedToProperty(int propertyId, int serviceId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT 1 FROM PropertyServices
                                     WHERE PropertyId = @PropertyId AND ServiceId = @ServiceId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        connection.Open();

                        object result = cmd.ExecuteScalar();
                        return result != null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking service: {ex.Message}");
                return false;
            }
        }
        public static bool DeleteAllServicesByPropertyID(int propertyId, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string query = "DELETE FROM PropertyServices WHERE PropertyId = @PropertyId";

                using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting services: {ex.Message}");
                return false;
            }
        }
    }
}
