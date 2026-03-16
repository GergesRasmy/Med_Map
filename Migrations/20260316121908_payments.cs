using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class payments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtpCodes_AspNetUsers_UserId",
                table: "OtpCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSession_AspNetUsers_UserId",
                table: "UserSession");

            migrationBuilder.AddForeignKey(
                name: "FK_OtpCodes_AspNetUsers_UserId",
                table: "OtpCodes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSession_AspNetUsers_UserId",
                table: "UserSession",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtpCodes_AspNetUsers_UserId",
                table: "OtpCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSession_AspNetUsers_UserId",
                table: "UserSession");

            migrationBuilder.AddForeignKey(
                name: "FK_OtpCodes_AspNetUsers_UserId",
                table: "OtpCodes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSession_AspNetUsers_UserId",
                table: "UserSession",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
