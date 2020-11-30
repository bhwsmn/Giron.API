using System.ComponentModel.DataAnnotations;

namespace Giron.API.Models.Input
{
    public class TwoFactorEnableDto
    {
        [Required]
        public string Password { get; set; }
        
        [Required]
        public int Token { get; set; }
    }
}