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
 
            // =====================================================
            // ADD IMAGE — async within transaction
            // =====================================================
            public static async Task<int> AddImageAsync(int propertyID, string imagePath,
                SqlConnection connection, SqlTransaction transaction)
            {
                try
                {
                    const string query = @"
                    INSERT INTO PropertyImages (PropertyId, ImagePath, CreatedAt)
                    OUTPUT INSERTED.ImageId
                    VALUES (@PropertyID, @ImagePath, GETDATE())";

                    using var cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.Add("@PropertyID", SqlDbType.Int).Value = propertyID;
                    cmd.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 200).Value = imagePath;

                    var result = await cmd.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AddImageAsync error: {ex.Message}");
                    return -1;
                }
            }

            

            // =====================================================
            // UPDATE IMAGE — async standalone
            // =====================================================
            public static async Task<bool> UpdateImageAsync(int imageId, string imagePath)
            {
                try
                {
                    using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                    const string query = @"
                    UPDATE PropertyImages
                    SET ImagePath = @ImagePath
                    WHERE ImageId = @ImageId";

                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.Add("@ImageId", SqlDbType.Int).Value = imageId;
                    cmd.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 200).Value = imagePath;

                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UpdateImageAsync error: {ex.Message}");
                    return false;
                }
            }

 

            // =====================================================
            // DELETE IMAGE — async within transaction
            // =====================================================
            public static async Task<bool> DeleteAsync(int imageId, int propertyId,
                SqlConnection connection, SqlTransaction transaction)
            {
                try
                {
                    const string query = @"
                    DELETE FROM PropertyImages
                    WHERE ImageId = @ImageId AND PropertyId = @PropertyId";

                    using var cmd = new SqlCommand(query, connection, transaction);
                    cmd.Parameters.Add("@ImageId", SqlDbType.Int).Value = imageId;
                    cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DeleteImageAsync error: {ex.Message}");
                    return false;
                }
            }

   
            // =====================================================
            // GET IMAGE BY ID — async standalone
            // =====================================================
            public static async Task<bool> GetImageByIDAsync(int imageId,
                Action<int, string, DateTime> onFound)
            {
                try
                {
                    using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                    const string query = @"
                    SELECT PropertyId, ImagePath, CreatedAt
                    FROM PropertyImages
                    WHERE ImageId = @ImageId";

                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.Add("@ImageId", SqlDbType.Int).Value = imageId;

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    if (!await reader.ReadAsync()) return false;

                    onFound(
                        (int)reader["PropertyId"],
                        reader["ImagePath"]?.ToString() ?? string.Empty,
                        (DateTime)reader["CreatedAt"]
                    );
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetImageByIDAsync error: {ex.Message}");
                    return false;
                }
            }

            
 
            // =====================================================
            // GET IMAGES COUNT — async standalone
            // =====================================================
            public static async Task<int> GetImagesCountByPropertyIDAsync(int propertyId)
            {
                try
                {
                    using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                    const string query = @"
                    SELECT COUNT(*)
                    FROM PropertyImages
                    WHERE PropertyId = @PropertyId";

                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;

                    await conn.OpenAsync();
                    return (int)await cmd.ExecuteScalarAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetImagesCountAsync error: {ex.Message}");
                    return 0;
                }
            }


            // =====================================================
            // GET IMAGES BY PROPERTY ID — async standalone
            // =====================================================
            public static async Task<DataTable> GetImagesByPropertyIDAsync(int propertyId)
            {
                var dt = new DataTable();
                try
                {
                    using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                    const string query = @"
                    SELECT ImageId, PropertyId, ImagePath, CreatedAt
                    FROM PropertyImages
                    WHERE PropertyId = @PropertyId
                    ORDER BY CreatedAt ASC";

                    using var cmd = new SqlCommand(query, conn);
                    cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    dt.Load(reader);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetImagesByPropertyIDAsync error: {ex.Message}");
                }
                return dt;
            }

 

        
    }
   
}


 