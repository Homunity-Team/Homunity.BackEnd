using Homunity_Shared_DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Shared_DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserResponse UserData { get; set; }
    }
}
