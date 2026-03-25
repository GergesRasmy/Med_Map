using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKeys2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_PharmacyProfille_PharmacyProfileId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PharmacyProfille_PharmacyProfileId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacy_PharmacyProfille_ActiveProfileId",
                table: "Pharmacy");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacy_PharmacyProfille_PendingProfileId",
                table: "Pharmacy");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInventory_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_PharmacyProfille_PharmacyProfileId",
                table: "Wallet");

            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequest_PharmacyProfille_PharmacyProfileId",
                table: "WithdrawalRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PharmacyProfille",
                table: "PharmacyProfille");

            migrationBuilder.RenameTable(
                name: "PharmacyProfille",
                newName: "PharmacyProfile");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PharmacyProfile",
                table: "PharmacyProfile",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_PharmacyProfile_PharmacyProfileId",
                table: "DoctorRequest",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PharmacyProfile_PharmacyProfileId",
                table: "Orders",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacy_PharmacyProfile_ActiveProfileId",
                table: "Pharmacy",
                column: "ActiveProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacy_PharmacyProfile_PendingProfileId",
                table: "Pharmacy",
                column: "PendingProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyProfileId",
                table: "PharmacyDocument",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInventory_PharmacyProfile_PharmacyProfileId",
                table: "PharmacyInventory",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfile_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfile_PharmacyProfileId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_PharmacyProfile_PharmacyProfileId",
                table: "Wallet",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequest_PharmacyProfile_PharmacyProfileId",
                table: "WithdrawalRequest",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorRequest_PharmacyProfile_PharmacyProfileId",
                table: "DoctorRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PharmacyProfile_PharmacyProfileId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacy_PharmacyProfile_ActiveProfileId",
                table: "Pharmacy");

            migrationBuilder.DropForeignKey(
                name: "FK_Pharmacy_PharmacyProfile_PendingProfileId",
                table: "Pharmacy");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfile_PharmacyProfileId",
                table: "PharmacyDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInventory_PharmacyProfile_PharmacyProfileId",
                table: "PharmacyInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfile_PharmacyId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfile_PharmacyProfileId",
                table: "PharmacyPhoneNumbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_PharmacyProfile_PharmacyProfileId",
                table: "Wallet");

            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequest_PharmacyProfile_PharmacyProfileId",
                table: "WithdrawalRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PharmacyProfile",
                table: "PharmacyProfile");

            migrationBuilder.RenameTable(
                name: "PharmacyProfile",
                newName: "PharmacyProfille");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PharmacyProfille",
                table: "PharmacyProfille",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorRequest_PharmacyProfille_PharmacyProfileId",
                table: "DoctorRequest",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PharmacyProfille_PharmacyProfileId",
                table: "Orders",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacy_PharmacyProfille_ActiveProfileId",
                table: "Pharmacy",
                column: "ActiveProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pharmacy_PharmacyProfille_PendingProfileId",
                table: "Pharmacy",
                column: "PendingProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyDocument_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyDocument",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInventory_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyInventory",
                column: "PharmacyProfileId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyPhoneNumbers_PharmacyProfille_PharmacyProfileId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_PharmacyProfille_PharmacyProfileId",
                table: "Wallet",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequest_PharmacyProfille_PharmacyProfileId",
                table: "WithdrawalRequest",
                column: "PharmacyProfileId",
                principalTable: "PharmacyProfille",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
