using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeDefaultSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppConfigurations",
                columns: new[] { "Id", "Category", "Description", "IsEncrypted", "Key", "LastModified", "LastModifiedBy", "Value" },
                values: new object[] { 25, "Theme", "Default theme for new users and guests (auto, light, or dark)", false, "Theme_Default", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "auto" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 25);
        }
    }
}
