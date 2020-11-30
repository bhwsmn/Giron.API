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
    public class PostRepository : IPostRepository
    {
        private readonly GironContext _context;

        public PostRepository(GironContext context)
        {
            _context = context;
        }
        
        public async Task<Post> CreatePostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<bool> PostExistsAsync(Guid id)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == id);

            return postExists;
        }

        public async Task<IEnumerable<Post>> GetPostsAsync(
            int pageSize,
            int pageNumber,
            string username = default,
            string domainName = default,
            string searchQueryTitle = default,
            string searchQueryBody = default,
            DateTime fromDate = default,
            DateTime toDate = default
        )
        {
            var posts = await _context.Posts
                .ConditionalWhere(() => username != default, p => p.ApplicationUser.NormalizedUserName == username)
                .ConditionalWhere(() => domainName != default, p => p.Domain.Name == domainName)
                .ConditionalWhere(() => fromDate != default, p => p.CreatedAt >= fromDate)
                .ConditionalWhere(() => toDate != default, p => p.CreatedAt <= toDate)
                .ConditionalWhere(() => searchQueryTitle != default, 
                    p=> p.Title
                        .DecodeBase64OrReturnOriginal()
                        .ToLowerInvariant()
                        .Contains(searchQueryTitle.ToLowerInvariant()))
                .ConditionalWhere(() => searchQueryBody != default, 
                    p=> p.Body
                        .DecodeBase64OrReturnOriginal()
                        .ToLowerInvariant()
                        .Contains(searchQueryBody.ToLowerInvariant()))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(p => p.Likes.Count)
                .ToListAsync();

            return posts;
        }

        public async Task<Post> GetPostByIdAsync(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            return post;
        }

        public async Task<Post> UpdatePostAsync(Guid id, string newTitle, string newBody)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            post.Title = newTitle ?? post.Title;
            post.Body = newBody ?? post.Body;
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task DeletePostAsync(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            _context.Posts.Remove(post);
            
            await _context.SaveChangesAsync();
        }

        public async Task<Post> LikeAsync(Guid id, ApplicationUser user)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            
            post.Likes.Add(new Like
            {
                ApplicationUser = user
            });
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<bool> LikeExistsAsync(Guid id, ApplicationUser user)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

            var likeExists = post.Likes.Any(l => l.ApplicationUser == user);

            return likeExists;
        }

        public async Task DeleteLikeAsync(Guid id, ApplicationUser user)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            var like = post.Likes.FirstOrDefault(l => l.ApplicationUser == user);
            post.Likes.Remove(like);
            
            await _context.SaveChangesAsync();
        }
    }
}