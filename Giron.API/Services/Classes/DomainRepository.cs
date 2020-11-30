using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Giron.API.DbContexts;
using Giron.API.Entities;
using Giron.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Giron.API.Services.Classes
{
    public class DomainRepository : IDomainRepository
    {
        private readonly GironContext _context;

        public DomainRepository(GironContext context)
        {
            _context = context;
        }

        public async Task<Domain> CreateDomainAsync(Domain domain)
        {
            domain.Name = domain.Name.ToLowerInvariant();
            await _context.Domains.AddAsync(domain);
            await _context.SaveChangesAsync();

            return domain;
        }
        
        public async Task<IEnumerable<Domain>> GetAllDomainsAsync()
        {
            var domains = await _context.Domains.ToListAsync();

            return domains;
        }

        public async Task<Domain> GetDomainByNameAsync(string name)
        {
            var normalizedName = name.ToLowerInvariant();
            var domain = await _context.Domains.FirstOrDefaultAsync(d => d.Name == normalizedName);

            return domain;
        }

        public async Task<bool> DomainExistsAsync(string name)
        {
            var normalizedName = name.ToLowerInvariant();
            var domainExists = await _context.Domains.AnyAsync(d => d.Name == normalizedName);

            return domainExists;
        }
        
    }
}