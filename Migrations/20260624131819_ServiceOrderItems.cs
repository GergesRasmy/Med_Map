using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class ServiceOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_MedicineMaster_MedicineId",
                table: "OrderItem");

            migrationBuilder.AlterColumn<Guid>(
                name: "MedicineId",
                table: "OrderItem",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceId",
                table: "OrderItem",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ServiceId",
                table: "OrderItem",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_MedicineMaster_MedicineId",
                table: "OrderItem",
                column: "MedicineId",
                principalTable: "MedicineMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_PharmacyServices_ServiceId",
                table: "OrderItem",
                column: "ServiceId",
                principalTable: "PharmacyServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_MedicineMaster_MedicineId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_PharmacyServices_ServiceId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_ServiceId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "OrderItem");

            migrationBuilder.AlterColumn<Guid>(
                name: "MedicineId",
                table: "OrderItem",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_MedicineMaster_MedicineId",
                table: "OrderItem",
                column: "MedicineId",
                principalTable: "MedicineMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
