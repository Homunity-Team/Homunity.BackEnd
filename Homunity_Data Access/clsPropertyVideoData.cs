
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Homunity_Data_Access
{
    public class clsPropertyVideoData
    {
        // ================= ADD NEW VIDEO =================
        public static bool AddVideo(int PropertyID, string VideoPath,SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string query = @"INSERT INTO PropertyVideo(PropertyId, VideoPath, CreatedAt)
                         VALUES(@PropertyID, @VideoPath, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@PropertyID", PropertyID);
                    cmd.Parameters.AddWithValue("@VideoPath", VideoPath);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding video: {ex.Message}");
                return false;
            }
        }
 
        // ================= UPDATE VIDEO =================
        public static bool UpdateVideo(int VideoId, string VideoPath)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE PropertyVideo
                                     SET VideoPath = @VideoPath
                                     WHERE VideoId = @VideoId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@VideoId", VideoId);
                        command.Parameters.AddWithValue("@VideoPath", VideoPath);

                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating video: {ex.Message}");
            }

            return rowsAffected > 0;
        }

        // ================= Delete VIDEO =================

        public static bool DeleteByPropertyID(int propertyId,SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string query = @"DELETE FROM PropertyVideo WHERE PropertyId = @PropertyId";

                using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting video: {ex.Message}");
                return false;
            }
        }
        
        
        // ================= GET VIDEO BY ID =================
        public static bool GetVideoByID(int VideoId, ref int PropertyId, ref string VideoPath, ref DateTime CreatedAt)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT PropertyId, VideoPath, CreatedAt
                                     FROM PropertyVideo 
                                     WHERE VideoId = @VideoId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@VideoId", VideoId);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                isFound = true;
                                PropertyId = (int)reader["PropertyId"];
                                VideoPath = reader["VideoPath"]?.ToString() ?? string.Empty;
                                CreatedAt = (DateTime)reader["CreatedAt"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting video: {ex.Message}");
            }

            return isFound;
        }

        
        // ================= GET VIDEO BY PROPERTY ID =================
        public static bool GetVideoByPropertyID(int PropertyId, ref int VideoId, ref string VideoPath, ref DateTime CreatedAt)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT TOP 1 VideoId, VideoPath, CreatedAt
                                     FROM PropertyVideo
                                     WHERE PropertyId = @PropertyId
                                     ORDER BY CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                isFound = true;
                                VideoId = (int)reader["VideoId"];
                                VideoPath = reader["VideoPath"]?.ToString() ?? string.Empty;
                                CreatedAt = (DateTime)reader["CreatedAt"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting property video: {ex.Message}");
            }

            return isFound;
        }

    }
}