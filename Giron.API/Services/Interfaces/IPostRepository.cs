using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Giron.API.Entities;

namespace Giron.API.Services.Interfaces
{
    public interface IPostRepository
    {
        Task<Post> CreatePostAsync(Post post);
        Task<bool> PostExistsAsync(Guid id);

        Task<IEnumerable<Post>> GetPostsAsync(
            int pageSize,
            int pageNumber,
            string username = default,
            string domainName = default,
            string searchQueryTitle = default,
            string searchQueryBody = default,
            DateTime fromDate = default,
            DateTime toDate = default
        );
        Task<Post> GetPostByIdAsync(Guid id);
        Task<Post> UpdatePostAsync(Guid id, string newTitle, string newBody);
        Task DeletePostAsync(Guid id);
        Task<Post> LikeAsync(Guid id, ApplicationUser user);
        Task<bool> LikeExistsAsync(Guid id, ApplicationUser user);
        Task DeleteLikeAsync(Guid id, ApplicationUser user);
    }
}