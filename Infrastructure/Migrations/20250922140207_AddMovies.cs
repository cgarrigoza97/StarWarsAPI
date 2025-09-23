using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMovies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3955a54e-5bac-4d08-8e1c-c04493be77f8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d74a8274-5d2e-48e0-9f9d-e36941ddc4dc");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "288f4d91-d62f-4cae-8476-483cf6e427dd", "ce2ae769-b576-4d7a-bfc7-a85cf2914656", "User", "USER" },
                    { "be9a450d-e68d-4a39-a241-d2e4968f8bec", "056d0f6d-1bf3-47e7-a495-6e57be04d131", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "288f4d91-d62f-4cae-8476-483cf6e427dd");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "be9a450d-e68d-4a39-a241-d2e4968f8bec");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3955a54e-5bac-4d08-8e1c-c04493be77f8", "c49e9c20-b5da-4ed8-961a-6a114e57d19b", "Admin", "ADMIN" },
                    { "d74a8274-5d2e-48e0-9f9d-e36941ddc4dc", "c5640b59-fe13-4866-962a-f4acd73c619e", "User", "USER" }
                });
        }
    }
}
