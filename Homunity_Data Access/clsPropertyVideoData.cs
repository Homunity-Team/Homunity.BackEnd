
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

        // ================= ADD NEW VIDEO =================
        public static int AddNewVideo(int PropertyId, string VideoPath)
        {
            int VideoId = -1;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"INSERT INTO PropertyVideo
                                     (PropertyId, VideoPath, CreatedAt)
                                     OUTPUT INSERTED.VideoId
                                     VALUES(@PropertyId, @VideoPath, GETDATE())";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        command.Parameters.AddWithValue("@VideoPath", VideoPath);

                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null)
                            VideoId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding video: {ex.Message}");
            }

            return VideoId;
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

        // ================= DELETE VIDEO =================
        public static bool DeleteVideo(int VideoId)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"DELETE FROM PropertyVideo
                                     WHERE VideoId = @VideoId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@VideoId", VideoId);
                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting video: {ex.Message}");
            }

            return rowsAffected > 0;
        }
    }
}