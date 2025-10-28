using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixModelPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "smtp.gmail.com");

            migrationBuilder.UpdateData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 6,
                column: "Value",
                value: "Airline Manager");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "smtp.example.com");

            migrationBuilder.UpdateData(
                table: "AppConfigurations",
                keyColumn: "Id",
                keyValue: 6,
                column: "Value",
                value: "AirlineManager");
        }
    }
}
