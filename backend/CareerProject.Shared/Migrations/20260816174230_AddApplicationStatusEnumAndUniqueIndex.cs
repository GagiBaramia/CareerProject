using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerProject.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationStatusEnumAndUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_JobId",
                table: "Applications");

            // Applications table is empty at this point in the project (Task 17 introduces
            // the flow), so drop+recreate avoids a text->integer cast Postgres can't do automatically.
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Applications");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Applications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId_PersonId",
                table: "Applications",
                columns: new[] { "JobId", "PersonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_JobId_PersonId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Applications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId",
                table: "Applications",
                column: "JobId");
        }
    }
}
