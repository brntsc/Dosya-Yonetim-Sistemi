using AutoMapper;
using Uyg.API.DTOs;
using Uyg.API.Models;

namespace Uyg.API.Mapping
{
    public class MapProfile : Profile
    {
       public MapProfile() 
        {
            CreateMap<Product,ProductDto>().ReverseMap();
            CreateMap<Category,CategoryDto>().ReverseMap();
            CreateMap<AppUser,UserDto>().ReverseMap();
        }
    }
}
