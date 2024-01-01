using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class SettingsToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatteryLifespan",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CannulaLifespan",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CartridgeLifespan",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CatheterLifespan",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EatingDuration",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveBasal",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAverageCalculateActive",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCannulaCalculateActive",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCycleCalculateActive",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExerciseCalculateActive",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPregnancy",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LengthGraph",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonitoringLifespan",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatteryLifespan",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CannulaLifespan",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CartridgeLifespan",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CatheterLifespan",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EatingDuration",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActiveBasal",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAverageCalculateActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsCannulaCalculateActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsCycleCalculateActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsExerciseCalculateActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPregnancy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LengthGraph",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MonitoringLifespan",
                table: "Users");
        }
    }
}
