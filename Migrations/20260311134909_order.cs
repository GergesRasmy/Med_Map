using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Med_Map.Migrations
{
    /// <inheritdoc />
    public partial class order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "OrderItem");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "MedicineMaster",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ServicePrice",
                table: "DoctorRequest",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "MedicineMaster");

            migrationBuilder.DropColumn(
                name: "ServicePrice",
                table: "DoctorRequest");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
