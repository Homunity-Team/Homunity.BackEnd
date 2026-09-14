using System.ComponentModel.DataAnnotations;

namespace Homunity_Shared_DTOs.Users
{
    public class RegisterUserRequest
    {
        [Required, StringLength(20, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required, StringLength(20, MinimumLength = 2)]
        public string LastName { get; set; }

        [Required, Phone, StringLength(20)]
        public string Phone { get; set; }

        [Required, StringLength(300, MinimumLength = 4)]
        public string Password { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "RoleId must be a valid positive number.")]
        public int RoleId { get; set; }
    }
}