using Homunity_Data_Access;
using System.Data;

namespace Homunity_Buisness_Logic
{
    public class clsRoles
    {
        public int RoleId { get; set; }
        public string? Name { get; set; }

        public static DataTable GetRoles()
        {
            return clsRolesData.GetAllRoles();
        }
    }
}
