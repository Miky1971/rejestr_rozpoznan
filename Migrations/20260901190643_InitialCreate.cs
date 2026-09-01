using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rejestr_rozpoznan.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PESEL = table.Column<string>(type: "TEXT", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalSystemKind = table.Column<int>(type: "INTEGER", nullable: false),
                    ExternalSymbolPatient = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Diagnoses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalSystemKind = table.Column<int>(type: "INTEGER", nullable: false),
                    PatientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalSymbolDiagnosis = table.Column<string>(type: "TEXT", nullable: false),
                    DateDiagnosis = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DateOnset = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    AgeOnset = table.Column<int>(type: "INTEGER", nullable: true),
                    Icd10Code = table.Column<string>(type: "TEXT", nullable: false),
                    CodingSystem = table.Column<string>(type: "TEXT", nullable: false),
                    Icd10Description = table.Column<string>(type: "TEXT", nullable: false),
                    ClinicalStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfirmationStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportStatus = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diagnoses_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_PatientId",
                table: "Diagnoses",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diagnoses");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
