namespace Workflow.IO.Shared.Authorization
{
    public static class Permissions
    {
        public const string ProjectsCreate = "projects.create";
        public const string ProjectsEdit = "projects.edit";
        public const string ProjectsDelete = "projects.delete";
        public const string ProjectsView = "projects.view";

        public const string TasksCreate = "tasks.create";
        public const string TasksEdit = "tasks.edit";
        public const string TasksDelete = "tasks.delete";
        public const string TasksView = "tasks.view";

        public const string BillingManage = "billing.manage";
        public const string WorkspaceManage = "workspace.manage";
        public const string OrganizationManage = "organization.manage";
    }
}
