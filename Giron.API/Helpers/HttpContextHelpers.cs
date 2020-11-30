using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Giron.API.Entities;
using Giron.API.Models.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Giron.API.Helpers
{
    public static class HttpContextHelpers
    {
        public static async Task<ApplicationUser> GetUserAsync(HttpContext httpContext, UserManager<ApplicationUser> userManager)
        {
            try
            {
                var username = httpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Name)?.Value;
                var user = await userManager.FindByNameAsync(username);

                return user;
            }
            catch
            {
                return default;
            }
        }

        public static async Task<bool> IsAdminAsync(HttpContext httpContext, UserManager<ApplicationUser> userManager)
        {
            try
            {
                var username = httpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Name)?.Value;
                var user = await userManager.FindByNameAsync(username);

                var isAdmin = (await userManager.GetRolesAsync(user)).Any(r => r == Roles.Admin);

                return isAdmin;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsAuthorizedUserAsync(
            HttpContext httpContext,
            UserManager<ApplicationUser> userManager,
            ApplicationUser userToValidateAuthority
        )
        {
            try
            {
                var username = httpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Name)?.Value;
                var user = await userManager.FindByNameAsync(username);

                var isAuthorizedUser = userToValidateAuthority == user;

                return isAuthorizedUser;
            }
            catch
            {
                return false;
            }
        }
    }
}