using System.ComponentModel.DataAnnotations;

namespace Giron.API.Models.Input
{
    public class UserPasswordChangeDto
    {
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        public string NewPassword { get; set; }
    }
}