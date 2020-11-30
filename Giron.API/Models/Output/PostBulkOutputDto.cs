using System;
using System.Collections.Generic;

namespace Giron.API.Models.Output
{
    public class PostBulkOutputDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string DomainName { get; set; }
        public string Username { get; set; }
        public int TotalLikes { get; set; }
        public bool HasLiked { get; set; }
        public int TotalComments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }  
    }
}