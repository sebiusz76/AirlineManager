using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferredTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredTheme",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredTheme",
                table: "AspNetUsers");
        }
    }
}
