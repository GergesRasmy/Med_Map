using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class unified_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiChatSession_Customer_CustomerId",
                table: "AiChatSession");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_Customer_CustomerId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_Pharmacy_PharmacyId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customer_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Pharmacy_PharmacyId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_Pharmacy_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInventory_Pharmacy_PharmacyId",
                table: "PharmacyInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_Pharmacy_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_Pharmacy_PharmacyId",
                table: "Wallet");

            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequest_Pharmacy_PharmacyId",
                table: "WithdrawalRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pharmacy",
                table: "Pharmacy");

            migrationBuilder.DropIndex(
                name: "IX_Pharmacy_ApplicationUserId",
                table: "Pharmacy");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRequest_CustomerId",
                table: "DoctorRequest");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRequest_PharmacyId",
                table: "DoctorRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Customer",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_ApplicationUserId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Pharmacy");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DoctorRequest");

            migrationBuilder.DropColumn(
                name: "PharmacyId",
                table: "DoctorRequest");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacyId",
                table: "WithdrawalRequest",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacyId",
                table: "Wallet",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacyId",
                table: "PharmacyPhoneNumbers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacyId",
                table: "PharmacyInventory",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacyId",
                table: "PharmacyDocument",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacyId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "CustomerApplicationUserId",
                table: "DoctorRequest",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PharmacyApplicationUserId",
                table: "DoctorRequest",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "AiChatSession",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pharmacy",
                table: "Pharmacy",
                column: "ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customer",
                table: "Customer",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRequest_CustomerApplicationUserId",
                table: "DoctorRequest",
                column: "CustomerApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRequest_PharmacyApplicationUserId",
                table: "DoctorRequest",
                column: "PharmacyApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiChatSession_Customer_CustomerId",
                table: "AiChatSession",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_Customer_CustomerApplicationUserId",
                table: "DoctorRequest",
                column: "CustomerApplicationUserId",
                principalTable: "Customer",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_Pharmacy_PharmacyApplicationUserId",
                table: "DoctorRequest",
                column: "PharmacyApplicationUserId",
                principalTable: "Pharmacy",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customer_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customer",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiChatSession_Customer_CustomerId",
                table: "AiChatSession");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_Customer_CustomerApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_Pharmacy_PharmacyApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customer_CustomerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Pharmacy_PharmacyId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_Pharmacy_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInventory_Pharmacy_PharmacyId",
                table: "PharmacyInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_Pharmacy_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_Pharmacy_PharmacyId",
                table: "Wallet");

            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequest_Pharmacy_PharmacyId",
                table: "WithdrawalRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pharmacy",
                table: "Pharmacy");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRequest_CustomerApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.DropIndex(
                name: "IX_DoctorRequest_PharmacyApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Customer",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CustomerApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.DropColumn(
                name: "PharmacyApplicationUserId",
                table: "DoctorRequest");

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyId",
                table: "WithdrawalRequest",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyId",
                table: "Wallet",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyId",
                table: "PharmacyPhoneNumbers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyId",
                table: "PharmacyInventory",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyId",
                table: "PharmacyDocument",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Pharmacy",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "PharmacyId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DoctorRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PharmacyId",
                table: "DoctorRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Customer",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "AiChatSession",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pharmacy",
                table: "Pharmacy",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customer",
                table: "Customer",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacy_ApplicationUserId",
                table: "Pharmacy",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRequest_CustomerId",
                table: "DoctorRequest",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRequest_PharmacyId",
                table: "DoctorRequest",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_ApplicationUserId",
                table: "Customer",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AiChatSession_Customer_CustomerId",
                table: "AiChatSession",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_Customer_CustomerId",
                table: "DoctorRequest",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_Pharmacy_PharmacyId",
                table: "DoctorRequest",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customer_CustomerId",
                table: "Orders",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Pharmacy_PharmacyId",
                table: "Orders",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_Pharmacy_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInventory_Pharmacy_PharmacyId",
                table: "PharmacyInventory",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_Pharmacy_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_Pharmacy_PharmacyId",
                table: "Wallet",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequest_Pharmacy_PharmacyId",
                table: "WithdrawalRequest",
                column: "PharmacyId",
                principalTable: "Pharmacy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
