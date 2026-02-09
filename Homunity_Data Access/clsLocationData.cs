using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsLocationData
    {
        public static bool IsLocationExists(int locationId)
        {
            try
            {
                using SqlConnection conn =
                    new SqlConnection(clsDataAccessSettings.ConnectionString);

                string query = "SELECT 1 FROM Location WHERE LocationId = @ID";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", locationId);

                conn.Open();
                return cmd.ExecuteScalar() != null;
            }
            catch (Exception ex)
            {
                throw new Exception("Location check failed", ex);
            }

        }

        public static DataTable GetAllCities()
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"SELECT DISTINCT City FROM Location";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
            }

            return dt;
        }


        public static DataTable GetAreasByCity(string city)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                string query = @"SELECT DISTINCT Area 
                         FROM Location 
                         WHERE City = @City";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@City", city);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
            }

            return dt;
        }
    }
}
