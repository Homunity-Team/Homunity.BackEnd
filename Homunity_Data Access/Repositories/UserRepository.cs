using Homunity_Data_Access.Data;
using Homunity_Data_Access.Entities;
using Microsoft.EntityFrameworkCore;
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Data_Access.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HomunityDbContext _db;
        public UserRepository(HomunityDbContext db) => _db = db;

        public Task<UserEntity> GetByIdAsync(int userId) =>
      _db.Users.AsNoTracking().Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);

        public Task<UserEntity> GetByPhoneAsync(string phone) =>
            _db.Users.AsNoTracking().Include(u => u.Role).FirstOrDefaultAsync(u => u.Phone == phone);
        public Task<bool> PhoneExistsAsync(string phone) =>
            _db.Users.AsNoTracking().AnyAsync(u => u.Phone == phone);

        public async Task<int> AddAsync(UserEntity user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user.UserId;
        }

        public async Task<bool> UpdateStatusAsync(int userId, bool isActive)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;
            user.IsActive = isActive;
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;
            _db.Users.Remove(user);
            return await _db.SaveChangesAsync() > 0;
        }


        public async Task<bool> UpdatePasswordHashAsync(int userId, string newHash)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;
            user.PasswordHash = newHash;
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
