using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Domain.Entities
{
    public class TaskLink
    {
        public Guid TaskLinkId { get; private set; }

        public Guid SourceTaskId { get; private set; }

        public Guid TargetTaskId { get; private set; }

        public TaskLinkType LinkType { get; private set; }

        public Guid CreatedById { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private TaskLink()
        {
        }

        public TaskLink(
            Guid sourceTaskId,
            Guid targetTaskId,
            TaskLinkType linkType,
            Guid createdById)
        {
            if (sourceTaskId == targetTaskId)
            {
                throw new ArgumentException(
                    "Cannot link a task to itself");
            }

            TaskLinkId = Guid.NewGuid();

            SourceTaskId = sourceTaskId;

            TargetTaskId = targetTaskId;

            LinkType = linkType;

            CreatedById = createdById;

            CreatedAt = DateTime.UtcNow;
        }
    }
}
