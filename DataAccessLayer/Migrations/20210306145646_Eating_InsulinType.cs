using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class Eating_InsulinType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BolusTypeId",
                table: "Injections",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BasalTypeId",
                table: "Eatings",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BolusTypeId",
                table: "Eatings",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Injections_BolusTypeId",
                table: "Injections",
                column: "BolusTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Eatings_BasalTypeId",
                table: "Eatings",
                column: "BasalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Eatings_BolusTypeId",
                table: "Eatings",
                column: "BolusTypeId");

            // Данный тип миграции не поддерживается SQLLite

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Eatings_InsulinTypes_BasalTypeId",
            //    table: "Eatings",
            //    column: "BasalTypeId",
            //    principalTable: "InsulinTypes",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Eatings_InsulinTypes_BolusTypeId",
            //    table: "Eatings",
            //    column: "BolusTypeId",
            //    principalTable: "InsulinTypes",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Injections_InsulinTypes_BolusTypeId",
            //    table: "Injections",
            //    column: "BolusTypeId",
            //    principalTable: "InsulinTypes",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Eatings_InsulinTypes_BasalTypeId",
                table: "Eatings");

            migrationBuilder.DropForeignKey(
                name: "FK_Eatings_InsulinTypes_BolusTypeId",
                table: "Eatings");

            migrationBuilder.DropForeignKey(
                name: "FK_Injections_InsulinTypes_BolusTypeId",
                table: "Injections");

            migrationBuilder.DropIndex(
                name: "IX_Injections_BolusTypeId",
                table: "Injections");

            migrationBuilder.DropIndex(
                name: "IX_Eatings_BasalTypeId",
                table: "Eatings");

            migrationBuilder.DropIndex(
                name: "IX_Eatings_BolusTypeId",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "BolusTypeId",
                table: "Injections");

            migrationBuilder.DropColumn(
                name: "BasalTypeId",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "BolusTypeId",
                table: "Eatings");
        }
    }
}
