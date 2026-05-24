using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Homunity_Data_Access
{
    public class clsBookingData
    {
        // =============================================
        // ADD NEW BOOKING
        // =============================================
        public static int AddNewBooking(int PropertyId, int StudentId, int StatusId)
        {
            int BookingId = -1;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"INSERT INTO Booking (PropertyId, StudentId, StatusId, CreatedAt)
                                     OUTPUT INSERTED.BookingId
                                     VALUES (@PropertyId, @StudentId, @StatusId, GETDATE())";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        command.Parameters.AddWithValue("@StudentId", StudentId);
                        command.Parameters.AddWithValue("@StatusId", StatusId);

                        connection.Open();
                        object result = command.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int insertedId))
                            BookingId = insertedId;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding new booking: " + ex.Message);
            }

            return BookingId;
        }

      
        
        // =============================================
        // UPDATE BOOKING STATUS
        // =============================================
        public static bool UpdateBookingStatus(int BookingId, int StatusId, DateTime? ConfirmedAt)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE Booking
                                     SET StatusId = @StatusId,
                                         ConfirmedAt = @ConfirmedAt
                                     WHERE BookingId = @BookingId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookingId", BookingId);
                        command.Parameters.AddWithValue("@StatusId", StatusId);
                        command.Parameters.AddWithValue("@ConfirmedAt",
                            ConfirmedAt.HasValue ? (object)ConfirmedAt.Value : DBNull.Value);

                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating booking status: " + ex.Message);
                return false;
            }

            return rowsAffected > 0;
        }



        // =============================================
        // CONFIRM BOOKING - Transaction: Confirm + Cancel Others
        // =============================================
        public static bool ConfirmBookingWithTransaction(int bookingId, int propertyId, DateTime confirmedAt)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Confirm booking
                            string confirmQuery = @"
                        UPDATE Booking
                        SET StatusId = 5,
                            ConfirmedAt = @ConfirmedAt
                        WHERE BookingId = @BookingId";

                            using (SqlCommand cmd = new SqlCommand(confirmQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                                cmd.Parameters.AddWithValue("@ConfirmedAt", confirmedAt);
                                cmd.ExecuteNonQuery();
                            }

                            // Cancel others
                            string cancelQuery = @"
                        UPDATE Booking
                        SET StatusId = 4
                        WHERE PropertyId = @PropertyId
                          AND BookingId <> @BookingId
                          AND StatusId <> 4";

                            using (SqlCommand cmd = new SqlCommand(cancelQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }


        /*
        // =============================================
        // GET BOOKING BY ID — مع Property Details
        // =============================================
        public static bool GetBookingByID(int BookingId, ref int PropertyId, ref int StudentId,
                                          ref int StatusId, ref string StatusName,
                                          ref DateTime CreatedAt, ref DateTime? ConfirmedAt,
                                          ref string PropertyTitle, ref string PropertyCity,
                                          ref string PropertyArea, ref decimal PropertyPrice,
                                          ref string PropertyImagePath)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT B.BookingId, B.PropertyId, B.StudentId,
                                    B.StatusId, BS.StatusName,
                                    B.CreatedAt, B.ConfirmedAt,
                                    P.Title, P.Price,
                                    L.City, L.Area,
                                    (SELECT TOP 1 ImagePath 
                                     FROM PropertyImages 
                                     WHERE PropertyId = P.PropertyId) AS ImagePath
                             FROM Booking B
                             INNER JOIN Properties P  ON B.PropertyId  = P.PropertyId
                             INNER JOIN Location L    ON P.LocationId  = L.LocationId
                             INNER JOIN BookingStatus BS ON B.StatusId = BS.BookingStatusId
                             WHERE B.BookingId = @BookingId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookingId", BookingId);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                PropertyId = (int)reader["PropertyId"];
                                StudentId = (int)reader["StudentId"];
                                StatusId = (int)reader["StatusId"];
                                StatusName = reader["StatusName"].ToString();
                                CreatedAt = (DateTime)reader["CreatedAt"];
                                ConfirmedAt = reader["ConfirmedAt"] == DBNull.Value
                                                    ? null : (DateTime?)reader["ConfirmedAt"];
                                PropertyTitle = reader["Title"].ToString();
                                PropertyPrice = (decimal)reader["Price"];
                                PropertyCity = reader["City"].ToString();
                                PropertyArea = reader["Area"].ToString();
                                PropertyImagePath = reader["ImagePath"] == DBNull.Value
                                                    ? null : reader["ImagePath"].ToString();
                                isFound = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving booking by ID: " + ex.Message);
            }

            return isFound;
        }

        
        
        // =============================================
        // GET BOOKINGS BY STUDENT ID — مع Property Details
        // =============================================
        public static DataTable GetBookingsByStudentID(int StudentId)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT B.BookingId, B.PropertyId, B.StudentId,
                                    B.StatusId, BS.StatusName,
                                    B.CreatedAt, B.ConfirmedAt,
                                    P.Title AS PropertyTitle, P.Price,
                                    L.City, L.Area,
                                    (SELECT TOP 1 ImagePath 
                                     FROM PropertyImages 
                                     WHERE PropertyId = P.PropertyId) AS ImagePath
                             FROM Booking B
                             INNER JOIN Properties P    ON B.PropertyId  = P.PropertyId
                             INNER JOIN Location L      ON P.LocationId  = L.LocationId
                             INNER JOIN BookingStatus BS ON B.StatusId   = BS.BookingStatusId
                             WHERE B.StudentId = @StudentId
                             ORDER BY B.CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", StudentId);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving bookings by student ID: " + ex.Message);
            }

            return dt;
        }



        // =============================================
        // GET BOOKINGS BY OWNER ID — مع Property Details
        // =============================================
        public static DataTable GetBookingsByOwnerID(int OwnerId)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT B.BookingId, B.PropertyId, B.StudentId,
                                    B.StatusId, BS.StatusName,
                                    B.CreatedAt, B.ConfirmedAt,
                                    P.Title AS PropertyTitle, P.Price,
                                    L.City, L.Area,
                                    U.FirstName + ' ' + U.LastName AS StudentName,
                                    (SELECT TOP 1 ImagePath 
                                     FROM PropertyImages 
                                     WHERE PropertyId = P.PropertyId) AS ImagePath
                             FROM Booking B
                             INNER JOIN Properties P     ON B.PropertyId  = P.PropertyId
                             INNER JOIN Location L       ON P.LocationId  = L.LocationId
                             INNER JOIN BookingStatus BS ON B.StatusId    = BS.BookingStatusId
                             INNER JOIN Users U          ON B.StudentId   = U.UserId
                             WHERE P.OwnerId = @OwnerId
                             ORDER BY B.CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OwnerId", OwnerId);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving bookings by owner ID: " + ex.Message);
            }

            return dt;
        }
       
        */



        public static DataTable GetBookingsByStudentID(int StudentId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                SELECT 
                    B.BookingId, B.PropertyId, B.StudentId,
                    B.StatusId, BS.StatusName,
                    B.CreatedAt, B.ConfirmedAt,
                    P.Title AS PropertyTitle, 
                    P.Price,
                    -- ✅ Address: FullAddress لو موجود، لو لأ Street، لو لأ City+Area
                    COALESCE(
                        NULLIF(LTRIM(RTRIM(P.FullAddress)), ''),
                        NULLIF(LTRIM(RTRIM(L.Street)), ''),
                        L.City + ', ' + L.Area
                    ) AS PropertyAddress,
                    L.City, 
                    L.Area,
                    (SELECT TOP 1 ImagePath 
                     FROM PropertyImages 
                     WHERE PropertyId = P.PropertyId) AS ImagePath
                FROM Booking B
                INNER JOIN Properties P     ON B.PropertyId  = P.PropertyId
                INNER JOIN Location L       ON P.LocationID  = L.LocationID
                INNER JOIN BookingStatus BS ON B.StatusId    = BS.BookingStatusId
                WHERE B.StudentId = @StudentId
                ORDER BY B.CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", StudentId);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetBookingsByStudentID: " + ex.Message);
            }
            return dt;
        }


        // ── GetBookingsByOwnerID ──
        public static DataTable GetBookingsByOwnerID(int OwnerId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                SELECT 
                    B.BookingId, B.PropertyId, B.StudentId,
                    B.StatusId, BS.StatusName,
                    B.CreatedAt, B.ConfirmedAt,
                    P.Title AS PropertyTitle, 
                    P.Price,
                    -- ✅ Address: FullAddress لو موجود، لو لأ Street، لو لأ City+Area
                    COALESCE(
                        NULLIF(LTRIM(RTRIM(P.FullAddress)), ''),
                        NULLIF(LTRIM(RTRIM(L.Street)), ''),
                        L.City + ', ' + L.Area
                    ) AS PropertyAddress,
                    L.City, 
                    L.Area,
                    U.FirstName + ' ' + U.LastName AS StudentName,
                    (SELECT TOP 1 ImagePath 
                     FROM PropertyImages 
                     WHERE PropertyId = P.PropertyId) AS ImagePath
                FROM Booking B
                INNER JOIN Properties P     ON B.PropertyId  = P.PropertyId
                INNER JOIN Location L       ON P.LocationID  = L.LocationID
                INNER JOIN BookingStatus BS ON B.StatusId    = BS.BookingStatusId
                INNER JOIN Users U          ON B.StudentId   = U.UserId
                WHERE P.OwnerId = @OwnerId
                ORDER BY B.CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OwnerId", OwnerId);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                            dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error GetBookingsByOwnerID: " + ex.Message);
            }
            return dt;
        }


        // ── GetBookingByID ──  (لو محتاج تعدله برضو)
        public static bool GetBookingByID(int BookingId, ref int PropertyId, ref int StudentId,
                                          ref int StatusId, ref string StatusName,
                                          ref DateTime CreatedAt, ref DateTime? ConfirmedAt,
                                          ref string PropertyTitle, ref string PropertyCity,
                                          ref string PropertyArea, ref decimal PropertyPrice,
                                          ref string PropertyImagePath,
                                          ref string PropertyAddress)  // ✅ NEW PARAM
        {
            bool isFound = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                SELECT 
                    B.BookingId, B.PropertyId, B.StudentId,
                    B.StatusId, BS.StatusName,
                    B.CreatedAt, B.ConfirmedAt,
                    P.Title, P.Price,
                    L.City, L.Area,
                    COALESCE(
                        NULLIF(LTRIM(RTRIM(P.FullAddress)), ''),
                        NULLIF(LTRIM(RTRIM(L.Street)), ''),
                        L.City + ', ' + L.Area
                    ) AS PropertyAddress,
                    (SELECT TOP 1 ImagePath 
                     FROM PropertyImages 
                     WHERE PropertyId = P.PropertyId) AS ImagePath
                FROM Booking B
                INNER JOIN Properties P     ON B.PropertyId  = P.PropertyId
                INNER JOIN Location L       ON P.LocationID  = L.LocationID
                INNER JOIN BookingStatus BS ON B.StatusId    = BS.BookingStatusId
                WHERE B.BookingId = @BookingId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookingId", BookingId);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                PropertyId = (int)reader["PropertyId"];
                                StudentId = (int)reader["StudentId"];
                                StatusId = (int)reader["StatusId"];
                                StatusName = reader["StatusName"].ToString();
                                CreatedAt = (DateTime)reader["CreatedAt"];
                                ConfirmedAt = reader["ConfirmedAt"] == DBNull.Value ? null : (DateTime?)reader["ConfirmedAt"];
                                PropertyTitle = reader["Title"].ToString();
                                PropertyPrice = (decimal)reader["Price"];
                                PropertyCity = reader["City"].ToString();
                                PropertyArea = reader["Area"].ToString();
                                PropertyAddress = reader["PropertyAddress"].ToString();  // ✅
                                PropertyImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString();
                                isFound = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("Error GetBookingByID: " + ex.Message); }
            return isFound;
        }


        // =============================================
        // CHECK: Is Property Already Booked (StatusId = 3)
        // =============================================
        public static bool IsPropertyAlreadyBooked(int propertyId)
        {
            using SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"
        SELECT 1 
        FROM Booking
        WHERE PropertyId = @PropertyId
          AND StatusId = 3"; // Booked only

            using SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@PropertyId", propertyId);

            connection.Open();
            return cmd.ExecuteScalar() != null;
        }



        // =============================================
        // CHECK: Student Already Has InProcess Booking For Same Property
        // =============================================
        public static bool IsStudentAlreadyRequestedProperty(int StudentId, int PropertyId)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    // StatusId = 2 → InProcess
                    string query = @"SELECT 1 FROM Booking 
                                     WHERE StudentId = @StudentId 
                                       AND PropertyId = @PropertyId
                                       AND StatusId = 2";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", StudentId);
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        isFound = (result != null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking student property booking: " + ex.Message);
            }

            return isFound;
        }

     
        
        
        // =============================================
        // CHECK: Is Booking Exist
        // =============================================
        public static bool IsBookingExist(int BookingId)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT 1 FROM Booking WHERE BookingId = @BookingId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookingId", BookingId);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        isFound = (result != null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking booking existence: " + ex.Message);
            }

            return isFound;
        }

     
        
        // =============================================
        // CHECK: User Role
        // =============================================
        public static bool IsUserHasRole(int UserId, string Name)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    // يفترض إن عندك Users JOIN Roles
                    string query = @"SELECT 1 FROM Users U
                                     INNER JOIN Roles R ON U.RoleId = R.RoleId
                                     WHERE U.UserId = @UserId
                                       AND R.Name = @Name";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", UserId);
                        command.Parameters.AddWithValue("@Name", Name);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        isFound = (result != null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking user role: " + ex.Message);
            }

            return isFound;
        }

        public static DataTable GetBookingsByPropertyID(int propertyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"
                SELECT 
                            b.BookingId, 
                            b.CreatedAt,
                            u.FirstName + '' + u.LastName AS StudentName,
                            bs.StatusName
                        FROM Booking b
                        INNER JOIN Users u ON b.StudentId = u.UserId
                        INNER JOIN BookingStatus bs ON b.StatusId = bs.BookingStatusId
                        WHERE b.PropertyId = @PropertyId
                        ORDER BY b.CreatedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // سجل الخطأ إن أردت
                Console.WriteLine($"Error in GetBookingsByPropertyID: {ex.Message}");
            }
            return dt;
        }

        public static bool UpdateBookingStatusByName(int bookingId, string statusName)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                string query = @"UPDATE b SET b.StatusId = bs.BookingStatusId
                FROM Booking b
                INNER JOIN BookingStatus bs ON bs.StatusName = @StatusName
                WHERE b.BookingId = @BookingId";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.Parameters.AddWithValue("@StatusName", statusName);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { Console.WriteLine($"UpdateBookingStatus: {ex.Message}"); return false; }
        }


        public static bool UpdateBookingStatusToBooked(int bookingId)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);

                string query = @"UPDATE Booking
                         SET StatusId = 3,
                             ConfirmedAt = GETDATE()
                         WHERE BookingId = @BookingId 
                         AND StatusId = 5";

                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateBookingStatusToBooked: " + ex.Message);
                return false;
            }
        }
    }
}