using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsAdminActionsData
    {
        // =============================================
        // GET PENDING PROPERTIES (مع Pagination)
        // =============================================
        public static DataTable GetPendingProperties(int pageNumber, int pageSize)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                                           P.Price, P.Rooms, P.PropertyType, P.LocationID,
                                           P.StatusID, P.RejectReason, P.CreatedAt,
                                           L.City, L.Area,
                                           U.FirstName + ' ' + U.LastName AS OwnerName
                                    FROM Properties P
                                    INNER JOIN Location L ON P.LocationID = L.LocationID
                                    INNER JOIN Users U    ON P.OwnerID    = U.UserId
                                    WHERE P.StatusID = 1
                                    ORDER BY P.CreatedAt DESC
                                    OFFSET @Offset ROWS
                                    FETCH NEXT @PageSize ROWS ONLY";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                        command.Parameters.AddWithValue("@PageSize", pageSize);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending properties: {ex.Message}");
            }

            return dt;
        }

        // =============================================
        // GET PENDING PROPERTIES COUNT
        // =============================================
        public static int GetPendingPropertiesCount()
        {
            int count = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT COUNT(*) FROM Properties WHERE StatusID = 1";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        count = (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending count: {ex.Message}");
            }

            return count;
        }



        // =============================================
        // GET REJECTED PROPERTIES
        // =============================================
        public static DataTable GetRejectedProperties()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT P.PropertyID, P.OwnerID, P.Title, P.Description,
                                            P.Price, P.Rooms, P.PropertyType, P.LocationID,
                                            P.StatusID, P.RejectReason, P.CreatedAt,
                                            L.City, L.Area
                                     FROM Properties P
                                     INNER JOIN Location L ON P.LocationID = L.LocationID
                                     WHERE P.StatusID = 3
                                     ORDER BY P.CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting rejected properties: {ex.Message}");
            }

            return dt;
        }



        // =============================================
        // UPDATE PROPERTY STATUS
        // =============================================
        public static bool UpdatePropertyStatus(int propertyId, int statusId)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE Properties
                                     SET StatusID = @StatusID
                                     WHERE PropertyID = @PropertyID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyID", propertyId);
                        command.Parameters.AddWithValue("@StatusID", statusId);
                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating property status: {ex.Message}");
            }

            return rowsAffected > 0;
        }




        // =============================================
        // SAVE REJECT REASON
        // =============================================
        public static bool SaveRejectReason(int propertyId, string reason)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE Properties
                                     SET RejectReason = @RejectReason
                                     WHERE PropertyID = @PropertyID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyID", propertyId);
                        command.Parameters.AddWithValue("@RejectReason", reason);
                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving reject reason: {ex.Message}");
            }

            return rowsAffected > 0;
        }




        // =============================================
        // GET DASHBOARD STATS — Query واحدة ✅
        // =============================================
        public static bool GetDashboardStats(
            ref int totalProperties,
            ref int pendingProperties,
            ref int approvedProperties,
            ref int rejectedProperties,
            ref int totalBookings,
            ref int totalUsers)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                        SELECT
                            (SELECT COUNT(*) FROM Properties)                      AS TotalProperties,
                            (SELECT COUNT(*) FROM Properties WHERE StatusID = 1)   AS PendingProperties,
                            (SELECT COUNT(*) FROM Properties WHERE StatusID = 2)   AS ApprovedProperties,
                            (SELECT COUNT(*) FROM Properties WHERE StatusID = 3)   AS RejectedProperties,
                            (SELECT COUNT(*) FROM Booking)                         AS TotalBookings,
                            (SELECT COUNT(*) FROM Users)                           AS TotalUsers";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                totalProperties = Convert.ToInt32(reader["TotalProperties"]);
                                pendingProperties = Convert.ToInt32(reader["PendingProperties"]);
                                approvedProperties = Convert.ToInt32(reader["ApprovedProperties"]);
                                rejectedProperties = Convert.ToInt32(reader["RejectedProperties"]);
                                totalBookings = Convert.ToInt32(reader["TotalBookings"]);
                                totalUsers = Convert.ToInt32(reader["TotalUsers"]);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting dashboard stats: {ex.Message}");
            }
            return false;
        }




        // =============================================
        // IS ADMIN VALID
        // =============================================
        public static bool IsAdminValid(int adminId)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    // يفترض إن عندك Users JOIN Roles
                    string query = @"SELECT 1 FROM Users U
                                     INNER JOIN Roles R ON U.RoleId = R.RoleId
                                     WHERE U.UserId = @AdminId
                                       AND R.Name = 'Admin'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AdminId", adminId);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        isFound = (result != null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating admin: {ex.Message}");
            }

            return isFound;
        }

        public static DataTable GetRecentActions(int pageSize)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT TOP (@PageSize)
                                           P.PropertyID,P.OwnerId, P.Title, P.Description, P.Price, P.Rooms,
                                           
                                    P.PropertyType, P.LocationID, P.StatusID, P.RejectReason, P.CreatedAt, L.City, L.Area,
                                           U.FirstName + ' ' + U.LastName AS OwnerName,
                                           PS.StatusName AS ActionType
                                    FROM Properties P
                                    INNER JOIN Location L        ON P.LocationID = L.LocationID
                                    INNER JOIN Users U           ON P.OwnerId    = U.UserId
                                    INNER JOIN PropertyStatus PS ON P.StatusId   = PS.PropertyStatusId
                                    WHERE P.StatusID IN (2, 3)
                                    ORDER BY P.CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PageSize", pageSize);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recent actions: {ex.Message}");
            }

            return dt;
        }

 
    }
}
