using System.Collections.Generic;
using System.Threading.Tasks;
using Giron.API.Entities;

namespace Giron.API.Services.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser> RegisterAsync(string username, string password, bool isAdmin);
        Task<KeyValuePair<bool, string>> IsPasswordStrongAsync(string password);
        Task<bool> IsPasswordValidAsync(string username, string password);
        Task<bool> Is2FATokenValidAsync(string username, string token);
        Task<bool> Is2FARecoveryCodeValidAsync(string username, string recoveryCode);
        Task<bool> UsernameExistsAsync(string username);
        Task<KeyValuePair<bool, string>> ChangePasswordAsync(string username, string currentPassword, string newPassword);

        Task<bool> Is2FAEnabledAsync(string username);
        Task<string> GenerateTwoFactorKeyAsync(string username);
        Task<IEnumerable<string>> Enable2FAAsync(string username);
        Task Disable2FAAsync(string username);
        Task DeleteUserAsync(string username);
    }
}