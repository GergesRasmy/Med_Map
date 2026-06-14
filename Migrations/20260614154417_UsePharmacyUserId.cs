using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class UsePharmacyUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PharmacyProfile_PharmacyProfileId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInventory_PharmacyProfile_PharmacyProfileId",
                table: "PharmacyInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_PharmacyProfile_PharmacyProfileId",
                table: "Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Wallet_PharmacyProfileId",
                table: "Wallet");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInventory_PharmacyProfileId",
                table: "PharmacyInventory");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PharmacyProfileId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "PharmacyInventory");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyUserId",
                table: "Wallet",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyUserId",
                table: "PharmacyInventory",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyUserId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_PharmacyUserId",
                table: "Wallet",
                column: "PharmacyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInventory_PharmacyUserId",
                table: "PharmacyInventory",
                column: "PharmacyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PharmacyUserId",
                table: "Orders",
                column: "PharmacyUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Pharmacy_PharmacyUserId",
                table: "Orders",
                column: "PharmacyUserId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInventory_Pharmacy_PharmacyUserId",
                table: "PharmacyInventory",
                column: "PharmacyUserId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_Pharmacy_PharmacyUserId",
                table: "Wallet",
                column: "PharmacyUserId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Pharmacy_PharmacyUserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInventory_Pharmacy_PharmacyUserId",
                table: "PharmacyInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_Pharmacy_PharmacyUserId",
                table: "Wallet");

            migrationBuilder.DropIndex(
                name: "IX_Wallet_PharmacyUserId",
                table: "Wallet");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInventory_PharmacyUserId",
                table: "PharmacyInventory");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PharmacyUserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PharmacyUserId",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "PharmacyUserId",
                table: "PharmacyInventory");

            migrationBuilder.DropColumn(
                name: "PharmacyUserId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "Wallet",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "PharmacyInventory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_PharmacyProfileId",
                table: "Wallet",
                column: "PharmacyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInventory_PharmacyProfileId",
                table: "PharmacyInventory",
                column: "PharmacyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PharmacyProfileId",
                table: "Orders",
                column: "PharmacyProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PharmacyProfile_PharmacyProfileId",
                table: "Orders",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInventory_PharmacyProfile_PharmacyProfileId",
                table: "PharmacyInventory",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_PharmacyProfile_PharmacyProfileId",
                table: "Wallet",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
