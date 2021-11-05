using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class BasalRate_ExpendableMaterials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasalRate",
                table: "Eatings",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsBatteryReplacement",
                table: "Eatings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCannulaReplacement",
                table: "Eatings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCartridgeReplacement",
                table: "Eatings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCatheterReplacement",
                table: "Eatings",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasalRate",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "IsBatteryReplacement",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "IsCannulaReplacement",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "IsCartridgeReplacement",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "IsCatheterReplacement",
                table: "Eatings");
        }
    }
}
