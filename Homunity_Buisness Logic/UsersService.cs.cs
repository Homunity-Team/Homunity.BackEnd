using Homunity_Data_Access.Entities;
using Homunity_Data_Access.Repositories;
using Homunity_Shared_DTOs.Users;
using Microsoft.Extensions.Logging;

namespace Homunity_Buisness_Logic
{
    // Service محقون (Scoped) — يحل محل الاعتماد الـstatic على clsUsersData
    public class UsersService : IUsersService
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<UsersService> _logger;

        public UsersService(IUserRepository repo,ILogger<UsersService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<(bool success, bool phoneConflict, UserResponse user)> RegisterAsync(RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName)) return (false, false, null);
            if (string.IsNullOrWhiteSpace(request.LastName)) return (false, false, null);
            if (string.IsNullOrWhiteSpace(request.Phone)) return (false, false, null);
            if (string.IsNullOrWhiteSpace(request.Password)) return (false, false, null);
            if (request.Password.Length < 4) return (false, false, null);

            if (await _repo.PhoneExistsAsync(request.Phone))
            {
                _logger.LogWarning("Register failed: phone already exists");
                return (false, true, null);
            }

            var entity = new UserEntity
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                PasswordHash = PasswordHasher.Hash(request.Password), // ← بقت BCrypt تلقائيًا
                RoleId = request.RoleId,
                IsActive = true
            };

            entity.UserId = await _repo.AddAsync(entity);

            _logger.LogInformation("New user registered: UserId {UserId}, RoleId {RoleId}", entity.UserId, entity.RoleId);

            return (true, false, MapToResponse(entity));
        }
        public async Task<UserResponse> LoginAsync(string phone, string password)
        {
            var user = await _repo.GetByPhoneAsync(phone);

            if (user == null || !user.IsActive)
                return null;

            if (PasswordHasher.Hash(password) != user.PasswordHash)
                return null;

            return MapToResponse(user);
        }

        public async Task<UserResponse> GetProfileAsync(int userId)
        {
            var user = await _repo.GetByIdAsync(userId);

            return user == null ? null : MapToResponse(user);
        }

        public Task<bool> UpdateStatusAsync(int userId, bool isActive)
            => _repo.UpdateStatusAsync(userId, isActive);

        public Task<bool> DeleteAsync(int userId)
            => _repo.DeleteAsync(userId);

        private static UserResponse MapToResponse(UserEntity u) => new UserResponse
        {
            UserID = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Phone = u.Phone,
            RoleId = u.RoleId,
            IsActive = u.IsActive
        };
    }
}