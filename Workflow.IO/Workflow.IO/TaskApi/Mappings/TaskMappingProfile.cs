using AutoMapper;
using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Dto;

namespace TaskApi.Mappings
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<TaskItem, TaskResponseDto>();

            CreateMap<TaskLabel, TaskLabelResponseDto>();

            CreateMap<SubTask, SubTaskResponseDto>();

            CreateMap<Board, BoardResponseDto>();

            CreateMap<BoardColumn, BoardColumnResponseDto>();

            CreateMap<Sprint, SprintResponseDto>();

            CreateMap<Epic, EpicResponseDto>();

            CreateMap<TaskWatcher, TaskWatcherResponseDto>();

            CreateMap<Component, ComponentResponseDto>();

            CreateMap<ReleaseVersion, ReleaseVersionResponseDto>();

            CreateMap<TaskLink, TaskLinkResponseDto>();

            CreateMap<WorkLog, WorkLogResponseDto>();

            CreateMap<SavedFilter, SavedFilterResponseDto>();

            CreateMap<Team, TeamResponseDto>();

            CreateMap<TeamMember, TeamMemberResponseDto>();

            CreateMap<AutomationRule, AutomationRuleResponseDto>();
        }
    }
}
