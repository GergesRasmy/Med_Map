using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class pharmacy_lists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseImageUrl",
                table: "Pharmacy");

            migrationBuilder.RenameColumn(
                name: "PhoneNumbers",
                table: "Pharmacy",
                newName: "doctorPhoneNumber");

            migrationBuilder.RenameColumn(
                name: "PharmacistPhoneNumber",
                table: "Pharmacy",
                newName: "doctorName");

            migrationBuilder.RenameColumn(
                name: "NationalIdUrl",
                table: "Pharmacy",
                newName: "address");

            migrationBuilder.CreateTable(
                name: "PharmacyDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PharmacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyDocument_Pharmacy_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyPhoneNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PharmacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyPhoneNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyPhoneNumbers_Pharmacy_PharmacyId",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyDocument_PharmacyId",
                table: "PharmacyDocument",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPhoneNumbers_PharmacyId",
                table: "PharmacyPhoneNumbers",
                column: "PharmacyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PharmacyDocument");

            migrationBuilder.DropTable(
                name: "PharmacyPhoneNumbers");

            migrationBuilder.RenameColumn(
                name: "doctorPhoneNumber",
                table: "Pharmacy",
                newName: "PhoneNumbers");

            migrationBuilder.RenameColumn(
                name: "doctorName",
                table: "Pharmacy",
                newName: "PharmacistPhoneNumber");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Pharmacy",
                newName: "NationalIdUrl");

            migrationBuilder.AddColumn<string>(
                name: "LicenseImageUrl",
                table: "Pharmacy",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
