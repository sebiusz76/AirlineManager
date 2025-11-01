using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLoginHistoryRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_UserLoginHistories_AspNetUsers_UserId",
                table: "UserLoginHistories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLoginHistories_AspNetUsers_UserId",
                table: "UserLoginHistories");
        }
    }
}
