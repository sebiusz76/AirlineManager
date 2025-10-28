using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLockoutConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppConfigurations",
                columns: new[] { "Id", "Category", "Description", "IsEncrypted", "Key", "LastModified", "LastModifiedBy", "Value" },
                values: new object[,]
                {
                    { 9, "Security", "Maximum number of failed login attempts before account lockout", false, "Security_MaxFailedLoginAttempts", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "5" },
                    { 10, "Security", "Duration of account lockout in minutes after max failed attempts", false, "Security_LockoutDurationMinutes", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "30" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
