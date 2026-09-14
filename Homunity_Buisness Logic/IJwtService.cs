using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IJwtService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(int userId, string phone, string roleName);
    }
}
