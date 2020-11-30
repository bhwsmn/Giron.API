using AutoMapper;
using Giron.API.Entities;
using Giron.API.Models.Input;
using Giron.API.Models.Output;

namespace Giron.API.Profiles
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<PostCreateDto, Post>();
            CreateMap<Post, PostSingleOutputDto>()
                .ForMember(
                    dest => dest.Username,
                    options => options.MapFrom(
                        source => source.ApplicationUser.UserName))
                .ForMember(
                    dest => dest.TotalLikes,
                    options => options.MapFrom(
                        source => source.Likes.Count))
                .ForMember(dest => dest.TotalComments,
                    options => options.MapFrom(
                        source => source.Comments.Count));
            CreateMap<Post, PostBulkOutputDto>()
                .ForMember(
                    dest => dest.Username,
                    options => options.MapFrom(
                        source => source.ApplicationUser.UserName))
                .ForMember(
                    dest => dest.TotalLikes,
                    options => options.MapFrom(
                        source => source.Likes.Count))
                .ForMember(dest => dest.TotalComments,
                    options => options.MapFrom(
                        source => source.Comments.Count));
        }
    }
}