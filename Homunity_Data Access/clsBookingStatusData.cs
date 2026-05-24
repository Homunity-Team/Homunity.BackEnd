using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Homunity_Data_Access
{
    public class clsBookingStatusData
    {
        // Get All Booking Statuses
        public static DataTable GetAllBookingStatuses()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT BookingStatusId, StatusName 
                                     FROM BookingStatus
                                     ORDER BY BookingStatusId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving booking statuses: " + ex.Message);
            }

            return dt;
        }

        // Get Booking Status By ID
        public static bool GetBookingStatusByID(int BookingStatusId, ref string StatusName)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT StatusName 
                                     FROM BookingStatus
                                     WHERE BookingStatusId = @BookingStatusId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookingStatusId", BookingStatusId);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                StatusName = (string)reader["StatusName"];
                                isFound = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving booking status by ID: " + ex.Message);
            }

            return isFound;
        }

        // Get Booking Status By Name
        public static bool GetBookingStatusByName(string StatusName, ref int BookingStatusId)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT BookingStatusId 
                                     FROM BookingStatus
                                     WHERE StatusName = @StatusName";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StatusName", StatusName);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                BookingStatusId = (int)reader["BookingStatusId"];
                                isFound = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving booking status by name: " + ex.Message);
            }

            return isFound;
        }

        // Check if Booking Status Exists by ID
        public static bool IsBookingStatusExist(int BookingStatusId)
        {
            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"SELECT 1 FROM BookingStatus 
                                     WHERE BookingStatusId = @BookingStatusId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookingStatusId", BookingStatusId);
                        connection.Open();

                        object result = command.ExecuteScalar();
                        isFound = (result != null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking booking status existence: " + ex.Message);
            }

            return isFound;
        }

         
    }
}