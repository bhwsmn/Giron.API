using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Giron.API.Entities;
using Giron.API.Models;
using Giron.API.Models.Constants;
using Giron.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Giron.API.Services.Classes
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        public async Task<ApplicationUser> RegisterAsync(string username, string password, bool isAdmin)
        {
            var user = new ApplicationUser
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = username
            };

            await _userManager.CreateAsync(user, password);

            await _userManager.AddToRoleAsync(user, isAdmin ? Roles.Admin : Roles.User);

            return user;
        }

        public async Task<KeyValuePair<bool, string>> IsPasswordStrongAsync(string password)
        {
            foreach (var passwordValidator in _userManager.PasswordValidators)
            {
                var result = await passwordValidator.ValidateAsync(_userManager, new ApplicationUser(), password);
                if (!result.Succeeded)
                {
                    return new KeyValuePair<bool, string>(false, result.Errors.FirstOrDefault()?.ToString());
                }
            }
            
            return new KeyValuePair<bool, string>(true, default);
        }

        public async Task<bool> IsPasswordValidAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

            return isPasswordValid;
        }

        public async Task<bool> Is2FATokenValidAsync(string username, string token)
        {
            var user = await _userManager.FindByNameAsync(username);

            var isTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user: user,
                tokenProvider: TokenOptions.DefaultAuthenticatorProvider,
                token: token
            );

            return isTokenValid;
        }

        public async Task<bool> Is2FARecoveryCodeValidAsync(string username, string recoveryCode)
        {
            var user = await _userManager.FindByNameAsync(username);

            var recoveryCodeValidityIdentityResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, recoveryCode);

            return recoveryCodeValidityIdentityResult.Succeeded;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            return user != null;
        }

        public async Task<KeyValuePair<bool, string>> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByNameAsync(username);
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            return new KeyValuePair<bool, string>(result.Succeeded, result.Errors.FirstOrDefault()?.Description);
        }

        public async Task<bool> Is2FAEnabledAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            var is2FAEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            return is2FAEnabled;
        }

        public async Task<string> GenerateTwoFactorKeyAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            await _userManager.ResetAuthenticatorKeyAsync(user);
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);

            return authenticatorKey;
        }

        public async Task<IEnumerable<string>> Enable2FAAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return recoveryCodes;
        }

        public async Task Disable2FAAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            await _userManager.SetTwoFactorEnabledAsync(user, false);
        }
        

        public async Task DeleteUserAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            await _userManager.DeleteAsync(user);
        }
    }
}