using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Giron.API.Entities;

namespace Giron.API.Services.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<bool> CommentExistsAsync(Guid id);

        Task<IEnumerable<Comment>> GetCommentsByUserAsync(
            string username,
            int pageSize,
            int pageNumber,
            string searchQueryBody = default,
            DateTime fromDate = default,
            DateTime toDate = default
        );
        Task<Comment> GetCommentByIdAsync(Guid id);
        Task<Comment> UpdateCommentMessageAsync(Guid id, string newMessage);
        Task DeleteCommentAsync(Guid id);
        Task<Comment> LikeAsync(Guid id, ApplicationUser user);
        Task<bool> LikeExistsAsync(Guid id, ApplicationUser user);
        Task DeleteLikeAsync(Guid id, ApplicationUser user);
    }
}