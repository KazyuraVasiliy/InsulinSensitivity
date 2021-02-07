using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class Pause_Error_IntermediateDimension : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "Eatings",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Pause",
                table: "Eatings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "IntermediateDimensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DimensionTime = table.Column<TimeSpan>(nullable: false),
                    DimensionDate = table.Column<DateTime>(nullable: false),
                    Glucose = table.Column<decimal>(nullable: false),
                    EatingId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntermediateDimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntermediateDimensions_Eatings_EatingId",
                        column: x => x.EatingId,
                        principalTable: "Eatings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntermediateDimensions_EatingId",
                table: "IntermediateDimensions",
                column: "EatingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntermediateDimensions");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "Pause",
                table: "Eatings");
        }
    }
}
