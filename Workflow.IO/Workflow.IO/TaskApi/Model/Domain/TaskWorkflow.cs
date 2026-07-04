using Workflow.IO.Shared.Exceptions;
using TaskStatus = TaskApi.Model.Domain.Enums.TaskStatus;

namespace TaskApi.Model.Domain
{
    /// <summary>
    /// Jira-style workflow: Todo → In Progress → Review → Done, with Blocked as interrupt.
    /// </summary>
    public static class TaskWorkflow
    {
        private static readonly IReadOnlyDictionary<
            TaskStatus,
            IReadOnlySet<TaskStatus>> AllowedTransitions =
            new Dictionary<TaskStatus, IReadOnlySet<TaskStatus>>
            {
                [TaskStatus.Todo] =
                    new HashSet<TaskStatus>
                    {
                        TaskStatus.InProgress,
                        TaskStatus.Review,
                        TaskStatus.Blocked,
                        TaskStatus.Done
                    },
                [TaskStatus.InProgress] =
                    new HashSet<TaskStatus>
                    {
                        TaskStatus.Todo,
                        TaskStatus.Review,
                        TaskStatus.Blocked,
                        TaskStatus.Done
                    },
                [TaskStatus.Review] =
                    new HashSet<TaskStatus>
                    {
                        TaskStatus.Todo,
                        TaskStatus.InProgress,
                        TaskStatus.Blocked,
                        TaskStatus.Done
                    },
                [TaskStatus.Blocked] =
                    new HashSet<TaskStatus>
                    {
                        TaskStatus.Todo,
                        TaskStatus.InProgress,
                        TaskStatus.Review,
                        TaskStatus.Done
                    },
                [TaskStatus.Done] =
                    new HashSet<TaskStatus>
                    {
                        TaskStatus.Todo,
                        TaskStatus.InProgress,
                        TaskStatus.Review,
                        TaskStatus.Blocked
                    }
            };

        public static void EnsureTransition(
            TaskStatus current,
            TaskStatus next)
        {
            if (current == next)
            {
                return;
            }

            if (!AllowedTransitions.TryGetValue(
                    current,
                    out var allowed) ||
                !allowed.Contains(next))
            {
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        ["status"] =
                        [
                            $"Cannot move task from '{current}' to '{next}'."
                        ]
                    });
            }
        }

        public static bool IsValidColumnStatus(TaskStatus status) =>
            AllowedTransitions.ContainsKey(status);
    }
}
