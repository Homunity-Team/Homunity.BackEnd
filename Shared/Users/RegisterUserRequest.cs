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

        /// <summary>
        /// Only Student (3) or Owner (2) are allowed for public registration. Enforced in UsersService as well.
        /// </summary>
        [Required]
        [Range(2, 3, ErrorMessage = "RoleId must be Owner (2) or Student (3). Admin self-registration is not allowed.")]
        public int RoleId { get; set; }
    }
}
