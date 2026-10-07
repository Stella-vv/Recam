using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Remp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0406610f-7f58-47bb-8549-48aefd18d37b", "8b09c6b8-89c3-4161-a363-425a4adf1e6e", "photographyCompany", "PHOTOGRAPHYCOMPANY" },
                    { "83d84d57-c75c-434d-af94-2c3a6e868162", "c11c055e-c153-4135-a0d9-4fa3f7e079fe", "user", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0406610f-7f58-47bb-8549-48aefd18d37b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "83d84d57-c75c-434d-af94-2c3a6e868162");
        }
    }
}
