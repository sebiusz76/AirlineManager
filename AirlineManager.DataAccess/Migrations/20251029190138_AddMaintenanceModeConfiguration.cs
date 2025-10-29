using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceModeConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppConfigurations",
                columns: new[] { "Id", "Category", "Description", "IsEncrypted", "Key", "LastModified", "LastModifiedBy", "Value" },
                values: new object[,]
                {
                    { 22, "Maintenance", "Enable maintenance mode (only SuperAdmin can access the site)", false, "Maintenance_Mode_Enabled", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "false" },
                    { 23, "Maintenance", "Message displayed to users during maintenance mode", false, "Maintenance_Mode_Message", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "We are currently performing scheduled maintenance. Please check back soon." },
                    { 24, "Maintenance", "Estimated end time of maintenance (leave empty if unknown)", false, "Maintenance_Mode_EstimatedEnd", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 24);
        }
    }
}
