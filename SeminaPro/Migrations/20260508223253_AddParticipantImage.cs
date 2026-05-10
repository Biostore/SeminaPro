using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeminaPro.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Participants",
                type: "TEXT",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Participants");
        }
    }
}
