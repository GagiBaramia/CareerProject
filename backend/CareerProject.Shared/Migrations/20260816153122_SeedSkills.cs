using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareerProject.Shared.Migrations
{
    /// <inheritdoc />
    public partial class SeedSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("044ba8a0-51eb-41ed-a4a7-63d30b4835d3"), "Angular" },
                    { new Guid("04bef3cb-a298-49b1-ae40-7921b7530b1b"), "SQL" },
                    { new Guid("2b87718d-b638-48d5-a8f5-0c2566974803"), "PostgreSQL" },
                    { new Guid("55f9afbb-f68b-4709-a189-8cffe25cec68"), "JavaScript" },
                    { new Guid("645360b7-3a40-4b6e-b2af-7c63ba8ab959"), "HTML" },
                    { new Guid("6a908eef-9fa3-4c1c-a091-0a351fd29f7f"), "C#" },
                    { new Guid("7110b157-31c4-4683-9f44-909734f94196"), "Redis" },
                    { new Guid("7151df15-375b-4fd8-bf53-ec4a99872f10"), "Java" },
                    { new Guid("776a6a2c-c04b-4a1c-a294-439df0eadeed"), "Docker" },
                    { new Guid("858b1dff-f15b-44fc-8335-849dd4c1e498"), "Spring Boot" },
                    { new Guid("8a22c113-106f-42af-b5c4-7a23fe1ec239"), "Git" },
                    { new Guid("984edcb0-ef2a-4ede-9c31-e1d7008e8513"), "AWS" },
                    { new Guid("a205340a-6964-42e9-9322-7a52dd454811"), "RabbitMQ" },
                    { new Guid("af530ac8-9a3d-4628-ac00-f4262e35f226"), "REST API" },
                    { new Guid("afff14fb-4893-49c0-9e13-c8b5a2fb97dc"), "CSS" },
                    { new Guid("e8f70325-47ee-4e3b-8007-c25c7395daad"), "TypeScript" },
                    { new Guid("ee3eedd0-25c4-4633-9154-c8d34c9825ec"), "Node.js" },
                    { new Guid("f6e3e75e-f4c7-4445-a950-bdcd242a2766"), ".NET" },
                    { new Guid("faad0c20-7a75-4d57-8317-821b8d88cbb5"), "React" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("044ba8a0-51eb-41ed-a4a7-63d30b4835d3"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("04bef3cb-a298-49b1-ae40-7921b7530b1b"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("2b87718d-b638-48d5-a8f5-0c2566974803"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("55f9afbb-f68b-4709-a189-8cffe25cec68"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("645360b7-3a40-4b6e-b2af-7c63ba8ab959"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("6a908eef-9fa3-4c1c-a091-0a351fd29f7f"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("7110b157-31c4-4683-9f44-909734f94196"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("7151df15-375b-4fd8-bf53-ec4a99872f10"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("776a6a2c-c04b-4a1c-a294-439df0eadeed"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("858b1dff-f15b-44fc-8335-849dd4c1e498"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("8a22c113-106f-42af-b5c4-7a23fe1ec239"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("984edcb0-ef2a-4ede-9c31-e1d7008e8513"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("a205340a-6964-42e9-9322-7a52dd454811"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("af530ac8-9a3d-4628-ac00-f4262e35f226"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("afff14fb-4893-49c0-9e13-c8b5a2fb97dc"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("e8f70325-47ee-4e3b-8007-c25c7395daad"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("ee3eedd0-25c4-4633-9154-c8d34c9825ec"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("f6e3e75e-f4c7-4445-a950-bdcd242a2766"));

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: new Guid("faad0c20-7a75-4d57-8317-821b8d88cbb5"));
        }
    }
}
