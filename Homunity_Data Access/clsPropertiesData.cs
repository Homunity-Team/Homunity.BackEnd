using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsPropertiesData
    {
        // =====================================================
        // ADD NEW PROPERTY — within transaction
        // typed parameters بدل AddWithValue
        // =====================================================
        public static int AddNewProperty(int OwnerID, string Title, string Description,
            decimal Price, int Rooms, string PropertyType,
            int LocationID, int StatusID, string RejectReason,
            SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                const string query = @"
                    INSERT INTO Properties
                        (OwnerID, Title, Description, Price, Rooms,
                         PropertyType, LocationID, StatusID, RejectReason, CreatedAt)
                    OUTPUT INSERTED.PropertyID
                    VALUES
                        (@OwnerID, @Title, @Description, @Price, @Rooms,
                         @PropertyType, @LocationID, @StatusID, @RejectReason, GETDATE())";

                using var cmd = new SqlCommand(query, connection, transaction);

                cmd.Parameters.Add("@OwnerID", SqlDbType.Int).Value = OwnerID;
                cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 150).Value = Title;
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = Description;
                cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = Price;
                cmd.Parameters["@Price"].Precision = 10;
                cmd.Parameters["@Price"].Scale = 2;
                cmd.Parameters.Add("@Rooms", SqlDbType.Int).Value = Rooms;
                cmd.Parameters.Add("@PropertyType", SqlDbType.VarChar, 20).Value = PropertyType;
                cmd.Parameters.Add("@LocationID", SqlDbType.Int).Value = LocationID;
                cmd.Parameters.Add("@StatusID", SqlDbType.Int).Value = StatusID;
                cmd.Parameters.Add("@RejectReason", SqlDbType.NVarChar, 300).Value =
                    string.IsNullOrEmpty(RejectReason) ? (object)DBNull.Value : RejectReason;

                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddNewProperty error: {ex.Message}");
                return -1;
            }
        }


        // =====================================================
        // UPDATE PROPERTY — within transaction
        // typed parameters بدل AddWithValue
        // =====================================================
        public static bool UpdateProperty(int PropertyID, int OwnerID,
            string Title, string Description, decimal Price, int Rooms,
            string PropertyType, int LocationID, int StatusID, string RejectReason,
            SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                const string query = @"
                    UPDATE Properties
                    SET OwnerID      = @OwnerID,
                        Title        = @Title,
                        Description  = @Description,
                        Price        = @Price,
                        Rooms        = @Rooms,
                        PropertyType = @PropertyType,
                        LocationID   = @LocationID,
                        StatusID     = @StatusID,
                        RejectReason = @RejectReason
                    WHERE PropertyID = @PropertyID";

                using var cmd = new SqlCommand(query, connection, transaction);

                cmd.Parameters.Add("@PropertyID", SqlDbType.Int).Value = PropertyID;
                cmd.Parameters.Add("@OwnerID", SqlDbType.Int).Value = OwnerID;
                cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 150).Value = Title;
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = Description;
                cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = Price;
                cmd.Parameters["@Price"].Precision = 10;
                cmd.Parameters["@Price"].Scale = 2;
                cmd.Parameters.Add("@Rooms", SqlDbType.Int).Value = Rooms;
                cmd.Parameters.Add("@PropertyType", SqlDbType.VarChar, 20).Value = PropertyType;
                cmd.Parameters.Add("@LocationID", SqlDbType.Int).Value = LocationID;
                cmd.Parameters.Add("@StatusID", SqlDbType.Int).Value = StatusID;
                cmd.Parameters.Add("@RejectReason", SqlDbType.NVarChar, 300).Value =
                    string.IsNullOrEmpty(RejectReason) ? (object)DBNull.Value : RejectReason;

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateProperty error: {ex.Message}");
                return false;
            }
        }


        // =====================================================
        // DELETE PROPERTY — مع Transaction كامل
        // =====================================================
        public static bool DeleteProperty(int propertyId)
        {
            int rowsAffected = 0;
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    void Execute(string sql)
                    {
                        using var cmd = new SqlCommand(sql, connection, transaction);
                        cmd.Parameters.Add("@PropertyID", SqlDbType.Int).Value = propertyId;
                        cmd.ExecuteNonQuery();
                    }

                    Execute("DELETE FROM PropertyImages  WHERE PropertyId = @PropertyID");
                    Execute("DELETE FROM PropertyVideo   WHERE PropertyId = @PropertyID");
                    Execute("DELETE FROM PropertyServices WHERE PropertyId = @PropertyID");
                    Execute("DELETE FROM Booking         WHERE PropertyId = @PropertyID");
                    Execute("DELETE FROM AdminActions    WHERE PropertyId = @PropertyID");

                    using var cmdProp = new SqlCommand(
                        "DELETE FROM Properties WHERE PropertyID = @PropertyID",
                        connection, transaction);
                    cmdProp.Parameters.Add("@PropertyID", SqlDbType.Int).Value = propertyId;
                    rowsAffected = cmdProp.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"DeleteProperty error: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                return false;
            }
            return rowsAffected > 0;
        }


        // =====================================================
        // GET ALL PROPERTIES
        // =====================================================
        public static DataTable GetAllProperties()
        {
            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                           P.StatusID, P.RejectReason, P.CreatedAt,
                           L.City, L.Area, L.Street
                    FROM Properties P
                    INNER JOIN Location L ON P.LocationID = L.LocationID
                    WHERE P.StatusID = 2
                    AND NOT EXISTS (
                        SELECT 1 FROM Booking B
                        WHERE B.PropertyId = P.PropertyID AND B.StatusId = 3
                    )";

                using var cmd = new SqlCommand(query, connection);
                connection.Open();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllProperties error: {ex.Message}");
            }
            return dt;
        }


        // =====================================================
        // SEARCH PROPERTIES
        // =====================================================
        public static DataTable SearchProperties(string city, string area,
            decimal? minPrice, decimal? maxPrice)
        {
            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

                var query = new StringBuilder(@"
                    SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                           P.StatusID, P.RejectReason, P.CreatedAt,
                           L.City, L.Area, L.Street
                    FROM Properties P
                    INNER JOIN Location L ON P.LocationID = L.LocationID
                    WHERE P.StatusID = 2
                    AND NOT EXISTS (
                        SELECT 1 FROM Booking B
                        WHERE B.PropertyId = P.PropertyID AND B.StatusId = 3
                    )");

                if (!string.IsNullOrWhiteSpace(city)) query.Append(" AND L.City  = @City");
                if (!string.IsNullOrWhiteSpace(area)) query.Append(" AND L.Area  = @Area");
                if (minPrice.HasValue) query.Append(" AND P.Price >= @MinPrice");
                if (maxPrice.HasValue) query.Append(" AND P.Price <= @MaxPrice");

                using var cmd = new SqlCommand(query.ToString(), connection);

                if (!string.IsNullOrWhiteSpace(city))
                    cmd.Parameters.Add("@City", SqlDbType.NVarChar, 20).Value = city;
                if (!string.IsNullOrWhiteSpace(area))
                    cmd.Parameters.Add("@Area", SqlDbType.NVarChar, 50).Value = area;
                if (minPrice.HasValue)
                    cmd.Parameters.Add("@MinPrice", SqlDbType.Decimal).Value = minPrice.Value;
                if (maxPrice.HasValue)
                    cmd.Parameters.Add("@MaxPrice", SqlDbType.Decimal).Value = maxPrice.Value;

                connection.Open();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SearchProperties error: {ex.Message}");
            }
            return dt;
        }


        // =====================================================
        // GET PROPERTY BY ID
        // =====================================================
        public static bool GetPropertyByID(int PropertyID,
            out int OwnerID, out string Title, out string Description,
            out decimal Price, out int Rooms, out string PropertyType,
            out int LocationID, out int StatusID, out string RejectReason,
            out DateTime CreatedAt, out string City, out string Area,
            out string Street, out double? Latitude, out double? Longitude)
        {
            OwnerID = 0; Title = ""; Description = ""; Price = 0; Rooms = 0;
            PropertyType = ""; LocationID = 0; StatusID = 0; RejectReason = "";
            CreatedAt = DateTime.Now; City = ""; Area = ""; Street = null;
            Latitude = null; Longitude = null;

            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                           P.StatusID, P.RejectReason, P.CreatedAt,
                           L.City, L.Area, L.Street, L.Latitude, L.Longitude
                    FROM Properties P
                    INNER JOIN Location L ON P.LocationID = L.LocationID
                    WHERE P.PropertyID = @PropertyID";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.Add("@PropertyID", SqlDbType.Int).Value = PropertyID;
                connection.Open();

                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return false;

                OwnerID = (int)reader["OwnerID"];
                Title = reader["Title"].ToString();
                Description = reader["Description"].ToString();
                Price = (decimal)reader["Price"];
                Rooms = (int)reader["Rooms"];
                PropertyType = reader["PropertyType"] == DBNull.Value ? "" : reader["PropertyType"].ToString();
                LocationID = (int)reader["LocationID"];
                StatusID = (int)reader["StatusID"];
                RejectReason = reader["RejectReason"] == DBNull.Value ? "" : reader["RejectReason"].ToString();
                CreatedAt = (DateTime)reader["CreatedAt"];
                City = reader["City"].ToString();
                Area = reader["Area"].ToString();
                Street = reader["Street"] == DBNull.Value ? null : reader["Street"].ToString();
                Latitude = reader["Latitude"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Latitude"]);
                Longitude = reader["Longitude"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Longitude"]);
                return true;
            }
            catch { return false; }
        }


        // =====================================================
        // IS PROPERTY EXIST
        // =====================================================
        public static bool IsPropertyExist(int PropertyID)
        {
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = "SELECT 1 FROM Properties WHERE PropertyId = @PropertyID";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.Add("@PropertyID", SqlDbType.Int).Value = PropertyID;
                connection.Open();
                return cmd.ExecuteScalar() != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsPropertyExist error: {ex.Message}");
                return false;
            }
        }


        // =====================================================
        // GET PROPERTIES BY OWNER ID
        // =====================================================
        public static DataTable GetPropertiesByOwnerID(int OwnerID)
        {
            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                           P.StatusID, P.RejectReason, P.CreatedAt,
                           L.City, L.Area, L.Street
                    FROM Properties P
                    INNER JOIN Location L ON P.LocationID = L.LocationID
                    WHERE P.OwnerID = @OwnerID
                    ORDER BY P.CreatedAt DESC";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.Add("@OwnerID", SqlDbType.Int).Value = OwnerID;
                connection.Open();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetPropertiesByOwnerID error: {ex.Message}");
            }
            return dt;
        }


        // =====================================================
        // GET PROPERTY BY ID V2
        // =====================================================
        public static bool GetPropertyByIDV2(int PropertyID,
            out int OwnerID, out string Title, out string Description,
            out decimal Price, out int Rooms, out string PropertyType,
            out int LocationID, out int StatusID, out string RejectReason,
            out DateTime CreatedAt, out string City, out string Area,
            out string Street, out double? Latitude, out double? Longitude,
            out int? UniversityId, out string UniversityName,
            out double? UniLat, out double? UniLon,
            out string Address)
        {
            OwnerID = 0; Title = ""; Description = ""; Price = 0; Rooms = 0;
            PropertyType = ""; LocationID = 0; StatusID = 0; RejectReason = "";
            CreatedAt = DateTime.Now; City = ""; Area = ""; Street = null;
            Latitude = null; Longitude = null;
            UniversityId = null; UniversityName = null;
            UniLat = null; UniLon = null;
            Address = "";

            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                           P.StatusID, P.RejectReason, P.CreatedAt,
                           P.UniversityId,
                           L.City, L.Area, L.Street,
                           L.Latitude, L.Longitude,
                           U.Name       AS UniversityName,
                           U.Latitude   AS UniLat,
                           U.Longitude  AS UniLon
                    FROM Properties P
                    INNER JOIN Location L     ON P.LocationID   = L.LocationID
                    LEFT JOIN  Universities U ON P.UniversityId = U.UniversityId
                    WHERE P.PropertyID = @PropertyID";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.Add("@PropertyID", SqlDbType.Int).Value = PropertyID;
                connection.Open();

                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return false;

                OwnerID = (int)reader["OwnerID"];
                Title = reader["Title"].ToString();
                Description = reader["Description"].ToString();
                Price = (decimal)reader["Price"];
                Rooms = (int)reader["Rooms"];
                PropertyType = reader["PropertyType"] == DBNull.Value ? "" : reader["PropertyType"].ToString();
                LocationID = (int)reader["LocationID"];
                StatusID = (int)reader["StatusID"];
                RejectReason = reader["RejectReason"] == DBNull.Value ? "" : reader["RejectReason"].ToString();
                CreatedAt = (DateTime)reader["CreatedAt"];
                City = reader["City"].ToString();
                Area = reader["Area"].ToString();
                Street = reader["Street"] == DBNull.Value ? null : reader["Street"].ToString();
                Latitude = reader["Latitude"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Latitude"]);
                Longitude = reader["Longitude"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Longitude"]);
                UniversityId = reader["UniversityId"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["UniversityId"]);
                UniversityName = reader["UniversityName"] == DBNull.Value ? null : reader["UniversityName"].ToString();
                UniLat = reader["UniLat"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["UniLat"]);
                UniLon = reader["UniLon"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["UniLon"]);
                Address = Street ?? "";
                return true;
            }
            catch { return false; }
        }


        // =====================================================
        // GET PROPERTIES BY OWNER ID V2
        // =====================================================
        public static DataTable GetPropertiesByOwnerIDV2(int OwnerID)
        {
            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                           P.StatusID, P.RejectReason, P.CreatedAt,
                           P.UniversityId,
                           L.City, L.Area, L.Street AS Address,
                           L.Latitude, L.Longitude,
                           U.Name      AS UniversityName,
                           U.Latitude  AS UniLat,
                           U.Longitude AS UniLon
                    FROM Properties P
                    INNER JOIN Location L     ON P.LocationID   = L.LocationID
                    LEFT JOIN  Universities U ON P.UniversityId = U.UniversityId
                    WHERE P.OwnerID = @OwnerID
                    ORDER BY P.CreatedAt DESC";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.Add("@OwnerID", SqlDbType.Int).Value = OwnerID;
                connection.Open();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetPropertiesByOwnerIDV2 error: {ex.Message}");
            }
            return dt;
        }


        // =====================================================
        // GET ALL PROPERTIES V2
        // =====================================================
        public static DataTable GetAllPropertiesV2()
        {
            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
                const string query = @"
                    SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                           P.StatusID, P.RejectReason, P.CreatedAt,
                           P.UniversityId,
                           L.City, L.Area, L.Street AS Address,
                           L.Latitude, L.Longitude,
                           U.Name      AS UniversityName,
                           U.Latitude  AS UniLat,
                           U.Longitude AS UniLon
                    FROM Properties P
                    INNER JOIN Location L     ON P.LocationID   = L.LocationID
                    LEFT JOIN  Universities U ON P.UniversityId = U.UniversityId
                    WHERE P.StatusID = 2
                    AND NOT EXISTS (
                        SELECT 1 FROM Booking B
                        WHERE B.PropertyId = P.PropertyID AND B.StatusId = 3
                    )";

                using var cmd = new SqlCommand(query, connection);
                connection.Open();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllPropertiesV2 error: {ex.Message}");
            }
            return dt;
        }
    }
}
