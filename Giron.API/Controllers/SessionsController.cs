using System.Threading.Tasks;
using Giron.API.Models.Constants;
using Giron.API.Models.Input;
using Giron.API.Models.Output;
using Giron.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Giron.API.Controllers
{
    [ApiController]
    [Route("/sessions")]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionRepository _sessionRepository;

        public SessionsController(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenOutputDto>> CreateSessionAsync(SessionCreateDto sessionCreateDto)
        {
            var isCredentialValid = await _sessionRepository.IsCredentialValidAsync(
                sessionCreateDto.Username,
                sessionCreateDto.Password,
                sessionCreateDto.Token2FA
            );

            if (!isCredentialValid)
            {
                return Unauthorized(new ErrorDto {Message = "Invalid Credential"});
            }

            var tokensDictionary = await _sessionRepository.CreateSessionAsync(sessionCreateDto.Username);

            var tokenOutputDto = new TokenOutputDto
            {
                AccessToken = tokensDictionary[TokenTypes.Access],
                RefreshToken = tokensDictionary[TokenTypes.Refresh]
            };

            return tokenOutputDto;
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenOutputDto>> CreateAccessTokenWithRefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var (isValidRefreshToken, accessToken) =
                await _sessionRepository.CreateAccessTokenWithRefreshTokenAsync(refreshTokenDto.RefreshToken);

            if (!isValidRefreshToken)
            {
                return BadRequest(new ErrorDto {Message = "Invalid refresh token"});
            }
            
            var tokenOutputDto = new TokenOutputDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenDto.RefreshToken
            };

            return tokenOutputDto;
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteSessionAsync(RefreshTokenDto refreshTokenDto)
        {
            if (!await _sessionRepository.DisableRefreshTokenAsync(refreshTokenDto.RefreshToken))
            {
                return BadRequest(new ErrorDto {Message =  "Invalid refresh token"});
            }

            return NoContent();
        }
    }
}