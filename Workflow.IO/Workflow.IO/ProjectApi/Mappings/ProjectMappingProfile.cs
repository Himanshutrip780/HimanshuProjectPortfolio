using AutoMapper;
using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Dto;

namespace ProjectApi.Mappings
{
    public class ProjectMappingProfile : Profile
    {
        public ProjectMappingProfile()
        {
            CreateMap<Project, ProjectResponseDto>();
        }
    }
}