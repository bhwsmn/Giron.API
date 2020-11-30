using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Giron.API.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
    }
}