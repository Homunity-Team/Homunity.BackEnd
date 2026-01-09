using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access
{
    public class clsRolesData
    {
        public static DataTable GetAllRoles()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection connection =new SqlConnection(clsDataAccessSettings.ConnectionString))
                {
                    string query = "SELECT RoleId, [Name] FROM Roles";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        dt.Load(reader);
                    }
                }
            }
            catch
            {
                dt = null;
            }

            return dt;
        }
    }
}
