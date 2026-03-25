using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKey4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyProfileId1",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfile_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyPhoneNumbers_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyDocument_PharmacyProfileId1",
                table: "PharmacyDocument");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId1",
                table: "PharmacyDocument");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyId",
                table: "PharmacyPhoneNumbers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId1",
                table: "PharmacyDocument",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPhoneNumbers_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDocument_PharmacyProfileId1",
                table: "PharmacyDocument",
                column: "PharmacyProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyProfileId1",
                table: "PharmacyDocument",
                column: "PharmacyProfileId1",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfile_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
