using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Homunity_Data_Access
{
    public class clsLocationData
    {
        // ================= IS LOCATION EXISTS =================
        public static bool IsLocationExists(int locationId)
        {
            try
            {
                using SqlConnection conn =
                    new SqlConnection(clsDataAccessSettings.ConnectionString);
                string query = "SELECT 1 FROM Location WHERE LocationId = @ID";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", locationId);
                conn.Open();
                return cmd.ExecuteScalar() != null;
            }
            catch (Exception ex)
            {
                throw new Exception("Location check failed", ex);
            }
        }

        // ================= GET ALL CITIES =================
        public static DataTable GetAllCities()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = "SELECT DISTINCT City FROM Location ORDER BY City";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            return dt;
        }

        // ================= GET AREAS BY CITY =================
        public static DataTable GetAreasByCity(string city)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT LocationId, Area, Street, Latitude, Longitude
                                     FROM Location
                                     WHERE City = @City
                                     ORDER BY Area";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@City", city);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting areas: {ex.Message}");
            }
            return dt;
        }

        // ================= ADD LOCATION =================
        public static int AddLocation(string city, string area, string street,
            double? latitude, double? longitude)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"INSERT INTO Location (City, Area, Street, Latitude, Longitude)
                                     OUTPUT INSERTED.LocationId
                                     VALUES (@City, @Area, @Street, @Latitude, @Longitude)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@City", city);
                        cmd.Parameters.AddWithValue("@Area", area);
                        cmd.Parameters.AddWithValue("@Street", (object)street ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Latitude", (object)latitude ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Longitude", (object)longitude ?? DBNull.Value);

                        con.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding location: {ex.Message}");
                return -1;
            }
        }


        // ================= UPDATE LOCATION =================
        public static bool UpdateLocation(int locationId, string address,
            double latitude, double longitude)
        {
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE Location
                             SET Street    = @Address,
                                 Latitude  = @Latitude,
                                 Longitude = @Longitude
                             WHERE LocationId = @LocationId";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@LocationId", locationId);
                        cmd.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Latitude", latitude);
                        cmd.Parameters.AddWithValue("@Longitude", longitude);

                        connection.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating location: {ex.Message}");
                return false;
            }
        }

        // ================= UPDATE PROPERTY UNIVERSITY =================
        public static bool UpdatePropertyUniversity(int propertyId, int universityId)
        {
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE Properties
                             SET UniversityId = @UniversityId
                             WHERE PropertyId = @PropertyId";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        cmd.Parameters.AddWithValue("@UniversityId", universityId);

                        connection.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating property university: {ex.Message}");
                return false;
            }
        }
        // أضف هذه الدالة في نهاية الكلاس
        public static bool UpdatePropertyAddressAndUniversity(int propertyId, string fullAddress, int universityId)
        {
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE Properties SET FullAddress = @FullAddress, UniversityId = @UniversityId WHERE PropertyId = @PropertyId";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        cmd.Parameters.AddWithValue("@FullAddress", fullAddress ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UniversityId", universityId);
                        connection.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }
        // أضف هذه الدالة في نهاية الكلاس (موجودة جزئياً، لكن نضيفها بشكل صريح)

        // ================= UPDATE PROPERTY FULL ADDRESS =================
        public static bool UpdatePropertyFullAddress(int propertyId, string fullAddress)
        {
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE Properties SET FullAddress = @FullAddress WHERE PropertyId = @PropertyId";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        cmd.Parameters.AddWithValue("@FullAddress", fullAddress ?? (object)DBNull.Value);
                        connection.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating property full address: {ex.Message}");
                return false;
            }
        }


    }
}