using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeminaPro.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedPaymentMethod",
                table: "Inscriptions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedPaymentMethod",
                table: "Inscriptions");
        }
    }
}
