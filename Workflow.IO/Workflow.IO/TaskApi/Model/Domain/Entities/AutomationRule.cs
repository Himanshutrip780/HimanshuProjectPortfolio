using System;

namespace TaskApi.Model.Domain.Entities
{
    public class AutomationRule
    {
        public Guid AutomationRuleId { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string TriggerType { get; private set; } = string.Empty; // e.g. "StatusChanged"
        public string TriggerValue { get; private set; } = string.Empty; // e.g. "Done"
        public string ActionType { get; private set; } = string.Empty;   // e.g. "Reassign"
        public string ActionValue { get; private set; } = string.Empty;  // e.g. "UserId"
        public bool IsEnabled { get; private set; } = true;
        public DateTime CreatedAt { get; private set; }

        private AutomationRule()
        {
        }

        public AutomationRule(Guid projectId, string name, string triggerType, string triggerValue, string actionType, string actionValue)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");
            if (string.IsNullOrWhiteSpace(triggerType)) throw new ArgumentException("TriggerType is required");
            if (string.IsNullOrWhiteSpace(actionType)) throw new ArgumentException("ActionType is required");

            AutomationRuleId = Guid.NewGuid();
            ProjectId = projectId;
            Name = name.Trim();
            TriggerType = triggerType.Trim();
            TriggerValue = triggerValue?.Trim() ?? string.Empty;
            ActionType = actionType.Trim();
            ActionValue = actionValue?.Trim() ?? string.Empty;
            IsEnabled = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Toggle(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}
