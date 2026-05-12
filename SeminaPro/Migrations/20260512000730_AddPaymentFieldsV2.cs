using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeminaPro.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFieldsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Specialites",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "TEXT", nullable: false),
                    MediaType = table.Column<int>(type: "INTEGER", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ParticipantId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeminaireId = table.Column<int>(type: "INTEGER", nullable: true),
                    SpecialiteId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFiles_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MediaFiles_Seminaires_SeminaireId",
                        column: x => x.SeminaireId,
                        principalTable: "Seminaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MediaFiles_Specialites_SpecialiteId",
                        column: x => x.SpecialiteId,
                        principalTable: "Specialites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_ParticipantId",
                table: "MediaFiles",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_SeminaireId",
                table: "MediaFiles",
                column: "SeminaireId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_SpecialiteId",
                table: "MediaFiles",
                column: "SpecialiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Specialites");
        }
    }
}
