using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Giron.API.DbContexts;
using Giron.API.Entities;
using Giron.API.Extensions;
using Giron.API.Helpers;
using Giron.API.Models.Constants;
using Giron.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Giron.API.Services.Classes
{
    public class SessionRepository : ISessionRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RefreshTokenLogContext _refreshTokenLogContext;
        private readonly IConfiguration _configuration;

        public SessionRepository(UserManager<ApplicationUser> userManager,
            RefreshTokenLogContext refreshTokenLogContext, IConfiguration configuration)
        {
            _userManager = userManager;
            _refreshTokenLogContext = refreshTokenLogContext;
            _configuration = configuration;
        }

        public async Task<bool> IsCredentialValidAsync(string username, string password, string token2FA = null)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return false;
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                return false;
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(token2FA))
            {
                return false;
            }

            bool isTokenValid;
            if (token2FA.All(char.IsDigit))
            {
                isTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user,
                    TokenOptions.DefaultAuthenticatorProvider, token2FA);
            }
            else
            {
                isTokenValid = (await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, token2FA)).Succeeded;
            }

            return isTokenValid;
        }

        public async Task<Dictionary<string, string>> CreateSessionAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            var accessAndRefreshTokens = new Dictionary<string, string>
            {
                {"Access", await CreateAccessTokenAsync(user)},
                {"Refresh", await CreateRefreshTokenAsync(user)}
            };

            return accessAndRefreshTokens;
        }

        public async Task<KeyValuePair<bool, string>> CreateAccessTokenWithRefreshTokenAsync(string refreshToken)
        {
            if (await _refreshTokenLogContext.DisabledRefreshTokens.AnyAsync(r => r.RefreshToken == refreshToken))
            {
                return new KeyValuePair<bool, string>(false, "Refresh token is invalid");
            }

            var (isValid, claimsPrincipal) = Jwt.IsRefreshTokenValid(
                refreshToken: refreshToken,
                signingKey: EnvironmentVariables.JwtRefreshSecretKey,
                issuer: EnvironmentVariables.JwtIssuer,
                audience: EnvironmentVariables.JwtAudience
            );

            if (!isValid)
            {
                return new KeyValuePair<bool, string>(false, "Refresh token is invalid");
            }

            var username = claimsPrincipal.Identity.Name;
            var user = await _userManager.FindByNameAsync(username);

            var accessToken = await CreateAccessTokenAsync(user);

            return new KeyValuePair<bool, string>(true, accessToken);
        }

        public async Task<bool> DisableRefreshTokenAsync(string refreshToken)
        {
            var (isValid, _) = Jwt.IsRefreshTokenValid(
                refreshToken: refreshToken,
                signingKey: EnvironmentVariables.JwtRefreshSecretKey,
                issuer: EnvironmentVariables.JwtIssuer,
                audience: EnvironmentVariables.JwtAudience
            );

            if (!isValid)
            {
                return false;
            }

            await _refreshTokenLogContext.DisabledRefreshTokens.AddIfNotExistsAsync(new DisabledRefreshToken
            {
                RefreshToken = refreshToken
            });
            await _refreshTokenLogContext.SaveChangesAsync();

            return true;
        }

        private async Task<string> CreateAccessTokenAsync(ApplicationUser user)
        {
            var accessToken = Jwt.CreateJwtTokenAsync(
                signingKey: EnvironmentVariables.JwtAccessSecretKey,
                username: user.UserName,
                roleName: (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                issuer: EnvironmentVariables.JwtIssuer,
                audience: EnvironmentVariables.JwtAudience,
                tokenLifetime: TimeSpan.FromMinutes(15)
            );

            return accessToken;
        }

        private async Task<string> CreateRefreshTokenAsync(ApplicationUser user)
        {
            var refreshToken = Jwt.CreateJwtTokenAsync(
                signingKey: EnvironmentVariables.JwtRefreshSecretKey,
                username: user.UserName,
                roleName: (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                issuer: EnvironmentVariables.JwtIssuer,
                audience: EnvironmentVariables.JwtAudience,
                tokenLifetime: TimeSpan.FromHours(4)
            );

            return refreshToken;
        }
    }
}