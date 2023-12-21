using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class FileTimeUtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileTimeUtcDateCreated",
                table: "Eatings",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Eatings_FileTimeUtcDateCreated",
                table: "Eatings",
                column: "FileTimeUtcDateCreated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Eatings_FileTimeUtcDateCreated",
                table: "Eatings");

            migrationBuilder.DropColumn(
                name: "FileTimeUtcDateCreated",
                table: "Eatings");
        }
    }
}
