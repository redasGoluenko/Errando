using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusLogs_Users_RunnerId",
                table: "StatusLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_ClientId",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusLogs_Users_RunnerId",
                table: "StatusLogs",
                column: "RunnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_ClientId",
                table: "Tasks",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusLogs_Users_RunnerId",
                table: "StatusLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_ClientId",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusLogs_Users_RunnerId",
                table: "StatusLogs",
                column: "RunnerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_ClientId",
                table: "Tasks",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
