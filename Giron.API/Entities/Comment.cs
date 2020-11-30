using System;
using System.Collections.Generic;

namespace Giron.API.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual Post Post { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }  
    }
}