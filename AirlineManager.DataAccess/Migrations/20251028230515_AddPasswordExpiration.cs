using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordChangedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AppConfigurations",
                columns: new[] { "Id", "Category", "Description", "IsEncrypted", "Key", "LastModified", "LastModifiedBy", "Value" },
                values: new object[] { 8, "Security", "Number of days before password expires (0 = never expires)", false, "Security_PasswordExpirationDays", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "90" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "PasswordChangedAt",
                table: "AspNetUsers");
        }
    }
}
