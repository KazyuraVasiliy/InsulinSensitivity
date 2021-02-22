using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class EndEating_BasalInjectionTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BasalInjectionTime",
                table: "Eatings",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndEating",
                table: "Eatings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasalInjectionTime",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "EndEating",
                table: "Eatings");
        }
    }
}
