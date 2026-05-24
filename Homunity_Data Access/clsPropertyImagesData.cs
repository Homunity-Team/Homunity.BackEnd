using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsPropertyImagesData
    {
       
        // ================= ADD NEW IMAGE ===================
        public static int AddImage(int PropertyID, string ImagePath,SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string query = @"INSERT INTO PropertyImages(PropertyId, ImagePath, CreatedAt)
                         OUTPUT INSERTED.ImageId
                         VALUES(@PropertyID, @ImagePath, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@PropertyID", PropertyID);
                    cmd.Parameters.AddWithValue("@ImagePath", ImagePath);

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding image: {ex.Message}");
                return -1;
            }
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


        public static bool Delete(int imageId, int propertyId,SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string query = @"DELETE FROM PropertyImages 
                         WHERE ImageId = @ImageId AND PropertyId = @PropertyId";

                using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@ImageId", imageId);
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
                return false;
            }
        }


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


        // Method جديدة بنفس الـ Pattern الموجود في الكلاس
        public static DataTable GetImagesByPropertyID(int propertyId)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT ImageId, PropertyId, ImagePath, CreatedAt
                             FROM PropertyImages
                             WHERE PropertyId = @PropertyId
                             ORDER BY CreatedAt ASC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", propertyId);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting images by property: {ex.Message}");
            }

            return dt;
        }


    }
}
