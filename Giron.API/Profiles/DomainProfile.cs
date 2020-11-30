using AutoMapper;
using Giron.API.Entities;
using Giron.API.Models.Input;
using Giron.API.Models.Output;

namespace Giron.API.Profiles
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<DomainCreateDto, Domain>();
            CreateMap<Domain, DomainOutputDto>();
        }
    }
}