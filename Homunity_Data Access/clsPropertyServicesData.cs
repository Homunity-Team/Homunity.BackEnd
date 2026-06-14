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
        // =====================================================
        // GET VALID SERVICE IDs — async batch (ONE query)
        // =====================================================
        public static async Task<HashSet<int>> GetValidServiceIdsAsync(IEnumerable<int> serviceIds)
        {
            var result = new HashSet<int>();
            try
            {
                var ids = serviceIds.Distinct().ToList();
                if (!ids.Any()) return result;

                var paramNames = ids.Select((_, i) => $"@id{i}").ToList();
                var sql = $"SELECT ServiceId FROM Services WHERE ServiceId IN ({string.Join(",", paramNames)})";

                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                using var cmd = new SqlCommand(sql, conn);

                for (int i = 0; i < ids.Count; i++)
                    cmd.Parameters.Add($"@id{i}", SqlDbType.Int).Value = ids[i];

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    result.Add(reader.GetInt32(0));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetValidServiceIdsAsync error: {ex.Message}");
            }
            return result;
        }


        // =====================================================
        // BULK ADD SERVICES — async within transaction (ONE INSERT)
        // =====================================================
        public static async Task BulkAddServicesAsync(int propertyId,
            IEnumerable<int> serviceIds,
            SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var ids = serviceIds.Distinct().ToList();
                if (!ids.Any()) return;

                var values = ids.Select((_, i) => $"(@PropId, @svc{i})");
                var sql = $"INSERT INTO PropertyServices (PropertyId, ServiceId) VALUES {string.Join(",", values)}";

                using var cmd = new SqlCommand(sql, connection, transaction);
                cmd.Parameters.Add("@PropId", SqlDbType.Int).Value = propertyId;

                for (int i = 0; i < ids.Count; i++)
                    cmd.Parameters.Add($"@svc{i}", SqlDbType.Int).Value = ids[i];

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BulkAddServicesAsync error: {ex.Message}");
                throw; // fatal — نرجع الـ exception عشان الـ transaction يعمل Rollback
            }
        }


        // =====================================================
        // DELETE ALL BY PROPERTY ID — async within transaction
        // =====================================================
        public static async Task DeleteAllByPropertyIDAsync(int propertyId,
            SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                const string query = "DELETE FROM PropertyServices WHERE PropertyId = @PropertyId";

                using var cmd = new SqlCommand(query, connection, transaction);
                cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteAllServicesAsync error: {ex.Message}");
                throw;
            }
        }


         
        // =====================================================
        // ADD SERVICE — async within transaction (single)
        // =====================================================
        public static async Task<bool> AddServiceToPropertyAsync(int propertyId, int serviceId,
            SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                const string query = @"
                    INSERT INTO PropertyServices (PropertyId, ServiceId)
                    VALUES (@PropertyId, @ServiceId)";

                using var cmd = new SqlCommand(query, connection, transaction);
                cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;
                cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = serviceId;

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddServiceToPropertyAsync error: {ex.Message}");
                return false;
            }
        }

 

        // =====================================================
        // ADD SERVICE — async standalone (بدون transaction)
        // =====================================================
        public static async Task<bool> AddServiceToPropertyAsync(int propertyId, int serviceId)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    INSERT INTO PropertyServices (PropertyId, ServiceId)
                    VALUES (@PropertyId, @ServiceId)";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;
                cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = serviceId;

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddServiceToProperty error: {ex.Message}");
                return false;
            }
        }

 

        // =====================================================
        // IS SERVICE ADDED — sync (بتتكلم من الـ Validation)
        // =====================================================
        public static bool IsServiceAddedToProperty(int propertyId, int serviceId)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT 1 FROM PropertyServices
                    WHERE PropertyId = @PropertyId AND ServiceId = @ServiceId";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;
                cmd.Parameters.Add("@ServiceId", SqlDbType.Int).Value = serviceId;

                conn.Open();
                return cmd.ExecuteScalar() != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsServiceAdded error: {ex.Message}");
                return false;
            }
        }


        // =====================================================
        // GET SERVICES BY PROPERTY ID — async standalone
        // =====================================================
        public static async Task<DataTable> GetServicesByPropertyIDAsync(int propertyId)
        {
            var dt = new DataTable();
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT S.ServiceId, S.Name, S.Icon
                    FROM PropertyServices PS
                    INNER JOIN Services S ON PS.ServiceId = S.ServiceId
                    WHERE PS.PropertyId = @PropertyId";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.Add("@PropertyId", SqlDbType.Int).Value = propertyId;

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetServicesByPropertyIDAsync error: {ex.Message}");
            }
            return dt;
        }

        
    }
}
