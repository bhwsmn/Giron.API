using System.Collections.Generic;
using System.Threading.Tasks;

namespace Giron.API.Services.Interfaces
{
    public interface ISessionRepository
    {
        Task<bool> IsCredentialValidAsync(string username, string password, string token2FA);
        Task<Dictionary<string, string>> CreateSessionAsync(string username);
        Task<KeyValuePair<bool, string>> CreateAccessTokenWithRefreshTokenAsync(string refreshToken);
        Task<bool> DisableRefreshTokenAsync(string refreshToken);
    }
}