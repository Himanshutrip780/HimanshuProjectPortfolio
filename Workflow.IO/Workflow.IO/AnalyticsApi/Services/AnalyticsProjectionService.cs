using System.Text.Json;
using AnalyticsApi.Data;
using AnalyticsApi.Model.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Workflow.IO.Shared.Contracts;

namespace AnalyticsApi.Services
{
    public class AnalyticsProjectionService : IAnalyticsProjectionService
    {
        private readonly AnalyticsDbContext _context;

        public AnalyticsProjectionService(
            AnalyticsDbContext context)
        {
            _context = context;
        }

        public async Task ProjectAsync(
            IntegrationEventRequest integrationEvent,
            CancellationToken cancellationToken = default)
        {
            using var payload =
                TryParsePayload(integrationEvent.PayloadJson);

            var projectId =
                ReadGuid(payload, "projectId");

            await _context.AnalyticsEvents.AddAsync(
                new AnalyticsEvent(
                    integrationEvent.EventType,
                    integrationEvent.EntityType,
                    integrationEvent.EntityId,
                    projectId,
                    integrationEvent.ActorId,
                    integrationEvent.RecipientId,
                    integrationEvent.Description,
                    integrationEvent.PayloadJson),
                cancellationToken);

            if (integrationEvent.EntityType == "Task")
            {
                await ProjectTaskAsync(
                    integrationEvent,
                    payload,
                    projectId,
                    cancellationToken);
            }

        }

        private async Task ProjectTaskAsync(
            IntegrationEventRequest integrationEvent,
            JsonDocument? payload,
            Guid? projectId,
            CancellationToken cancellationToken)
        {
            var taskId =
                ReadGuid(payload, "taskId") ??
                integrationEvent.EntityId;

            if (!projectId.HasValue)
            {
                return;
            }

            var task =
                await _context.TaskAnalyticsItems
                    .FirstOrDefaultAsync(
                        x => x.TaskId == taskId,
                        cancellationToken);

            if (task == null)
            {
                task = new TaskAnalyticsItem(
                    taskId,
                    projectId.Value);

                await _context.TaskAnalyticsItems.AddAsync(
                    task,
                    cancellationToken);
            }

            if (integrationEvent.EventType == "TaskDeleted")
            {
                task.MarkDeleted();

                return;
            }

            task.ApplySnapshot(
                ReadString(payload, "status"),
                ReadString(payload, "priority"),
                ReadGuid(payload, "assigneeId"),
                ReadGuid(payload, "sprintId"),
                ReadGuid(payload, "epicId"),
                ReadInt(payload, "storyPoints"),
                ReadDateTime(payload, "dueDate"));
        }

        private static JsonDocument? TryParsePayload(
            string? payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                return null;
            }

            try
            {
                return JsonDocument.Parse(payloadJson);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? ReadString(
            JsonDocument? payload,
            string propertyName)
        {
            if (payload == null ||
                !payload.RootElement.TryGetProperty(
                    propertyName,
                    out var property) ||
                property.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return property.GetString();
        }

        private static Guid? ReadGuid(
            JsonDocument? payload,
            string propertyName)
        {
            var value =
                ReadString(payload, propertyName);

            return Guid.TryParse(value, out var parsed)
                ? parsed
                : null;
        }

        private static int? ReadInt(
            JsonDocument? payload,
            string propertyName)
        {
            if (payload == null ||
                !payload.RootElement.TryGetProperty(
                    propertyName,
                    out var property) ||
                property.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number &&
                property.TryGetInt32(out var number))
            {
                return number;
            }

            return int.TryParse(property.GetString(), out var parsed)
                ? parsed
                : null;
        }

        private static DateTime? ReadDateTime(
            JsonDocument? payload,
            string propertyName)
        {
            var value =
                ReadString(payload, propertyName);

            return DateTime.TryParse(value, out var parsed)
                ? parsed
                : null;
        }
    }
}
