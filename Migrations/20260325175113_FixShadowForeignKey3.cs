using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKey3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.RenameColumn(
                name: "PharmacyId",
                table: "PharmacyDocument",
                newName: "PharmacyProfileId1");

            migrationBuilder.RenameIndex(
                name: "IX_PharmacyDocument_PharmacyId",
                table: "PharmacyDocument",
                newName: "IX_PharmacyDocument_PharmacyProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyProfileId1",
                table: "PharmacyDocument",
                column: "PharmacyProfileId1",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyProfileId1",
                table: "PharmacyDocument");

            migrationBuilder.RenameColumn(
                name: "PharmacyProfileId1",
                table: "PharmacyDocument",
                newName: "PharmacyId");

            migrationBuilder.RenameIndex(
                name: "IX_PharmacyDocument_PharmacyProfileId1",
                table: "PharmacyDocument",
                newName: "IX_PharmacyDocument_PharmacyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
