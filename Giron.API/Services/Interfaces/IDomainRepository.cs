using System.Collections.Generic;
using System.Threading.Tasks;
using Giron.API.Entities;

namespace Giron.API.Services.Interfaces
{
    public interface IDomainRepository
    {
        Task<IEnumerable<Domain>> GetAllDomainsAsync();
        Task<Domain> GetDomainByNameAsync(string name);
        Task<bool> DomainExistsAsync(string name);
        Task<Domain> CreateDomainAsync(Domain domain);
    }
}