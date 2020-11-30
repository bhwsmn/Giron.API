using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Giron.API.Extensions
{
    public static class DbSetExtensions
    {
        public static async Task<EntityEntry<T>> AddIfNotExistsAsync<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? await dbSet.AnyAsync(predicate) : await dbSet.AnyAsync();
            return !exists ? await dbSet.AddAsync(entity) : null;
        }
    }
}