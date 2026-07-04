using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActivityApi.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityCreatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Activities_CreatedAt",
                schema: "activity",
                table: "Activities",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Activities_CreatedAt",
                schema: "activity",
                table: "Activities");
        }
    }
}
