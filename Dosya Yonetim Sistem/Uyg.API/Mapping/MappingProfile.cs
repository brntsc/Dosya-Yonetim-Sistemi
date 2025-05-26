using AutoMapper;
using Uyg.API.DTOs;
using Uyg.API.Models;

namespace Uyg.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // File mappings
            CreateMap<FileModel, FileDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.FileTags, opt => opt.MapFrom(src => src.FileTags.Select(ft => ft.Id)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.FileTags.Select(t => t.TagName)))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0));

            // Category mappings
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Comment mappings
            CreateMap<Comment, CommentDto>().ReverseMap();

            // FileTag mappings
            CreateMap<FileTag, FileTagDto>().ReverseMap();

            CreateMap<AppUser, UserDto>().ReverseMap();
            CreateMap<AppUser, RegisterDto>().ReverseMap();
            CreateMap<LoginDto, AppUser>();
        }
    }
} 