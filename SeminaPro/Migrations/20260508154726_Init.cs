using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeminaPro.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specialites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Libelle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    NumeroTelephone = table.Column<string>(type: "TEXT", nullable: true),
                    SpecialiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipantType = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    Fonction = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NomEntreprise = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Niveau = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NomUniversite = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_Specialites_SpecialiteId",
                        column: x => x.SpecialiteId,
                        principalTable: "Specialites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Seminaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Titre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Lieu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Tarif = table.Column<decimal>(type: "TEXT", nullable: false),
                    DateSeminaire = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NombreMaximal = table.Column<int>(type: "INTEGER", nullable: false),
                    SpecialiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seminaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seminaires_Specialites_SpecialiteId",
                        column: x => x.SpecialiteId,
                        principalTable: "Specialites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateInscription = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AffichageConfirmation = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParticipantId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeminaireId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscriptions_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscriptions_Seminaires_SeminaireId",
                        column: x => x.SeminaireId,
                        principalTable: "Seminaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_ParticipantId",
                table: "Inscriptions",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_SeminaireId",
                table: "Inscriptions",
                column: "SeminaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_Email",
                table: "Participants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_SpecialiteId",
                table: "Participants",
                column: "SpecialiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Seminaires_Code",
                table: "Seminaires",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seminaires_SpecialiteId",
                table: "Seminaires",
                column: "SpecialiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialites_Libelle",
                table: "Specialites",
                column: "Libelle",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inscriptions");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Seminaires");

            migrationBuilder.DropTable(
                name: "Specialites");
        }
    }
}
