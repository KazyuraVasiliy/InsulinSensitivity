using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class ExpendableMaterial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMonitoring",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMonitoringReplacement",
                table: "Eatings",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExpendableMaterialTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpendableMaterialTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpendableMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExpendableMaterialTypeId = table.Column<int>(nullable: false),
                    Count = table.Column<decimal>(nullable: false),
                    ChangeType = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpendableMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpendableMaterials_ExpendableMaterialTypes_ExpendableMaterialTypeId",
                        column: x => x.ExpendableMaterialTypeId,
                        principalTable: "ExpendableMaterialTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpendableMaterials_ExpendableMaterialTypeId",
                table: "ExpendableMaterials",
                column: "ExpendableMaterialTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpendableMaterials");

            migrationBuilder.DropTable(
                name: "ExpendableMaterialTypes");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsMonitoring",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsMonitoringReplacement",
                table: "Eatings");
        }
    }
}
