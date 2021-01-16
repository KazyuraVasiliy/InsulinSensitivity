using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class MenstrualCycle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Данный тип миграции не поддерживается SQLLite

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Eatings_InsulinTypes_InsulinTypeId",
            //    table: "Eatings");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Eatings_InsulinTypes_InsulinTypeId1",
            //    table: "Eatings");

            //migrationBuilder.DropIndex(
            //    name: "IX_Eatings_InsulinTypeId",
            //    table: "Eatings");

            //migrationBuilder.DropIndex(
            //    name: "IX_Eatings_InsulinTypeId1",
            //    table: "Eatings");

            //migrationBuilder.DropColumn(
            //    name: "InsulinTypeId",
            //    table: "Eatings");

            //migrationBuilder.DropColumn(
            //    name: "InsulinTypeId1",
            //    table: "Eatings");

            migrationBuilder.CreateTable(
                name: "MenstrualCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateStart = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenstrualCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenstrualCycles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenstrualCycles_UserId",
                table: "MenstrualCycles",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenstrualCycles");

            // Данный тип миграции не поддерживается SQLLite

            //migrationBuilder.AddColumn<Guid>(
            //    name: "InsulinTypeId",
            //    table: "Eatings",
            //    type: "TEXT",
            //    nullable: true);

            //migrationBuilder.AddColumn<Guid>(
            //    name: "InsulinTypeId1",
            //    table: "Eatings",
            //    type: "TEXT",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Eatings_InsulinTypeId",
            //    table: "Eatings",
            //    column: "InsulinTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Eatings_InsulinTypeId1",
            //    table: "Eatings",
            //    column: "InsulinTypeId1");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Eatings_InsulinTypes_InsulinTypeId",
            //    table: "Eatings",
            //    column: "InsulinTypeId",
            //    principalTable: "InsulinTypes",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Eatings_InsulinTypes_InsulinTypeId1",
            //    table: "Eatings",
            //    column: "InsulinTypeId1",
            //    principalTable: "InsulinTypes",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }
    }
}
