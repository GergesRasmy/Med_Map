using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpPurposeAndAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "OtpCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "OtpCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "OtpCodes");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "OtpCodes");
        }
    }
}
