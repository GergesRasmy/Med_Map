using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class jwt_nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSession_JwtId",
                table: "UserSession");

            migrationBuilder.AlterColumn<string>(
                name: "JwtId",
                table: "UserSession",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_JwtId",
                table: "UserSession",
                column: "JwtId",
                unique: true,
                filter: "[JwtId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSession_JwtId",
                table: "UserSession");

            migrationBuilder.AlterColumn<string>(
                name: "JwtId",
                table: "UserSession",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_JwtId",
                table: "UserSession",
                column: "JwtId",
                unique: true);
        }
    }
}
