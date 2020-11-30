using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Giron.API.DbContexts;
using Giron.API.Entities;
using Giron.API.Extensions;
using Giron.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giron.API.Services.Classes
{
    public class CommentRepository : ICommentRepository
    {
        private readonly GironContext _context;

        public CommentRepository(GironContext context)
        {
            _context = context;
        }
        
        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<bool> CommentExistsAsync(Guid id)
        {
            var commentExists = await _context.Comments.AnyAsync(c => c.Id == id);

            return commentExists;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByUserAsync(
            string username,
            int pageSize,
            int pageNumber,
            string searchQueryBody = default,
            DateTime fromDate = default,
            DateTime toDate = default
        )
        {
            var comments = await _context.Comments
                .Where(c => c.ApplicationUser.NormalizedUserName == username.ToUpperInvariant())
                .ConditionalWhere(() => fromDate != default, p => p.CreatedAt >= fromDate)
                .ConditionalWhere(() => toDate != default, p => p.CreatedAt <= toDate)
                .ConditionalWhere(() => searchQueryBody != default, 
                    c=> c.Body
                        .DecodeBase64OrReturnOriginal()
                        .ToLowerInvariant()
                        .Contains(searchQueryBody.ToLowerInvariant()))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(p => p.Likes.Count)
                .ToListAsync();
            
            return comments;
        }

        public async Task<Comment> GetCommentByIdAsync(Guid id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            return comment;
        }

        public async Task<Comment> UpdateCommentMessageAsync(Guid id, string newMessage)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            comment.Body = newMessage;
            await _context.SaveChangesAsync();
            
            return comment;
        }

        public async Task DeleteCommentAsync(Guid id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            _context.Comments.Remove(comment);
            
            await _context.SaveChangesAsync();
        }
        
        public async Task<Comment> LikeAsync(Guid id, ApplicationUser user)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            comment.Likes.Add(new Like
            {
                ApplicationUser = user
            });
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<bool> LikeExistsAsync(Guid id, ApplicationUser user)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            var likeExists = comment.Likes.Any(l => l.ApplicationUser == user);

            return likeExists;
        }

        public async Task DeleteLikeAsync(Guid id, ApplicationUser user)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            var like = comment.Likes.FirstOrDefault(l => l.ApplicationUser == user);
            comment.Likes.Remove(like);
            
            await _context.SaveChangesAsync();
        }
    }
}