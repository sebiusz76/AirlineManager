using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordPolicyConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppConfigurations",
                columns: new[] { "Id", "Category", "Description", "IsEncrypted", "Key", "LastModified", "LastModifiedBy", "Value" },
                values: new object[,]
                {
                    { 11, "Password Security", "Password must contain at least one digit (0-9)", false, "Security_Password_RequireDigit", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "true" },
                    { 12, "Password Security", "Password must contain at least one lowercase letter (a-z)", false, "Security_Password_RequireLowercase", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "true" },
                    { 13, "Password Security", "Password must contain at least one uppercase letter (A-Z)", false, "Security_Password_RequireUppercase", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "true" },
                    { 14, "Password Security", "Password must contain at least one special character (!@#$%^&* etc.)", false, "Security_Password_RequireNonAlphanumeric", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "false" },
                    { 15, "Password Security", "Minimum password length", false, "Security_Password_RequiredLength", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "8" },
                    { 16, "Password Security", "Minimum number of unique characters in password", false, "Security_Password_RequiredUniqueChars", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "1" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 16);
        }
    }
}
