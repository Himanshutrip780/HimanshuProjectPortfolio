using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkspaceMembers",
                schema: "project",
                columns: table => new
                {
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceMembers", x => new { x.WorkspaceId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                schema: "project",
                columns: table => new
                {
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.WorkspaceId);
                });
                
            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                schema: "project",
                table: "Projects",
                type: "uuid",
                nullable: true);

            // ✅ Backfill Workspaces for existing Projects
            migrationBuilder.Sql(@"
                INSERT INTO project.""Workspaces"" (""WorkspaceId"", ""Name"", ""Description"", ""OrganizationId"", ""CreatedAt"", ""UpdatedAt"")
                SELECT DISTINCT ""OrganizationId"", 'Personal Workspace', 'Auto-generated workspace', ""OrganizationId"", NOW(), NOW()
                FROM project.""Projects"";

                UPDATE project.""Projects""
                SET ""WorkspaceId"" = ""OrganizationId"";
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkspaceId",
                schema: "project",
                table: "Projects",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceMembers",
                schema: "project");

            migrationBuilder.DropTable(
                name: "Workspaces",
                schema: "project");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                schema: "project",
                table: "Projects");
        }
    }
}
