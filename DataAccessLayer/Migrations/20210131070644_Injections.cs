using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class Injections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Injections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    InjectionTime = table.Column<TimeSpan>(nullable: false),
                    BolusDose = table.Column<decimal>(nullable: false),
                    EatingId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Injections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Injections_Eatings_EatingId",
                        column: x => x.EatingId,
                        principalTable: "Eatings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Injections_EatingId",
                table: "Injections",
                column: "EatingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Injections");
        }
    }
}
