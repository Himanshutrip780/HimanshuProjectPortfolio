using AutoMapper;
using UserApi.Model.Domian.Entities;
using UserApi.Model.Dto;

namespace UserApi.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>()

                // ✅ Entity -> DTO Mapping

                .ForMember(
                    dest => dest.UserId,
                    opt => opt.MapFrom(src => src.UserId))

                //.ForMember(
                //    dest => dest.Email,
                //    opt => opt.MapFrom(src => src.Email))

                .ForMember(
                    dest => dest.FirstName,
                    opt => opt.MapFrom(src => src.FirstName))

                .ForMember(
                    dest => dest.LastName,
                    opt => opt.MapFrom(src => src.LastName))

                .ForMember(
                    dest => dest.AvatarUrl,
                    opt => opt.MapFrom(src => src.AvatarUrl));

                //.ForMember(
                //    dest => dest.Role,
                //    opt => opt.MapFrom(src => src.Role.ToString()));
        }
    }
}