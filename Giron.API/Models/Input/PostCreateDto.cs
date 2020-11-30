using System.ComponentModel.DataAnnotations;
using Giron.API.Entities;

namespace Giron.API.Models.Input
{
    public class PostCreateDto
    {
        [Required, MinLength(1), MaxLength(250)]
        public string Title { get; set; }
        
        [Required, MinLength(1),MaxLength(50000)]
        public string Body { get; set; }
        
        [Required, MinLength(1), MaxLength(100)]
        public string DomainName { get; set; }
    }
}