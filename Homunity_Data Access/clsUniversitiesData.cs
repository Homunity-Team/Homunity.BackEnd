using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Homunity_Data_Access
{
    public class clsUniversitiesData
    {
        // ================= GET ALL =================
        public static DataTable GetAllUniversities()
        {
            var dt = new DataTable();
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT UniversityId, Name, Latitude, Longitude
                                     FROM Universities
                                     ORDER BY Name";

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
                Console.WriteLine($"Error getting universities: {ex.Message}");
            }
            return dt;
        }

        // ================= GET BY ID =================
        public static bool GetUniversityByID(int universityId,
            out string name, out double latitude, out double longitude)
        {
            name = ""; latitude = 0; longitude = 0;
            try
            {
                using (var connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT Name, Latitude, Longitude
                                     FROM Universities
                                     WHERE UniversityId = @UniversityId";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UniversityId", universityId);
                        connection.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                name = reader["Name"].ToString();
                                latitude = Convert.ToDouble(reader["Latitude"]);
                                longitude = Convert.ToDouble(reader["Longitude"]);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting university: {ex.Message}");
            }
            return false;
        }

        // ================= SEARCH PROPERTIES FOR UNIVERSITY =================
        public static DataTable SearchPropertiesForUniversity(
            decimal? minPrice, decimal? maxPrice)
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
                               L.City, L.Area, L.Street,
                               L.Latitude, L.Longitude
                        FROM Properties P
                        INNER JOIN Location L ON P.LocationID = L.LocationID
                        WHERE P.StatusID = 2
                        AND L.Latitude  IS NOT NULL
                        AND L.Longitude IS NOT NULL
                        AND NOT EXISTS (
                            SELECT 1 FROM Booking B
                            WHERE B.PropertyId = P.PropertyID
                            AND B.StatusId = 3
                        )";

                    if (minPrice.HasValue)
                        query += " AND P.Price >= @MinPrice";

                    if (maxPrice.HasValue)
                        query += " AND P.Price <= @MaxPrice";

                    using (var cmd = new SqlCommand(query, connection))
                    {
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
                Console.WriteLine($"Error searching properties: {ex.Message}");
            }
            return dt;
        }

        // ================= SEARCH BY UNIVERSITY ID =================
        public static DataTable SearchPropertiesByUniversityId(
            int universityId, decimal? maxPrice)
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
                       L.City, L.Area, L.Street,
                       L.Latitude, L.Longitude
                FROM Properties P
                INNER JOIN Location L ON P.LocationID = L.LocationID
                WHERE P.StatusID     = 2
                AND   P.UniversityId = @UniversityId
                AND NOT EXISTS (
                    SELECT 1 FROM Booking B
                    WHERE B.PropertyId = P.PropertyID
                    AND   B.StatusId   = 3
                )";

                    if (maxPrice.HasValue)
                        query += " AND P.Price <= @MaxPrice";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UniversityId", universityId);

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
                Console.WriteLine($"Error searching by university: {ex.Message}");
            }
            return dt;
        }
    }
}