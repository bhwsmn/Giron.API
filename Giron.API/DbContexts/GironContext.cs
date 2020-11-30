using System;
using Giron.API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Giron.API.DbContexts
{
    public class GironContext : IdentityDbContext<ApplicationUser>
    {
        public GironContext(DbContextOptions<GironContext> options) : base(options)
        {
            ChangeTracker.Tracked += OnEntityTracked;
            ChangeTracker.StateChanged += OnEntityStateChanged;
        }

        public DbSet<Domain> Domains { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Domain>()
                .HasIndex(d => d.Name)
                .IsUnique();

            builder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Post>()
                .HasMany(p => p.Likes)
                .WithOne(l => l.Post)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Comment>()
                .HasMany(c => c.Likes)
                .WithOne(l => l.Comment)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.ApplicationUser)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.ApplicationUser)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Likes)
                .WithOne(l => l.ApplicationUser)
                .OnDelete(DeleteBehavior.SetNull);
        }

        private void OnEntityTracked(object sender, EntityTrackedEventArgs e)
        {
            if (!e.FromQuery && e.Entry.State == EntityState.Added && e.Entry.Entity is Post post)
            {
                post.CreatedAt = DateTime.UtcNow;
            }
            if (!e.FromQuery && e.Entry.State == EntityState.Added && e.Entry.Entity is Comment comment)
            {
                comment.CreatedAt = DateTime.UtcNow;
            }
        }

        private void OnEntityStateChanged(object sender, EntityStateChangedEventArgs e)
        {
            if (e.NewState == EntityState.Modified && e.Entry.Entity is Post post)
            {
                post.ModifiedAt = DateTime.UtcNow;
            }
            if (e.NewState == EntityState.Modified && e.Entry.Entity is Comment comment)
            {
                comment.ModifiedAt = DateTime.UtcNow;
            }
        }
    }
}
