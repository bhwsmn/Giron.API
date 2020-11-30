using System;
using System.Collections.Generic;

namespace Giron.API.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public virtual Domain Domain { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }  
    }
}