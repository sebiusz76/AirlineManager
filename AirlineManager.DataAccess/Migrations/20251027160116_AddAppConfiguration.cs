using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAppConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppConfigurations",
                columns: new[] { "Id", "Category", "Description", "IsEncrypted", "Key", "LastModified", "LastModifiedBy", "Value" },
                values: new object[,]
                {
                    { 1, "SMTP", "SMTP server hostname", false, "SMTP_Host", new DateTime(2025, 10, 27, 16, 1, 14, 326, DateTimeKind.Utc).AddTicks(2794), "System", "smtp.example.com" },
                    { 2, "SMTP", "SMTP server port", false, "SMTP_Port", new DateTime(2025, 10, 27, 16, 1, 14, 326, DateTimeKind.Utc).AddTicks(3245), "System", "587" },
                    { 3, "SMTP", "SMTP authentication username", false, "SMTP_Username", new DateTime(2025, 10, 27, 16, 1, 14, 326, DateTimeKind.Utc).AddTicks(3247), "System", "" },
                    { 4, "SMTP", "SMTP authentication password", true, "SMTP_Password", new DateTime(2025, 10, 27, 16, 1, 14, 326, DateTimeKind.Utc).AddTicks(3248), "System", "" },
                    { 5, "SMTP", "Sender email address", false, "SMTP_FromEmail", new DateTime(2025, 10, 27, 16, 1, 14, 326, DateTimeKind.Utc).AddTicks(3249), "System", "noreply@example.com" },
                    { 6, "SMTP", "Sender display name", false, "SMTP_FromName", new DateTime(2025, 10, 27, 16, 1, 14, 326, DateTimeKind.Utc).AddTicks(3250), "System", "AirlineManager" },
                    { 7, "SMTP", "Enable SSL/TLS encryption", false, "SMTP_EnableSSL", new DateTime(2025, 10, 27, 16, 1, 14, 326, DateTimeKind.Utc).AddTicks(3251), "System", "true" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_Category",
                table: "AppConfigurations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_Key",
                table: "AppConfigurations",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigurations");
        }
    }
}
