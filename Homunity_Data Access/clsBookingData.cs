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
        public static bool ConfirmBookingWithTransaction(int BookingId, int PropertyId, DateTime ConfirmedAt)
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
                            // Step 1: Confirm the booking
                            string confirmQuery = @"UPDATE Booking
                                                    SET StatusId = 3,
                                                        ConfirmedAt = @ConfirmedAt
                                                    WHERE BookingId = @BookingId";

                            using (SqlCommand cmd = new SqlCommand(confirmQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BookingId", BookingId);
                                cmd.Parameters.AddWithValue("@ConfirmedAt", ConfirmedAt);
                                cmd.ExecuteNonQuery();
                            }

                            // Step 2: Cancel all other bookings for same property
                            string cancelOthersQuery = @"UPDATE Booking
                                                          SET StatusId = 4
                                                          WHERE PropertyId = @PropertyId
                                                            AND BookingId <> @BookingId
                                                            AND StatusId <> 4";

                            using (SqlCommand cmd = new SqlCommand(cancelOthersQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PropertyId", PropertyId);
                                cmd.Parameters.AddWithValue("@BookingId", BookingId);
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
            catch (Exception ex)
            {
                Console.WriteLine("Error confirming booking with transaction: " + ex.Message);
                return false;
            }
        }

        // =============================================
        // GET BOOKING BY ID
        // =============================================
        public static bool GetBookingByID(int BookingId, ref int PropertyId, ref int StudentId,
                                          ref int StatusId, ref DateTime CreatedAt, ref DateTime? ConfirmedAt)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT PropertyId, StudentId, StatusId, CreatedAt, ConfirmedAt
                                     FROM Booking
                                     WHERE BookingId = @BookingId";

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
                                CreatedAt = (DateTime)reader["CreatedAt"];
                                ConfirmedAt = reader["ConfirmedAt"] == DBNull.Value
                                    ? null
                                    : (DateTime?)reader["ConfirmedAt"];
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
        // GET BOOKINGS BY STUDENT ID
        // =============================================
        public static DataTable GetBookingsByStudentID(int StudentId)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT BookingId, PropertyId, StudentId,
                                            StatusId, CreatedAt, ConfirmedAt
                                     FROM Booking
                                     WHERE StudentId = @StudentId";

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
        // GET BOOKINGS BY OWNER ID
        // =============================================
        public static DataTable GetBookingsByOwnerID(int OwnerId)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT B.BookingId, B.PropertyId, B.StudentId,
                                            B.StatusId, B.CreatedAt, B.ConfirmedAt
                                     FROM Booking B
                                     INNER JOIN Properties P ON B.PropertyId = P.PropertyId
                                     WHERE P.OwnerId = @OwnerId";

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

        // =============================================
        // CHECK: Is Property Already Booked (StatusId = 3)
        // =============================================
        public static bool IsPropertyAlreadyBooked(int PropertyId)
        {
            bool isBooked = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT 1 FROM Booking 
                                     WHERE PropertyId = @PropertyId 
                                       AND StatusId = 3";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", PropertyId);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        isBooked = (result != null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking property booking: " + ex.Message);
            }

            return isBooked;
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
    }
}