using System.Text.Json;
using RealtimeApi.Model.Dto;
using Workflow.IO.Shared.Contracts;

namespace RealtimeApi.Services
{
    public static class RealtimeEventMapper
    {
        public static RealtimeEventDto Map(
            IntegrationEventRequest integrationEvent)
        {
            using var payload =
                TryParsePayload(integrationEvent.PayloadJson);

            return new RealtimeEventDto
            {
                EventType = integrationEvent.EventType,
                EntityType = integrationEvent.EntityType,
                EntityId = integrationEvent.EntityId,
                ProjectId = ReadGuid(payload, "projectId"),
                TaskId =
                    ReadGuid(payload, "taskId") ??
                    (integrationEvent.EntityType == "Task"
                        ? integrationEvent.EntityId
                        : null),
                ActorId = integrationEvent.ActorId,
                RecipientId = integrationEvent.RecipientId,
                Description = integrationEvent.Description,
                PayloadJson = integrationEvent.PayloadJson,
                OccurredAt = DateTime.UtcNow
            };
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

        private static Guid? ReadGuid(
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

            return Guid.TryParse(
                property.GetString(),
                out var parsed)
                ? parsed
                : null;
        }
    }
}
