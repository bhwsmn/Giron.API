using System.Threading.Tasks;
using AutoMapper;
using Giron.API.Entities;
using Giron.API.Helpers;
using Giron.API.Models.Constants;
using Giron.API.Models.Input;
using Giron.API.Models.Output;
using Giron.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Giron.API.Controllers
{
    [ApiController]
    [Route("/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(IUserRepository userRepository, UserManager<ApplicationUser> userManager, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUserOutputDto>> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            if ((!EnvironmentVariables.IsAdminRegistrationEnabled && userRegisterDto.IsAdmin) ||
                !EnvironmentVariables.IsUserRegistrationEnabled)
            {
                return Forbid();
            }

            if (await _userRepository.UsernameExistsAsync(userRegisterDto.Username))
            {
                return Conflict(new ErrorDto {Message = "Username exists"});
            }

            var (isPasswordValid, passwordInvalidError) = await _userRepository.IsPasswordStrongAsync(userRegisterDto.Password);
            if (isPasswordValid == false)
            {
                return BadRequest(new ErrorDto {Message = passwordInvalidError});
            }

            var user = await _userRepository.RegisterAsync(
                userRegisterDto.Username,
                userRegisterDto.Password,
                userRegisterDto.IsAdmin
            );

            return CreatedAtAction(
                actionName: nameof(UsernameExists),
                routeValues: new {username = user.UserName},
                value: _mapper.Map<ApplicationUserOutputDto>(user)
            );
        }
        
        [HttpHead("{username}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UsernameExists(string username)
        {
            var usernameExists = await _userRepository.UsernameExistsAsync(username);

            if (usernameExists)
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpPatch("{username}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePasswordAsync(string username, UserPasswordChangeDto passwordChangeDto)
        {
            if (!await  _userRepository.UsernameExistsAsync(username))
            {
                return NotFound();
            }
            
            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: await _userManager.FindByNameAsync(username)
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }

            var (isSuccess, errorMessage) = await _userRepository.ChangePasswordAsync(username,
                passwordChangeDto.CurrentPassword, passwordChangeDto.NewPassword);

            if (!isSuccess)
            {
                return BadRequest(new ErrorDto{Message = errorMessage});
            }

            return Ok();
        }

        [HttpHead("{username}/2fa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Is2FAEnabledAsync(string username)
        {
            if (!await  _userRepository.UsernameExistsAsync(username))
            {
                return NotFound();
            }
            
            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: await _userManager.FindByNameAsync(username)
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }
            
            if (await _userRepository.Is2FAEnabledAsync(username))
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpGet("{username}/2fa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<TwoFactorKeyDto>> GetTwoFactorKeyAsync(string username)
        {
            if (!await  _userRepository.UsernameExistsAsync(username))
            {
                return NotFound();
            }
            
            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: await _userManager.FindByNameAsync(username)
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }

            if (await _userRepository.Is2FAEnabledAsync(username))
            {
                return Conflict();
            }
            
            var key = await _userRepository.GenerateTwoFactorKeyAsync(username);

            return new TwoFactorKeyDto
            {
                AuthenticatorKey = key
            };
        }

        [HttpPost("{username}/2fa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TwoFactorRecoveryCodesOutputDto>> Enable2FAAsync(string username, 
            [FromBody] TwoFactorEnableDto twoFactorEnableDto)
        {
            if (!await  _userRepository.UsernameExistsAsync(username))
            {
                return NotFound();
            }

            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: await _userManager.FindByNameAsync(username)
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }
            
            if (!await _userRepository.IsPasswordValidAsync(username, twoFactorEnableDto.Password))
            {
                return Unauthorized(new ErrorDto {Message = "Password is invalid"});
            }
            
            if (!await _userRepository.Is2FATokenValidAsync(username, twoFactorEnableDto.Token.ToString()))
            {
                return Unauthorized(new ErrorDto {Message = "2FA Token is invalid"});
            }

            var recoveryCodes = await _userRepository.Enable2FAAsync(username);

            return new TwoFactorRecoveryCodesOutputDto
            {
                RecoveryCodes = recoveryCodes
            };
        }

        [HttpDelete("{username}/2fa")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Disable2FAAsync(string username, PasswordInputDto passwordInputDto)
        {
            if (!await  _userRepository.UsernameExistsAsync(username))
            {
                return NotFound();
            }
            
            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: await _userManager.FindByNameAsync(username)
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }
            
            if (!await _userRepository.IsPasswordValidAsync(username, passwordInputDto.Password))
            {
                return Unauthorized(new ErrorDto {Message = "Password is invalid"});
            }

            await _userRepository.Disable2FAAsync(username);

            return NoContent();
        }

        [HttpDelete("{username}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUserAsync(string username, PasswordInputDto passwordInputDto)
        {
            if (!await  _userRepository.UsernameExistsAsync(username))
            {
                return NotFound();
            }

            if (await HttpContextHelpers.IsAdminAsync(_httpContextAccessor.HttpContext, _userManager))
            {
                await _userRepository.DeleteUserAsync(username);

                return NoContent();
            }

            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: await _userManager.FindByNameAsync(username)
            );

            if (!isAuthorizedUser || !await _userRepository.IsPasswordValidAsync(username, passwordInputDto.Password)) 
            {
                return Unauthorized();
            }

            await _userRepository.DeleteUserAsync(username);

            return NoContent();
        }
    }
}