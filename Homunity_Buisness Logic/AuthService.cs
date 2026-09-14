// AuthService.cs — الجزء المعدّل بس
using Homunity_Buisness_Logic;
using Homunity_Data_Access.Repositories;
using Homunity_Shared_DTOs.Users;
using Microsoft.Extensions.Logging;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repo;
    private readonly IJwtService _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository repo, IJwtService jwt, ILogger<AuthService> logger)
    {
        _repo = repo;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<(bool success, UserResponse user, string token, DateTime? expiresAt)> LoginAsync(string phone, string password)
    {
        _logger.LogInformation("Login attempt for phone {PhoneMasked}", MaskPhone(phone));

        var entity = await _repo.GetByPhoneAsync(phone);
        if (entity == null || !entity.IsActive)
        {
            _logger.LogWarning("Login failed: user not found or inactive for phone {PhoneMasked}", MaskPhone(phone));
            return (false, null, null, null);
        }

        if (PasswordHasher.Hash(password) != entity.PasswordHash)
        {
            _logger.LogWarning("Login failed: invalid password for UserId {UserId}", entity.UserId);
            return (false, null, null, null);
        }

        var roleName = entity.Role?.Name ?? "";
        var (token, expiresAt) = _jwt.GenerateToken(entity.UserId, entity.Phone, roleName);

        _logger.LogInformation("Login succeeded for UserId {UserId}", entity.UserId);

        var userResponse = new UserResponse
        {
            UserID = entity.UserId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Phone = entity.Phone,
            RoleId = entity.RoleId,
            IsActive = entity.IsActive
        };

        return (true, userResponse, token, expiresAt);
    }

    private static string MaskPhone(string phone) =>
        string.IsNullOrEmpty(phone) || phone.Length < 4 ? "****" : $"****{phone[^4..]}";
}