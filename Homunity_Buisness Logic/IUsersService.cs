using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Entities;
using Homunity_Shared_DTOs;
using Homunity_Shared_DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    public interface IUsersService
    {
        Task<(bool success, bool phoneConflict, UserResponse user)> RegisterAsync(RegisterUserRequest request);
        Task<UserResponse> LoginAsync(string phone, string password);
        Task<UserResponse> GetProfileAsync(int userId);
        Task<bool> UpdateStatusAsync(int userId, bool isActive);
        Task<bool> DeleteAsync(int userId);
 
    }
}
