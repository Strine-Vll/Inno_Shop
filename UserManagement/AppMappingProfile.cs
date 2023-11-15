using AutoMapper;
using UserManagement.Dtos;
using UserManagement.Models;

namespace UserManagement
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile() 
        {
            CreateMap<User, UserDto>().ReverseMap();

        }
    }
}
