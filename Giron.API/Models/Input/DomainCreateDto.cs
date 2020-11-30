using System.ComponentModel.DataAnnotations;

namespace Giron.API.Models.Input
{
    public class DomainCreateDto
    {
        [Required, MinLength(1), MaxLength(100)]
        [RegularExpression("([a-zA-Z0-9]+)", ErrorMessage = "Only alphanumeric characters are allowed")]
        public string Name { get; set; }
    }
}