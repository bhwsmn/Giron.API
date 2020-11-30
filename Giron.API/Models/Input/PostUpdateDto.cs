using System.ComponentModel.DataAnnotations;

namespace Giron.API.Models.Input
{
    public class PostUpdateDto
    {
        [MinLength(1), MaxLength(250)]
        public string Title { get; set; }
        
        [MinLength(1),MaxLength(50000)]
        public string Body { get; set; }
    }
}