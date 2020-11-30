using Giron.API.Entities;
using Giron.API.Models.Output;
using Profile = AutoMapper.Profile;

namespace Giron.API.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserOutputDto>();
        }
    }
}