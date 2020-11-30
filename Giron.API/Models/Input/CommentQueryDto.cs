using System;
using System.ComponentModel.DataAnnotations;

namespace Giron.API.Models.Input
{
    public class CommentQueryDto
    {
        [Range(1, 50, ErrorMessage = "PageSize must be within 1 to 50")]
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        
        [RegularExpression("([a-zA-Z0-9_]+)", ErrorMessage = "Only alphanumeric and underscores (_) are allowed for username")]
        public string Username { get; set; }
        public string SearchQueryBody { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}