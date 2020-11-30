using System;

namespace Giron.API.Models.Output
{
    public class CommentOutputDto
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public string Username { get; set; }
        public Guid PostId { get; set; }
        public int TotalLikes { get; set; }
        public bool HasLiked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; } 
    }
}