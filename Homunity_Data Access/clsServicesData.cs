using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsServicesData
    {
        // ================= GET ALL SERVICES =================
        public static DataTable GetAllServices()
        {
            var dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT ServiceId, Name
                                     FROM Services
                                     ORDER BY Name";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all services: {ex.Message}");
            }

            return dt;
        }

        // ================= GET SERVICE BY ID =================
        public static bool GetServiceByID(int serviceId, out string name, out string icon)
        {
            name = string.Empty;
            icon = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT Name, Icon
                                     FROM Services
                                     WHERE ServiceId = @ServiceId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        connection.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                name = reader["Name"].ToString();
                                icon = reader["Icon"] == DBNull.Value ? null : reader["Icon"].ToString();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting service by ID: {ex.Message}");
            }

            return false;
        }

        // ================= IS SERVICE EXISTS =================
        public static bool IsServiceExists(int serviceId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT 1 FROM Services WHERE ServiceId = @ServiceId";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                        connection.Open();

                        object result = cmd.ExecuteScalar();
                        return result != null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking service existence: {ex.Message}");
                return false;
            }
        }
    }
}
