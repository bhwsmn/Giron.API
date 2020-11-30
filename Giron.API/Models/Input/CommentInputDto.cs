using System;
using System.ComponentModel.DataAnnotations;

namespace Giron.API.Models.Input
{
    public class CommentInputDto
    {
        [Required, MinLength(1), MaxLength(50000)]
        public string Body { get; set; }

        [Required]
        public Guid PostId { get; set; }
    }
}