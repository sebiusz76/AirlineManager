using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDataRetentionConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppConfigurations",
                columns: new[] { "Id", "Category", "Description", "IsEncrypted", "Key", "LastModified", "LastModifiedBy", "Value" },
                values: new object[,]
                {
                    { 17, "Data Retention", "Number of days to retain application logs (0 = keep forever)", false, "DataRetention_ApplicationLogs_Days", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "90" },
                    { 18, "Data Retention", "Number of days to retain login history (0 = keep forever)", false, "DataRetention_LoginHistory_Days", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "180" },
                    { 19, "Data Retention", "Number of days to retain audit logs (0 = keep forever)", false, "DataRetention_AuditLogs_Days", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "365" },
                    { 20, "Data Retention", "Number of days to retain inactive sessions (0 = keep forever)", false, "DataRetention_InactiveSessions_Days", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "30" },
                    { 21, "Data Retention", "Enable automatic cleanup of old data based on retention policies", false, "DataRetention_EnableAutoCleanup", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "true" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 21);
        }
    }
}
