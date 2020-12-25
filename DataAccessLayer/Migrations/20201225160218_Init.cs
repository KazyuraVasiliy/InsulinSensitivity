using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EatingTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    TimeStart = table.Column<TimeSpan>(nullable: false),
                    TimeEnd = table.Column<TimeSpan>(nullable: false),
                    IsBasal = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EatingTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsDefault = table.Column<bool>(nullable: false),
                    IsEmpty = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsulinTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsBasal = table.Column<bool>(nullable: false),
                    Duration = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsulinTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExerciseTypeId = table.Column<Guid>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    HoursAfterInjection = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_ExerciseTypes_ExerciseTypeId",
                        column: x => x.ExerciseTypeId,
                        principalTable: "ExerciseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Gender = table.Column<bool>(nullable: false),
                    BirthDate = table.Column<DateTime>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    Weight = table.Column<int>(nullable: false),
                    Hyperglycemia = table.Column<decimal>(nullable: false),
                    HighSugar = table.Column<decimal>(nullable: false),
                    TargetGlucose = table.Column<decimal>(nullable: false),
                    LowSugar = table.Column<decimal>(nullable: false),
                    Hypoglycemia = table.Column<decimal>(nullable: false),
                    IsPump = table.Column<bool>(nullable: false),
                    DosingAccuracy = table.Column<decimal>(nullable: false),
                    CarbohydrateCoefficient = table.Column<decimal>(nullable: false),
                    ProteinCoefficient = table.Column<decimal>(nullable: false),
                    FatCoefficient = table.Column<decimal>(nullable: false),
                    BasalTypeId = table.Column<Guid>(nullable: false),
                    BolusTypeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_InsulinTypes_BasalTypeId",
                        column: x => x.BasalTypeId,
                        principalTable: "InsulinTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_InsulinTypes_BolusTypeId",
                        column: x => x.BolusTypeId,
                        principalTable: "InsulinTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Eatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    InjectionTime = table.Column<TimeSpan>(nullable: false),
                    GlucoseStart = table.Column<decimal>(nullable: false),
                    GlucoseEnd = table.Column<decimal>(nullable: true),
                    ActiveInsulinStart = table.Column<decimal>(nullable: false),
                    ActiveInsulinEnd = table.Column<decimal>(nullable: false),
                    Carbohydrate = table.Column<int>(nullable: false),
                    Protein = table.Column<int>(nullable: false),
                    Fat = table.Column<int>(nullable: false),
                    BasalDose = table.Column<decimal>(nullable: false),
                    BolusDoseCalculate = table.Column<decimal>(nullable: true),
                    BolusDoseFact = table.Column<decimal>(nullable: false),
                    InsulinSensitivityAutoOne = table.Column<decimal>(nullable: true),
                    InsulinSensitivityAutoTwo = table.Column<decimal>(nullable: true),
                    InsulinSensitivityUser = table.Column<decimal>(nullable: true),
                    InsulinSensitivityFact = table.Column<decimal>(nullable: true),
                    WriteOff = table.Column<decimal>(nullable: false),
                    IsMenstrualCycleStart = table.Column<bool>(nullable: false),
                    AccuracyAuto = table.Column<int>(nullable: true),
                    AccuracyUser = table.Column<int>(nullable: true),
                    EatingTypeId = table.Column<Guid>(nullable: false),
                    ExerciseId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    InsulinTypeId = table.Column<Guid>(nullable: true),
                    InsulinTypeId1 = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Eatings_EatingTypes_EatingTypeId",
                        column: x => x.EatingTypeId,
                        principalTable: "EatingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Eatings_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Eatings_InsulinTypes_InsulinTypeId",
                        column: x => x.InsulinTypeId,
                        principalTable: "InsulinTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Eatings_InsulinTypes_InsulinTypeId1",
                        column: x => x.InsulinTypeId1,
                        principalTable: "InsulinTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Eatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Eatings_EatingTypeId",
                table: "Eatings",
                column: "EatingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Eatings_ExerciseId",
                table: "Eatings",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Eatings_InsulinTypeId",
                table: "Eatings",
                column: "InsulinTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Eatings_InsulinTypeId1",
                table: "Eatings",
                column: "InsulinTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_Eatings_UserId",
                table: "Eatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ExerciseTypeId",
                table: "Exercises",
                column: "ExerciseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BasalTypeId",
                table: "Users",
                column: "BasalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BolusTypeId",
                table: "Users",
                column: "BolusTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Eatings");

            migrationBuilder.DropTable(
                name: "EatingTypes");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ExerciseTypes");

            migrationBuilder.DropTable(
                name: "InsulinTypes");
        }
    }
}
