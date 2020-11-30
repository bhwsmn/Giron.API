using System;

namespace Giron.API.Entities
{
    public class Like
    {
        public Guid Id { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual Post Post { get; set; }
        public virtual Comment Comment { get; set; }
    }
}