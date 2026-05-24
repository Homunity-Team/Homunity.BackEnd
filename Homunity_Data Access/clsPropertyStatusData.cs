using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsPropertyStatusData
    {
        public static bool IsStatusExists(int statusId)
        {
            try
            {
                using SqlConnection conn =
                    new SqlConnection(clsDataAccessSettings.ConnectionString);

                string query = "SELECT 1 FROM PropertyStatus WHERE PropertyStatusId = @ID";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", statusId);

                conn.Open();
                return cmd.ExecuteScalar() != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
