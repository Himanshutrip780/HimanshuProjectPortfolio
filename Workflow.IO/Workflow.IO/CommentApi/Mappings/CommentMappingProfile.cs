using AutoMapper;
using CommentApi.Model.Domain.Entities;
using CommentApi.Model.Dto;

namespace CommentApi.Mappings
{
    public class CommentMappingProfile : Profile
    {
        public CommentMappingProfile()
        {
            CreateMap<Comment, CommentResponseDto>();
            CreateMap<CommentReaction, CommentReactionDto>();
        }
    }
}
