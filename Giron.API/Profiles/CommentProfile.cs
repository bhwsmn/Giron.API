using AutoMapper;
using Giron.API.Entities;
using Giron.API.Models.Input;
using Giron.API.Models.Output;

namespace Giron.API.Profiles
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<CommentInputDto, Comment>();
            CreateMap<Comment, CommentOutputDto>()
                .ForMember(
                    dest => dest.Username,
                    options => options.MapFrom(
                        source => source.ApplicationUser.UserName))
                .ForMember(
                dest => dest.TotalLikes,
                options => options.MapFrom(
                    source => source.Likes.Count));
        }
    }
}