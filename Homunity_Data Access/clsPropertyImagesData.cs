using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsPropertyImagesData
    {
        // ================= Get Image ByI D ===============
        public static bool GetImageByID(int ImageId, ref int PropertyId, ref string ImagePath, ref DateTime CreatedAt)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT PropertyId, ImagePath, CreatedAt
                                     FROM PropertyImages
                                     WHERE ImageId = @ImageId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ImageId", ImageId);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                isFound = true;
                                PropertyId = (int)reader["PropertyId"];
                                ImagePath = reader["ImagePath"]?.ToString() ?? string.Empty;
                                CreatedAt = (DateTime)reader["CreatedAt"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting image: {ex.Message}");
            }

            return isFound;
        }


        // ================= GET IMAGES COUNT ==============
        public static int GetImagesCountByPropertyID(int propertyId)
        {
            int count = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT COUNT(*) 
                                     FROM PropertyImages
                                     WHERE PropertyId = @PropertyId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", propertyId);
                        connection.Open();
                        count = (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting images: {ex.Message}");
            }

            return count;
        }

        // ================= ADD NEW IMAGE ===================
        public static int AddNewImage(int PropertyId, string ImagePath)
        {
            int ImageId = -1;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"INSERT INTO PropertyImages
                                     (PropertyId, ImagePath, CreatedAt)
                                     OUTPUT INSERTED.ImageId
                                     VALUES(@PropertyId, @ImagePath, GETDATE())";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        command.Parameters.AddWithValue("@ImagePath", ImagePath);

                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null)
                            ImageId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding image: {ex.Message}");
            }

            return ImageId;
        }

        // ================= UPDATE IMAGE ===================
        public static bool UpdateImage(int ImageId, string ImagePath)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE PropertyImages
                                     SET ImagePath = @ImagePath
                                     WHERE ImageId = @ImageId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ImageId", ImageId);
                        command.Parameters.AddWithValue("@ImagePath", ImagePath);

                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating image: {ex.Message}");
            }

            return rowsAffected > 0;
        }

        // ================= DELETE IMAGE ==================
        public static bool DeleteImage(int ImageId)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"DELETE FROM PropertyImages
                                     WHERE ImageId = @ImageId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ImageId", ImageId);
                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }

            return rowsAffected > 0;
        }
    }
}
