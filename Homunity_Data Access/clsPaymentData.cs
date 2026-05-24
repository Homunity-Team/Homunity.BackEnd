using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsPaymentData
    {

        public static int CreatePayment(int bookingId, int studentId, int ownerId,
    int propertyId, decimal amount, string mockOrderId)
        {
            using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"
        INSERT INTO Payments
        (BookingId, StudentId, OwnerId, PropertyId, Amount, MockOrderId, Status)
        VALUES (@BookingId,@StudentId,@OwnerId,@PropertyId,@Amount,@MockOrderId,'Pending');
        SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@BookingId", bookingId);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);
            cmd.Parameters.AddWithValue("@PropertyId", propertyId);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@MockOrderId", mockOrderId);

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static bool UpdatePaymentStatus(string mockOrderId, string status)
        {
            using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"
        UPDATE Payments 
        SET Status = @Status,
            PaidAt = CASE WHEN @Status = 'Success' THEN GETDATE() ELSE NULL END
        WHERE MockOrderId = @MockOrderId";

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@MockOrderId", mockOrderId);
            cmd.Parameters.AddWithValue("@Status", status);

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }


        public static DataRow GetPaymentByBookingId(int bookingId)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                string query = "SELECT TOP 1 * FROM Payments WHERE BookingId=@BookingId ORDER BY CreatedAt DESC";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch (Exception ex) { Console.WriteLine($"GetPayment: {ex.Message}"); return null; }
        }

        // Get booking details needed for payment
        public static DataRow GetBookingForPayment(int bookingId)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                string query = @"
                SELECT b.BookingId, b.StudentId, b.PropertyId,
                       p.Price, p.Title, p.OwnerID,
                       u.FirstName + ' ' + u.LastName AS StudentName,
                       bs.StatusName,
                       img.ImageUrl
                FROM Booking b
                INNER JOIN Properties p ON b.PropertyId = p.PropertyId
                INNER JOIN Users u ON b.StudentId = u.UserId
                INNER JOIN BookingStatus bs ON b.StatusId = bs.BookingStatusId
                LEFT JOIN (
                    SELECT PropertyId, MIN(ImagePath) AS ImageUrl
                    FROM PropertyImages GROUP BY PropertyId
                ) img ON img.PropertyId = p.PropertyId
                WHERE b.BookingId = @BookingId";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                conn.Open();
                da.Fill(dt);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch (Exception ex) { Console.WriteLine($"GetBookingForPayment: {ex.Message}"); return null; }
        }

        // Update booking status by status name
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

        public static DataRow GetBookingForPaymentWithLock(int bookingId)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                conn.Open();

                string query = @"
            SELECT TOP 1 
                b.BookingId,
                b.PropertyId,
                b.StudentId,
                b.StatusId,
                bs.StatusName,
                b.ConfirmedAt
            FROM Booking b WITH (UPDLOCK, ROWLOCK)
            INNER JOIN BookingStatus bs ON b.StatusId = bs.BookingStatusId
            WHERE b.BookingId = @BookingId";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        // Check if booking already has a payment
        public static bool HasActivePayment(int bookingId)
        {
            try
            {
                using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
                string query = @"SELECT COUNT(*) 
                             FROM Payments 
                             WHERE BookingId=@BookingId 
                             AND Status IN ('Pending','Success')";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                conn.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HasActivePayment: {ex.Message}");
                return false;
            }
        }


        public static bool HasPendingPayment(int bookingId)
        {
            using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);

            // ✅ بس لو في Pending من آخر 10 دقايق — مش قديمة
            string query = @"SELECT COUNT(*) 
                              FROM Payments 
                              WHERE BookingId = @BookingId 
                              AND Status = 'Pending'
                              AND CreatedAt >= DATEADD(MINUTE, -10, GETDATE())";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@BookingId", bookingId);
            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }
    }
}



