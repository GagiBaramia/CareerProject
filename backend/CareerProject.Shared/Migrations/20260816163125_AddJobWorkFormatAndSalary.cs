using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerProject.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddJobWorkFormatAndSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalaryCurrency",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalaryMax",
                table: "Jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalaryMin",
                table: "Jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkFormat",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalaryCurrency",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SalaryMax",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SalaryMin",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "WorkFormat",
                table: "Jobs");
        }
    }
}
