
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Homunity_Data_Access
{
    public class clsPropertyVideoData
    {
        // =====================================================
        // ADD VIDEO — async within transaction
        // =====================================================
        public static async Task<bool> AddVideoAsync(int propertyID, string videoPath,
            SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                const string query = @"
                    INSERT INTO PropertyVideo (PropertyId, VideoPath, CreatedAt)
                    VALUES (@PropertyID, @VideoPath, GETDATE())";

                using var cmd = new SqlCommand(query, connection, transaction);
                cmd.Parameters.Add("@PropertyID", SqlDbType.Int).Value = propertyID;
                cmd.Parameters.Add("@VideoPath", SqlDbType.NVarChar, 200).Value = videoPath;

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddVideoAsync error: {ex.Message}");
                return false;
            }
        }

 

        // =====================================================
        // UPDATE VIDEO — async standalone
        // =====================================================
        public static async Task<bool> UpdateVideoAsync(int videoId, string videoPath)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    UPDATE PropertyVideo
                    SET VideoPath = @VideoPath
                    WHERE VideoId = @VideoId";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@VideoId", SqlDbType.Int).Value = videoId;
                cmd.Parameters.Add("@VideoPath", SqlDbType.NVarChar, 200).Value = videoPath;

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateVideoAsync error: {ex.Message}");
                return false;
            }
        }
 
        // =====================================================
        // DELETE BY PROPERTY ID — async within transaction
        // =====================================================
        public static async Task DeleteByPropertyIDAsync(int propertyId,
            SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                const string query = "DELETE FROM PropertyVideo WHERE PropertyId = @PropertyId";

                using var cmd = new SqlCommand(query, connection, transaction);
                cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteVideoByPropertyIDAsync error: {ex.Message}");
            }
        }

 

        // =====================================================
        // GET VIDEO BY ID — sync (ref pattern)
        // =====================================================
        public static bool GetVideoByID(int videoId,
            ref int propertyId, ref string videoPath, ref DateTime createdAt)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT PropertyId, VideoPath, CreatedAt
                    FROM PropertyVideo
                    WHERE VideoId = @VideoId";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@VideoId", SqlDbType.Int).Value = videoId;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (!reader.Read()) return false;

                propertyId = (int)reader["PropertyId"];
                videoPath = reader["VideoPath"]?.ToString() ?? string.Empty;
                createdAt = (DateTime)reader["CreatedAt"];
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideoByID error: {ex.Message}");
                return false;
            }
        }


        // =====================================================
        // GET VIDEO BY PROPERTY ID — sync (ref pattern)
        // =====================================================
        public static bool GetVideoByPropertyID(int propertyId,
            ref int videoId, ref string videoPath, ref DateTime createdAt)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT TOP 1 VideoId, VideoPath, CreatedAt
                    FROM PropertyVideo
                    WHERE PropertyId = @PropertyId
                    ORDER BY CreatedAt DESC";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;

                conn.Open();
                using var reader = cmd.ExecuteReader();

                if (!reader.Read()) return false;

                videoId = (int)reader["VideoId"];
                videoPath = reader["VideoPath"]?.ToString() ?? string.Empty;
                createdAt = (DateTime)reader["CreatedAt"];
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetVideoByPropertyID error: {ex.Message}");
                return false;
            }
        }
    }
}