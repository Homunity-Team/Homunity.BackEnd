using Homunity_Shared_DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IAuthService
    {
        Task<(bool success, UserResponse user, string token, DateTime? expiresAt)> LoginAsync(string phone, string password);
    }
}
