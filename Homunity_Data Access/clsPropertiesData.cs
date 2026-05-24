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

        // ================= Add New Property =================
        public static int AddNewProperty(int OwnerID, string Title, string Description,decimal Price, int Rooms, string PropertyType,
                      int LocationID, int StatusID, string RejectReason,SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string query = @"INSERT INTO Properties
                         (OwnerID, Title, Description, Price, Rooms,
                          PropertyType, LocationID, StatusID, RejectReason, CreatedAt)
                         OUTPUT INSERTED.PropertyID
                         VALUES
                         (@OwnerID, @Title, @Description, @Price, @Rooms,
                          @PropertyType, @LocationID, @StatusID, @RejectReason, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@OwnerID", OwnerID);
                    cmd.Parameters.AddWithValue("@Title", Title);
                    cmd.Parameters.AddWithValue("@Description", Description);
                    cmd.Parameters.AddWithValue("@Price", Price);
                    cmd.Parameters.AddWithValue("@Rooms", Rooms);
                    cmd.Parameters.AddWithValue("@PropertyType", PropertyType);
                    cmd.Parameters.AddWithValue("@LocationID", LocationID);
                    cmd.Parameters.AddWithValue("@StatusID", StatusID);
                    cmd.Parameters.AddWithValue("@RejectReason", (object)RejectReason ?? DBNull.Value);

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding property: {ex.Message}");
                return -1;
            }
        }



        // ================= Update Property =================
         public static bool UpdateProperty(int PropertyID, int OwnerID,
            string Title, string Description, decimal Price, int Rooms, string PropertyType,
            int LocationID, int StatusID, string RejectReason,
            SqlConnection connection, SqlTransaction transaction)
         {
            try
            {
                string query = @"UPDATE Properties
                         SET OwnerID = @OwnerID,
                             Title = @Title,
                             Description = @Description,
                             Price = @Price,
                             Rooms = @Rooms,
                             PropertyType = @PropertyType,
                             LocationID = @LocationID,
                             StatusID = @StatusID,
                             RejectReason = @RejectReason
                         WHERE PropertyID = @PropertyID";

                using (var command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@PropertyID", PropertyID);
                    command.Parameters.AddWithValue("@OwnerID", OwnerID);
                    command.Parameters.AddWithValue("@Title", Title);
                    command.Parameters.AddWithValue("@Description", Description);
                    command.Parameters.AddWithValue("@Price", Price);
                    command.Parameters.AddWithValue("@Rooms", Rooms);
                    command.Parameters.AddWithValue("@PropertyType", PropertyType);
                    command.Parameters.AddWithValue("@LocationID", LocationID);
                    command.Parameters.AddWithValue("@StatusID", StatusID);
                    command.Parameters.AddWithValue("@RejectReason",
                        string.IsNullOrEmpty(RejectReason) ? DBNull.Value : (object)RejectReason);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating property: {ex.Message}");
                return false;
            }
        }




        // ================= Delete Property =================
        public static bool DeleteProperty(int propertyId)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // خطوة 1 — امسح الصور
                            string deleteImages = @"DELETE FROM PropertyImages 
                                           WHERE PropertyId = @PropertyID";
                            using (SqlCommand cmd = new SqlCommand(deleteImages, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PropertyID", propertyId);
                                cmd.ExecuteNonQuery();
                            }

                            // خطوة 2 — امسح الفيديو
                            string deleteVideo = @"DELETE FROM PropertyVideo 
                                          WHERE PropertyId = @PropertyID";
                            using (SqlCommand cmd = new SqlCommand(deleteVideo, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PropertyID", propertyId);
                                cmd.ExecuteNonQuery();
                            }

                            // خطوة 3 — امسح الخدمات
                            string deleteServices = @"DELETE FROM PropertyServices 
                                             WHERE PropertyId = @PropertyID";
                            using (SqlCommand cmd = new SqlCommand(deleteServices, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PropertyID", propertyId);
                                cmd.ExecuteNonQuery();
                            }

                            // خطوة 4 — امسح الحجوزات
                            string deleteBookings = @"DELETE FROM Booking 
                                             WHERE PropertyId = @PropertyID";
                            using (SqlCommand cmd = new SqlCommand(deleteBookings, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PropertyID", propertyId);
                                cmd.ExecuteNonQuery();
                            }

                            // خطوة 5 — امسح Admin Actions
                            string deleteAdminActions = @"DELETE FROM AdminActions 
                                                 WHERE PropertyId = @PropertyID";
                            using (SqlCommand cmd = new SqlCommand(deleteAdminActions, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PropertyID", propertyId);
                                cmd.ExecuteNonQuery();
                            }

                            // خطوة 6 — امسح العقار نفسه
                            string deleteProperty = @"DELETE FROM Properties 
                                             WHERE PropertyID = @PropertyID";
                            using (SqlCommand cmd = new SqlCommand(deleteProperty, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PropertyID", propertyId);
                                rowsAffected = cmd.ExecuteNonQuery();
                            }

                            // كل حاجة تمت — Commit
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            // حاجة فشلت — Rollback كل حاجة
                            transaction.Rollback();
                            Console.WriteLine($"Error deleting property: {ex.Message}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting: {ex.Message}");
                return false;
            }

            return rowsAffected > 0;
        }





        // ================= Get All Properties =================
        public static DataTable GetAllProperties()
        {
            var dt = new DataTable();
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                       P.Price, P.Rooms, P.PropertyType, P.LocationID,
                       P.StatusID, P.RejectReason, P.CreatedAt,
                       L.City, L.Area, L.Street
                FROM Properties P
                INNER JOIN Location L ON P.LocationID = L.LocationID
                WHERE P.StatusID = 2
                AND NOT EXISTS (
                    SELECT 1 FROM Booking B
                    WHERE B.PropertyId = P.PropertyID
                    AND B.StatusId = 3
                )";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (var reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return dt;
        } 





        // ================= Search Properties =================
        public static DataTable SearchProperties(string city, string area, decimal? minPrice, decimal? maxPrice)
        {
            var dt = new DataTable();

            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    // ✅ أضفنا City + Area في الـ SELECT
                    string query = @"SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                                           P.StatusID, P.RejectReason, P.CreatedAt,
                                           L.City, L.Area, L.Street
                                    FROM Properties P
                                    INNER JOIN Location L ON P.LocationID = L.LocationID
                                    WHERE P.StatusID = 2
                                    AND NOT EXISTS (
                                        SELECT 1
                                        FROM Booking B
                                        WHERE B.PropertyId = P.PropertyID
                                        AND B.StatusId = 3
                                    )";

                    if (!string.IsNullOrWhiteSpace(city))
                        query += " AND L.City = @City";

                    if (!string.IsNullOrWhiteSpace(area))
                        query += " AND L.Area = @Area";

                    if (minPrice.HasValue)
                        query += " AND P.Price >= @MinPrice";

                    if (maxPrice.HasValue)
                        query += " AND P.Price <= @MaxPrice";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(city))
                            cmd.Parameters.AddWithValue("@City", city);

                        if (!string.IsNullOrWhiteSpace(area))
                            cmd.Parameters.AddWithValue("@Area", area);

                        if (minPrice.HasValue)
                            cmd.Parameters.AddWithValue("@MinPrice", minPrice.Value);

                        if (maxPrice.HasValue)
                            cmd.Parameters.AddWithValue("@MaxPrice", maxPrice.Value);

                        connection.Open();
                        using (var reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchProperties: {ex.Message}");
            }

            return dt;
        }




        // ================= Get Property By ID =================
        public static bool GetPropertyByID(int PropertyID, out int OwnerID, out string Title,
          out string Description, out decimal Price, out int Rooms, out string PropertyType,
          out int LocationID, out int StatusID, out string RejectReason, out DateTime CreatedAt,
          out string City, out string Area, out string Street,
          out double? Latitude, out double? Longitude)  // ✅ أضفنا
        {
            OwnerID = 0; Title = ""; Description = ""; Price = 0; Rooms = 0;
            PropertyType = ""; LocationID = 0; StatusID = 0; RejectReason = "";
            CreatedAt = DateTime.Now; City = ""; Area = ""; Street = null;
            Latitude = null; Longitude = null;  // ✅ أضفنا

            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                                    P.Price, P.Rooms, P.PropertyType, P.LocationID,
                                    P.StatusID, P.RejectReason, P.CreatedAt,
                                    L.City, L.Area, L.Street,
                                    L.Latitude, L.Longitude
                             FROM Properties P
                             INNER JOIN Location L ON P.LocationID = L.LocationID
                             WHERE P.PropertyID = @PropertyID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyID", PropertyID);
                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                OwnerID = (int)reader["OwnerID"];
                                Title = (string)reader["Title"];
                                Description = (string)reader["Description"];
                                Price = (decimal)reader["Price"];
                                Rooms = (int)reader["Rooms"];
                                PropertyType = reader["PropertyType"] == DBNull.Value ? "" : (string)reader["PropertyType"];
                                LocationID = (int)reader["LocationID"];
                                StatusID = (int)reader["StatusID"];
                                RejectReason = reader["RejectReason"] == DBNull.Value ? "" : (string)reader["RejectReason"];
                                CreatedAt = (DateTime)reader["CreatedAt"];
                                City = reader["City"].ToString();
                                Area = reader["Area"].ToString();
                                Street = reader["Street"] == DBNull.Value ? null : reader["Street"].ToString();
                                Latitude = reader["Latitude"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Latitude"]);   // ✅
                                Longitude = reader["Longitude"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["Longitude"]);  // ✅
                                return true;
                            }
                        }
                    }
                }
            }
            catch { return false; }

            return false;
        }





        // ================= Check if Property Exists =================
        public static bool IsPropertyExist(int PropertyID)
        {
            bool isFound = false;

            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = "SELECT 1 FROM Properties WHERE PropertyId = @PropertyID";

                    using (var command = new SqlCommand(query, connection))
                    {
                  command.Parameters.AddWithValue("@PropertyID", PropertyID);
                        connection.Open();

                        object result = command.ExecuteScalar();
                        isFound = (result != null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking property existence: " + ex.Message);
            }

            return isFound;
        }



        public static DataTable GetPropertiesByOwnerID(int OwnerID)
        {
            var dt = new DataTable();

            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    // أضفنا City + Area من Location في نفس الـ Query
                    string query = @"SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                                    P.Price, P.Rooms, P.PropertyType, P.LocationID,
                                    P.StatusID, P.RejectReason, P.CreatedAt,
                                    L.City, L.Area ,L.Street 
                             FROM Properties P
                             INNER JOIN Location L ON P.LocationID = L.LocationID
                             WHERE P.OwnerID = @OwnerID
                             ORDER BY P.CreatedAt DESC";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@OwnerID", OwnerID);
                        connection.Open();

                        using (var reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting properties by owner: {ex.Message}");
            }

            return dt;
        }





        // ================= Get Property By ID V2 (معدل) =================
        public static bool GetPropertyByIDV2(int PropertyID,
            out int OwnerID, out string Title, out string Description,
            out decimal Price, out int Rooms, out string PropertyType,
            out int LocationID, out int StatusID, out string RejectReason,
            out DateTime CreatedAt, out string City, out string Area,
            out string Street, out double? Latitude, out double? Longitude,
            out int? UniversityId, out string UniversityName,
            out double? UniLat, out double? UniLon,
            out string Address)   // ✅ NEW: العنوان الكامل
        {
            OwnerID = 0; Title = ""; Description = ""; Price = 0; Rooms = 0;
            PropertyType = ""; LocationID = 0; StatusID = 0; RejectReason = "";
            CreatedAt = DateTime.Now; City = ""; Area = ""; Street = null;
            Latitude = null; Longitude = null;
            UniversityId = null; UniversityName = null;
            UniLat = null; UniLon = null;
            Address = "";   // ✅ NEW

            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                       P.Price, P.Rooms, P.PropertyType, P.LocationID,
                       P.StatusID, P.RejectReason, P.CreatedAt,
                       P.UniversityId,
                       L.City, L.Area, L.Street,
                       L.Latitude, L.Longitude,
                       U.Name AS UniversityName,
                       U.Latitude AS UniLat,
                       U.Longitude AS UniLon
                FROM Properties P
                INNER JOIN Location L   ON P.LocationID   = L.LocationID
                LEFT JOIN  Universities U ON P.UniversityId = U.UniversityId
                WHERE P.PropertyID = @PropertyID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyID", PropertyID);
                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                OwnerID = (int)reader["OwnerID"];
                                Title = (string)reader["Title"];
                                Description = (string)reader["Description"];
                                Price = (decimal)reader["Price"];
                                Rooms = (int)reader["Rooms"];
                                PropertyType = reader["PropertyType"] == DBNull.Value ? "" : (string)reader["PropertyType"];
                                LocationID = (int)reader["LocationID"];
                                StatusID = (int)reader["StatusID"];
                                RejectReason = reader["RejectReason"] == DBNull.Value ? "" : (string)reader["RejectReason"];
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
                                Address = Street ?? "";   // ✅ استخدمنا Street كعنوان كامل
                                return true;
                            }
                        }
                    }
                }
            }
            catch { return false; }
            return false;
        }

       
        
        
        // ================= Get Properties By Owner ID V2 (معدل) =================
        public static DataTable GetPropertiesByOwnerIDV2(int OwnerID)
        {
            var dt = new DataTable();
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                       P.Price, P.Rooms, P.PropertyType, P.LocationID,
                       P.StatusID, P.RejectReason, P.CreatedAt,
                       P.UniversityId,
                       L.City, L.Area, L.Street AS Address,   -- ✅ نسميه Address
                       L.Latitude, L.Longitude,
                       U.Name AS UniversityName,
                       U.Latitude AS UniLat,
                       U.Longitude AS UniLon
                FROM Properties P
                INNER JOIN Location L   ON P.LocationID   = L.LocationID
                LEFT JOIN  Universities U ON P.UniversityId = U.UniversityId
                WHERE P.OwnerID = @OwnerID
                ORDER BY P.CreatedAt DESC";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@OwnerID", OwnerID);
                        connection.Open();
                        using (var reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return dt;
        }


        

        // ================= Get All Properties V2 (اختياري - معدل) =================
        public static DataTable GetAllPropertiesV2()
        {
            var dt = new DataTable();
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                       P.Price, P.Rooms, P.PropertyType, P.LocationID,
                       P.StatusID, P.RejectReason, P.CreatedAt,
                       P.UniversityId,
                       L.City, L.Area, L.Street AS Address,
                       L.Latitude, L.Longitude,
                       U.Name AS UniversityName,
                       U.Latitude AS UniLat,
                       U.Longitude AS UniLon
                FROM Properties P
                INNER JOIN Location L   ON P.LocationID   = L.LocationID
                LEFT JOIN  Universities U ON P.UniversityId = U.UniversityId
                WHERE P.StatusID = 2
                AND NOT EXISTS (
                    SELECT 1 FROM Booking B
                    WHERE B.PropertyId = P.PropertyID
                    AND B.StatusId = 3
                )";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (var reader = cmd.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return dt;
        }
    }
}




