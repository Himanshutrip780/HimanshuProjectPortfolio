namespace TaskApi.Model.Domain.Entities
{
    public class ProjectIssueCounter
    {
        public Guid ProjectId { get; private set; }

        public string ProjectKey { get; private set; } = string.Empty;

        public int LastIssueNumber { get; private set; }

        private ProjectIssueCounter()
        {
        }

        public ProjectIssueCounter(
            Guid projectId,
            string projectKey)
        {
            ProjectId = projectId;

            ProjectKey = projectKey.Trim().ToUpperInvariant();

            LastIssueNumber = 0;
        }

        public int AllocateNext()
        {
            LastIssueNumber++;

            return LastIssueNumber;
        }

        public void UpdateProjectKey(string projectKey)
        {
            ProjectKey = projectKey.Trim().ToUpperInvariant();
        }

        public void SyncLastIssueNumber(int maxExisting)
        {
            if (LastIssueNumber < maxExisting)
            {
                LastIssueNumber = maxExisting;
            }
        }
    }
}
