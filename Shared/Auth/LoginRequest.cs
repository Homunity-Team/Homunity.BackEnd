using System.ComponentModel.DataAnnotations;

namespace Homunity_Shared_DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        public string Phone { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
