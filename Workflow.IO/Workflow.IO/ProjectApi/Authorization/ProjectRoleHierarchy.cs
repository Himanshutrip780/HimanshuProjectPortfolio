using ProjectApi.Model.Domain.Enums;

namespace ProjectApi.Authorization
{
    public static class ProjectRoleHierarchy
    {
        public static bool MeetsMinimumRole(
            ProjectRole actual,
            ProjectRole minimum) =>
            GetLevel(actual) >= GetLevel(minimum);

        private static int GetLevel(ProjectRole role) =>
            role switch
            {
                ProjectRole.Owner => 4,
                ProjectRole.Admin => 3,
                ProjectRole.Member => 2,
                ProjectRole.Viewer => 1,
                _ => 0
            };
    }
}
