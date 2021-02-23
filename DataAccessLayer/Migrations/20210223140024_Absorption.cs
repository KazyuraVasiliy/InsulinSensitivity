using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class Absorption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AbsorptionRateOfCarbohydrates",
                table: "Users",
                nullable: false,
                defaultValue: 20m);

            migrationBuilder.AddColumn<decimal>(
                name: "AbsorptionRateOfFats",
                table: "Users",
                nullable: false,
                defaultValue: 17m);

            migrationBuilder.AddColumn<decimal>(
                name: "AbsorptionRateOfProteins",
                table: "Users",
                nullable: false,
                defaultValue: 24m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsorptionRateOfCarbohydrates",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AbsorptionRateOfFats",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AbsorptionRateOfProteins",
                table: "Users");
        }
    }
}
