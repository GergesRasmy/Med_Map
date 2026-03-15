using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class fk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_Pharmacy_PharmacyApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Pharmacy_PharmacyId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_Pharmacy_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInventory_Pharmacy_PharmacyId",
                table: "PharmacyInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_Pharmacy_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_Pharmacy_PharmacyId",
                table: "Wallet");

            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequest_Pharmacy_PharmacyId",
                table: "WithdrawalRequest");

            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequest_PharmacyId",
                table: "WithdrawalRequest");

            migrationBuilder.DropIndex(
                name: "IX_Wallet_PharmacyId",
                table: "Wallet");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyPhoneNumbers_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInventory_PharmacyId",
                table: "PharmacyInventory");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyDocument_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PharmacyId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRequest_PharmacyApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "WithdrawalRequest");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "PharmacyInventory");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PharmacyApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "WithdrawalRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "Wallet",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyProfileId",
                table: "PharmacyPhoneNumbers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "PharmacyInventory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyProfileId",
                table: "PharmacyDocument",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyProfileId",
                table: "DoctorRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_PharmacyProfileId",
                table: "WithdrawalRequest",
                column: "PharmacyProfileId");

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

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRequest_PharmacyProfileId",
                table: "DoctorRequest",
                column: "PharmacyProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_PharmacyProfille_PharmacyProfileId",
                table: "DoctorRequest",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PharmacyProfille_PharmacyProfileId",
                table: "Orders",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyDocument",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInventory_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyInventory",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_PharmacyProfille_PharmacyProfileId",
                table: "Wallet",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequest_PharmacyProfille_PharmacyProfileId",
                table: "WithdrawalRequest",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_PharmacyProfille_PharmacyProfileId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PharmacyProfille_PharmacyProfileId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInventory_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_PharmacyProfille_PharmacyProfileId",
                table: "Wallet");

            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequest_PharmacyProfille_PharmacyProfileId",
                table: "WithdrawalRequest");

            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequest_PharmacyProfileId",
                table: "WithdrawalRequest");

            migrationBuilder.DropIndex(
                name: "IX_Wallet_PharmacyProfileId",
                table: "Wallet");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInventory_PharmacyProfileId",
                table: "PharmacyInventory");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PharmacyProfileId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRequest_PharmacyProfileId",
                table: "DoctorRequest");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "WithdrawalRequest");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "PharmacyInventory");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PharmacyProfileId",
                table: "DoctorRequest");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyId",
                table: "WithdrawalRequest",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyId",
                table: "Wallet",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyProfileId",
                table: "PharmacyPhoneNumbers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyId",
                table: "PharmacyPhoneNumbers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyId",
                table: "PharmacyInventory",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyProfileId",
                table: "PharmacyDocument",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyId",
                table: "PharmacyDocument",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyApplicationUserId",
                table: "DoctorRequest",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequest_PharmacyId",
                table: "WithdrawalRequest",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallet_PharmacyId",
                table: "Wallet",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPhoneNumbers_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInventory_PharmacyId",
                table: "PharmacyInventory",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDocument_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PharmacyId",
                table: "Orders",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRequest_PharmacyApplicationUserId",
                table: "DoctorRequest",
                column: "PharmacyApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_Pharmacy_PharmacyApplicationUserId",
                table: "DoctorRequest",
                column: "PharmacyApplicationUserId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Pharmacy_PharmacyId",
                table: "Orders",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyDocument",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_Pharmacy_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInventory_Pharmacy_PharmacyId",
                table: "PharmacyInventory",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_Pharmacy_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_Pharmacy_PharmacyId",
                table: "Wallet",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequest_Pharmacy_PharmacyId",
                table: "WithdrawalRequest",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
