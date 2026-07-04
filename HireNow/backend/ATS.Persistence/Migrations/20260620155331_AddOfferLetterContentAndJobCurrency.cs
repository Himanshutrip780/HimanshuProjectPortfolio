using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferLetterContentAndJobCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OfferLetterContent",
                table: "Offers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfferLetterContent",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Jobs");
        }
    }
}
