using AngularAuthAPI.Dtos;
using AngularAuthAPI.Models;
using AutoMapper;

namespace AngularAuthAPI.Mapper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<ApplicationUser, UserDto>();
                
        }
    }
}
