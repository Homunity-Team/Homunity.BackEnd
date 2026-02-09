using Homunity_Data_Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public class clsPropertyStatus
    {
        public static bool IsValidStatus(int statusId)
        {
            if (statusId <= 0)
                return false;

            return clsPropertyStatusData.IsStatusExists(statusId);
        }
    }
}
