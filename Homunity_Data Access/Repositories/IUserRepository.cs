using Homunity_Data_Access.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity> GetByIdAsync(int userId);
        Task<UserEntity> GetByPhoneAsync(string phone);
        Task<bool> PhoneExistsAsync(string phone);
        Task<int> AddAsync(UserEntity user);
        Task<bool> UpdateStatusAsync(int userId, bool isActive);
        Task<bool> DeleteAsync(int userId);
        Task<bool> UpdatePasswordHashAsync(int userId, string newHash);

    }
}
