using Giron.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Giron.API.DbContexts
{
    public class RefreshTokenLogContext : DbContext
    {
        public RefreshTokenLogContext(DbContextOptions<RefreshTokenLogContext> options) : base(options)
        { }

        public DbSet<DisabledRefreshToken> DisabledRefreshTokens { get; set; }
    }
}