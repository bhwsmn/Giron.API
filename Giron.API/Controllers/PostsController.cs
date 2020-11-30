using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Giron.API.Entities;
using Giron.API.Helpers;
using Giron.API.Models;
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
    [Authorize]
    [Route("/posts")]
    public class PostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostsController(IPostRepository postRepository,
            IDomainRepository domainRepository,
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor, 
            UserManager<ApplicationUser> userManager)
        {
            _postRepository = postRepository;
            _domainRepository = domainRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostBulkOutputDto>> CreatePostAsync(PostCreateDto postCreateDto)
        {
            var user = await HttpContextHelpers.GetUserAsync(_httpContextAccessor.HttpContext, _userManager);

            if (user == null)
            {
                return Unauthorized();
            }
            
            var domain = await _domainRepository.GetDomainByNameAsync(postCreateDto.DomainName);

            if (domain == null)
            {
                return BadRequest(new ErrorDto {Message = "Domain Invalid"});
            }

            var post = _mapper.Map<Post>(postCreateDto);
            post.Domain = domain;
            post.ApplicationUser = user;
            
            await _postRepository.CreatePostAsync(post);

            return CreatedAtAction(
                actionName: nameof(GetPostByIdAsync),
                routeValues: new {id = post.Id},
                value: _mapper.Map<PostBulkOutputDto>(post)
            );
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PostBulkOutputDto>>> GetAllPostsAsync([FromQuery] PostQueryDto postQueryDto)
        {
            var user = await HttpContextHelpers.GetUserAsync(_httpContextAccessor.HttpContext, _userManager);

            var posts = await _postRepository.GetPostsAsync(
                pageSize: postQueryDto.PageSize,
                pageNumber: postQueryDto.PageNumber,
                username: postQueryDto.Username,
                domainName: postQueryDto.DomainName,
                searchQueryTitle: postQueryDto.SearchQueryTitle,
                searchQueryBody: postQueryDto.SearchQueryBody,
                fromDate: postQueryDto.FromDate,
                toDate: postQueryDto.ToDate
            );
            var postOutputDtoList = _mapper.Map<List<PostBulkOutputDto>>(posts);

            if (user != null)
            {
                foreach (var postOutputDto in postOutputDtoList)
                {
                    postOutputDto.HasLiked = await _postRepository.LikeExistsAsync(postOutputDto.Id, user);
                }
            }
            
            return postOutputDtoList;
        }

        [HttpGet("{id:guid}")]
        [HttpHead("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostSingleOutputDto>> GetPostByIdAsync(Guid id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            var postOutputDto = _mapper.Map<PostSingleOutputDto>(post);
            
            var user = await HttpContextHelpers.GetUserAsync(_httpContextAccessor.HttpContext, _userManager);

            if (user != null)
            {
                postOutputDto.HasLiked = await _postRepository.LikeExistsAsync(id, user);
            }
 
            return postOutputDto;
        }

        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostSingleOutputDto>> UpdatePostAsync(Guid id, PostUpdateDto postUpdateDto)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            
            if (!await _postRepository.PostExistsAsync(id))
            {
                return NotFound();
            }
            
            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: post.ApplicationUser
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }

            var updatedPost = await _postRepository.UpdatePostAsync(id, postUpdateDto.Title, postUpdateDto.Body);

            return _mapper.Map<PostSingleOutputDto>(updatedPost);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePostAsync(Guid id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            
            if (!await _postRepository.PostExistsAsync(id))
            {
                return NotFound();
            }
            
            if (await HttpContextHelpers.IsAdminAsync(_httpContextAccessor.HttpContext, _userManager))
            {
                await _postRepository.DeletePostAsync(id);

                return NoContent();
            }

            var isAuthorizedUser = await HttpContextHelpers.IsAuthorizedUserAsync(
                httpContext: _httpContextAccessor.HttpContext,
                userManager: _userManager,
                userToValidateAuthority: post.ApplicationUser
            );

            if (!isAuthorizedUser)
            {
                return Unauthorized();
            }

            await _postRepository.DeletePostAsync(id);

            return NoContent();
        }

        [HttpPost("{id:guid}/likes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PostSingleOutputDto>> LikeAsync(Guid id)
        {
            if (!await _postRepository.PostExistsAsync(id))
            {
                return NotFound();
            }
            
            var user = await HttpContextHelpers.GetUserAsync(_httpContextAccessor.HttpContext, _userManager);

            if (await _postRepository.LikeExistsAsync(id, user))
            {
                return Conflict();
            }

            var post = await _postRepository.LikeAsync(id, user);
            var postOutputDto = _mapper.Map<PostSingleOutputDto>(post);
            postOutputDto.HasLiked = true;

            return postOutputDto;
        }

        [HttpDelete("{id:guid}/likes")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLikeAsync(Guid id)
        {
            if (!await _postRepository.PostExistsAsync(id))
            {
                return NotFound(new ErrorDto { Message = "Post does not exist"});
            }
            
            var user = await HttpContextHelpers.GetUserAsync(_httpContextAccessor.HttpContext, _userManager);

            if (!await _postRepository.LikeExistsAsync(id, user))
            {
                return NotFound();
            }

            await _postRepository.DeleteLikeAsync(id, user);

            return NoContent();
        }
    }
}