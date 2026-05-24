using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Homunity_Data_Access
{
    public class clsUsersData
    {
        // Get User By ID
        public static bool GetUserByID(int UserID,out string FirstName,out string LastName,
                                out string Phone,out string PasswordHash, out int RoleID, out bool IsActive)
        {
            // تعيين قيم افتراضية
            FirstName = "";
            LastName = "";
            Phone = "";
            PasswordHash = "";
            RoleID = -1;
            IsActive = false;

            bool isFound = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = "SELECT * FROM Users WHERE UserId = @UserID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", UserID);

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
                            isFound = true;

                            FirstName = (string)reader["FirstName"];
                            LastName = (string)reader["LastName"];
                            Phone = (string)reader["Phone"];
                            PasswordHash = (string)reader["PasswordHash"];
                            RoleID = (int)reader["RoleId"];
                            IsActive = (bool)reader["IsActive"];
                        }
                        else
                        {
                            isFound = false;
                        }
                    }
                }
            }
            catch
            {
                isFound = false;
            }

            return isFound;
        }



        // Get User By Phone
        public static bool GetUserByPhone(string Phone, out int UserID, out string FirstName,
                                   out string LastName, out string PasswordHash,
                                   out int RoleID, out bool IsActive)
        {
            // قيم افتراضية — لازم تتحط لأنها out
            UserID = -1;
            FirstName = string.Empty;
            LastName = string.Empty;
            PasswordHash = string.Empty;
            RoleID = -1;
            IsActive = false;

            bool isFound = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = "SELECT * FROM Users WHERE Phone = @Phone";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Phone", Phone);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                isFound = true;
                                UserID = (int)reader["UserId"];
                                FirstName = (string)reader["FirstName"];
                                LastName = (string)reader["LastName"];
                                PasswordHash = (string)reader["PasswordHash"];
                                RoleID = (int)reader["RoleId"];
                                IsActive = (bool)reader["IsActive"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by phone: {ex.Message}");
            }

            return isFound;
        }


        // Add New User
        public static int AddNewUser(string FirstName, string LastName, string Phone, string PasswordHash, int RoleID, bool IsActive)
        {
            int UserID = -1;

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"INSERT INTO Users
                                    (FirstName, LastName, Phone, PasswordHash, RoleId, IsActive)
                                    OUTPUT INSERTED.UserID
                                    VALUES
                                    (@FirstName, @LastName, @Phone, @PasswordHash, @RoleID, 1)";


                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", FirstName);
                        command.Parameters.AddWithValue("@LastName", LastName);
                        command.Parameters.AddWithValue("@Phone", Phone);
                        command.Parameters.AddWithValue("@PasswordHash", PasswordHash);
                        command.Parameters.AddWithValue("@RoleID", RoleID);

                        connection.Open();

                        object result = command.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int insertedID))
                        {
                            UserID = insertedID;
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                // استخدم logging مناسب
                System.Diagnostics.Debug.WriteLine($"Error adding user: {ex.Message}");
                Console.WriteLine($"Error adding user: {ex.Message}");
            }

            return UserID;
        }



        public static bool IsPhoneExists(string Phone)
        {
            bool exists = false;

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = "SELECT 1 FROM Users WHERE Phone = @Phone";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Phone", Phone);

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        exists = reader.HasRows;
                    }
                }
            }
            catch
            {
                exists = false;
            }

            return exists;
        }



        // 4️⃣ Update User Status
        public static bool UpdateUserStatus(int UserID, bool IsActive)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"UPDATE Users 
                            SET IsActive = @IsActive
                            WHERE UserId = @UserID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", UserID);
                        command.Parameters.AddWithValue("@IsActive", IsActive);

                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // يجب إضافة logging مناسب
                Console.WriteLine("Error updating user status: " + ex.Message);
                return false;
            }

            return rowsAffected > 0;
        }



        // Delete User Status
        public static bool DeleteUser(int UserId)
        {
            int rowsAffected = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = @"DELETE FROM Users WHERE UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", UserId);
                        connection.Open();
                        rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return (rowsAffected > 0);
        }

    }
}

