using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyId",
                table: "PharmacyPhoneNumbers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyId",
                table: "PharmacyDocument",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPhoneNumbers_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDocument_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyPhoneNumbers_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyDocument_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "PharmacyDocument");
        }
    }
}
