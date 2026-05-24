using Homunity_Data_Access;
using Microsoft.Data.SqlClient;
using Homunity_Shared_DTOs;
using System;
using System.Collections.Generic;

public class clsChatData
{
    public static List<ChatMessageDTO> GetHistory(int studentId, int limit = 20)
    {
        var messages = new List<ChatMessageDTO>();
        try
        {
            using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
            string query = @"SELECT TOP (@Limit) MessageId, Role, Content, CreatedAt
                             FROM ChatMessages WHERE StudentId = @StudentId
                             ORDER BY CreatedAt DESC";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@Limit", limit);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                messages.Add(new ChatMessageDTO
                {
                    MessageId = Convert.ToInt32(reader["MessageId"]),
                    Role = reader["Role"].ToString(),
                    Content = reader["Content"].ToString(),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }
            messages.Reverse();
        }
        catch (Exception ex) { Console.WriteLine($"GetHistory error: {ex.Message}"); }
        return messages;
    }

    public static bool SaveMessage(int studentId, string role, string content)
    {
        try
        {
            using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
            string query = "INSERT INTO ChatMessages (StudentId, Role, Content) VALUES (@StudentId, @Role, @Content)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@Content", content);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
        catch (Exception ex) { Console.WriteLine($"SaveMessage error: {ex.Message}"); return false; }
    }

    public static bool ClearHistory(int studentId)
    {
        try
        {
            using var conn = new SqlConnection(clsDataAccessSettings.ConnectionString);
            string query = "DELETE FROM ChatMessages WHERE StudentId = @StudentId";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            conn.Open();
            return cmd.ExecuteNonQuery() >= 0;
        }
        catch (Exception ex) { Console.WriteLine($"ClearHistory error: {ex.Message}"); return false; }
    }
}