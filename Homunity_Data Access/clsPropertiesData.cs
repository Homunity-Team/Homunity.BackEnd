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
        // ================= Get Property By ID =================
        public static bool GetPropertyByID(int PropertyID, out int OwnerID, out string Title,
            out string Description, out decimal Price, out int Rooms, out string PropertyType,
            out int LocationID, out int StatusID, out string RejectReason, out DateTime CreatedAt)
        {
            OwnerID = 0;
            Title = "";
            Description = "";
            Price = 0;
            Rooms = 0;
            PropertyType = "";
            LocationID = 0;
            StatusID = 0;
            RejectReason = "";
            CreatedAt = DateTime.Now;

            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = "SELECT * FROM Properties WHERE PropertyID = @PropertyID";

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

                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        // ================= Add New Property =================
        public static int AddNewProperty(int OwnerID, string Title, string Description,
            decimal Price, int Rooms, string PropertyType, int LocationID, int StatusID, string RejectReason)
        {
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"INSERT INTO Properties (OwnerID, Title, Description, Price, Rooms,
                                     PropertyType, LocationID, StatusID, RejectReason, CreatedAt)
                                    OUTPUT INSERTED.PropertyID
                                    VALUES (@OwnerID, @Title, @Description, @Price, @Rooms,
                                     @PropertyType, @LocationID, @StatusID, @RejectReason, GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OwnerID", OwnerID);
                        command.Parameters.AddWithValue("@Title", Title);
                        command.Parameters.AddWithValue("@Description", Description);
                        command.Parameters.AddWithValue("@Price", Price);
                        command.Parameters.AddWithValue("@Rooms", Rooms);
                        command.Parameters.AddWithValue("@PropertyType", PropertyType);
                        command.Parameters.AddWithValue("@LocationID", LocationID);
                        command.Parameters.AddWithValue("@StatusID", StatusID);
                        command.Parameters.AddWithValue("@RejectReason",
                            string.IsNullOrEmpty(RejectReason) ? DBNull.Value : RejectReason);

                        connection.Open();
                        var result = command.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int insertedID))
                        {
                            return insertedID;
                        }
                    }
                }
            }
            catch
            {
                return -1;
            }

            return -1;
        }

        // ================= Update Property =================
        public static bool UpdateProperty(int PropertyID, int OwnerID,
            string Title, string Description, decimal Price, int Rooms, string PropertyType,
            int LocationID, int StatusID, string RejectReason)
        {
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
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

                    using (var command = new SqlCommand(query, connection))
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
                            string.IsNullOrEmpty(RejectReason) ? DBNull.Value : RejectReason);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // ================= Delete Property =================
        public static bool DeleteProperty(int PropertyID)
        {
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                using (var command = new SqlCommand("DELETE FROM Properties WHERE PropertyID = @PropertyID", connection))
                {
                    command.Parameters.AddWithValue("@PropertyID", PropertyID);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        // ================= Get All Properties =================
        public static DataTable GetAllProperties()
        {
            var dt = new DataTable();

            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                using (var cmd = new SqlCommand("SELECT * FROM Properties", connection))
                {
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch
            {
                // Log error if needed
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
                    string query = @"SELECT P.PropertyId,P.Title,P.Description,P.Price,P.Rooms,P.PropertyType,P.CreatedAt,
                                   P.RejectReason,P.LocationId,P.StatusId,P.OwnerId
                                   FROM Properties P
                                   INNER JOIN Location L ON P.LocationId = L.LocationId
                                   WHERE P.StatusId = 1";

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
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchProperties: {ex.Message}");
            }

            return dt;
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

    }
}




