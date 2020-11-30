using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Giron.API.Helpers
{
    public static class Jwt
    {
        public static string CreateJwtTokenAsync(
            byte[] signingKey,
            string username,
            string roleName,
            string issuer,
            string audience,
            TimeSpan tokenLifetime
        )
        {
            var signingCredentials = new SigningCredentials(
                key: new SymmetricSecurityKey(signingKey),
                algorithm: SecurityAlgorithms.HmacSha512
            );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,username),
                new Claim(ClaimTypes.Role, roleName),
            };
            
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.Add(tokenLifetime),
                signingCredentials: signingCredentials,
                issuer: issuer ?? "Default",
                audience: audience ?? "Default"
            );
            
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }

        public static KeyValuePair<bool, ClaimsPrincipal> IsRefreshTokenValid(
            string refreshToken,
            byte[] signingKey,
            string issuer,
            string audience
        )
        {
            try
            {
                var key = new SymmetricSecurityKey(signingKey);

                var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(refreshToken,
                    new TokenValidationParameters
                    {
                        RequireSignedTokens = true,
                        IssuerSigningKey = key,
                        ValidateLifetime = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience
                    },
                    out _);

                return new KeyValuePair<bool, ClaimsPrincipal>(true, claimsPrincipal);
            }
            catch
            {
                return new KeyValuePair<bool, ClaimsPrincipal>(false, default);
            }
        }
    }
}