using System.ComponentModel.DataAnnotations;

namespace Giron.API.Models.Input
{
    public class SessionCreateDto
    {
        [Required, MinLength(6), MaxLength(50)]
        [RegularExpression("([a-zA-Z0-9_]+)", ErrorMessage = "Only alphanumeric and underscores (_) are allowed for username")]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }

        [RegularExpression("^[a-z0-9]*$", ErrorMessage = "Invalid token")]
        public string Token2FA { get; set; }
    }
}