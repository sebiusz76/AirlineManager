using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuditLogRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_UserAuditLogs_AspNetUsers_ModifiedBy",
                table: "UserAuditLogs",
                column: "ModifiedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAuditLogs_AspNetUsers_UserId",
                table: "UserAuditLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAuditLogs_AspNetUsers_ModifiedBy",
                table: "UserAuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAuditLogs_AspNetUsers_UserId",
                table: "UserAuditLogs");
        }
    }
}
