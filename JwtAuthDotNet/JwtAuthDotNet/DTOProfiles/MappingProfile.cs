using AutoMapper;
using JwtAuthDotNet.Dtos.Response;
using JwtAuthDotNet.Model;

namespace JwtAuthDotNet.DTOProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDto>();
            CreateMap<Role, RoleResponseDto>();
        }
    }
}
