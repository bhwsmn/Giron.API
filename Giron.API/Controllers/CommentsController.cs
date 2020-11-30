using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Giron.API.Entities;
using Giron.API.Helpers;
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
    [Authorize]
    [Route("/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager
        )
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentOutputDto>> CreateCommentAsync(CommentInputDto commentInputDto)
        {
            var post = await _postRepository.GetPostByIdAsync(commentInputDto.PostId);

            if (post == null)
            {
                return NotFound();
            }

            var user = await HttpContextHelpers.GetUserAsync(_httpContextAccessor.HttpContext, _userManager);

            if (user == null)
            {
                return Unauthorized();
            }

            var comment = _mapper.Map<Comment>(commentInputDto);

            comment.ApplicationUser = user;
            comment.Post = post;

            await _commentRepository.CreateCommentAsync(comment);

            return CreatedAtAction(
                actionName: nameof(GetCommentByIdAsync),
                routeValues: new {id = comment.Id},
                value: _mapper.Map<CommentOutputDto>(comment)
            );
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CommentOutputDto>>> GetCommentsByUsernameAsync(
            [FromQuery] CommentQueryDto commentQueryDto)
        {
            var comments = await _commentRepository.GetCommentsByUserAsync(
                username: commentQueryDto.Username,
                pageSize: commentQueryDto.PageSize,
                pageNumber: commentQueryDto.PageNumber,
                searchQueryBody: commentQueryDto.SearchQueryBody,
                fromDate: commentQueryDto.FromDate,
                toDate: commentQueryDto.ToDate
            );
            
            return _mapper.Map<List<CommentOutputDto>>(comments);
        }

        [HttpGet("{id:guid}")]
        [HttpHead("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentOutputDto>> GetCommentByIdAsync(Guid id)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return _mapper.Map<CommentOutputDto>(comment);
        }
        
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentOutputDto>> UpdateCommentMessageAsync(Guid id, CommentInputDto commentInputDto)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }
            
            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: comment.ApplicationUser
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }

            await _commentRepository.UpdateCommentMessageAsync(id, commentInputDto.Body);

            return _mapper.Map<CommentOutputDto>(comment);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentOutputDto>> DeleteCommentAsync(Guid id)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            if (await HttpContextHelpers.IsAdminAsync(_httpContextAccessor.HttpContext, _userManager))
            {
                await _commentRepository.DeleteCommentAsync(id);

                return NoContent();
            }

            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: comment.ApplicationUser
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }

            await _commentRepository.DeleteCommentAsync(id);

            return NoContent();
        }
        
        [HttpPost("{id:guid}/likes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CommentOutputDto>> LikeAsync(Guid id)
        {
            if (!await _commentRepository.CommentExistsAsync(id))
            {
                return NotFound();
            }
            
            var user = await HttpContextHelpers.GetUserAsync(_httpContextAccessor.HttpContext, _userManager);

            if (user == null)
            {
                return Unauthorized();
            }

            if (await _commentRepository.LikeExistsAsync(id, user))
            {
                return Conflict();
            }

            var comment = await _commentRepository.LikeAsync(id, user);
            var commentOutputDto = _mapper.Map<CommentOutputDto>(comment);
            commentOutputDto.HasLiked = true;

            return commentOutputDto;
        }

        [HttpDelete("{id:guid}/likes")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLikeAsync(Guid id)
        {
            if (!await _commentRepository.CommentExistsAsync(id))
            {
                return NotFound();
            }
            
            var user = await HttpContextHelpers.GetUserAsync(_httpContextAccessor.HttpContext, _userManager);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!await _commentRepository.LikeExistsAsync(id, user))
            {
                return NotFound();
            }

            await _commentRepository.DeleteLikeAsync(id, user);

            return NoContent();
        }
    }
}