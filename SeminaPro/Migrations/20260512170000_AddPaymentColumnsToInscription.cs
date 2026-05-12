using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeminaPro.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentColumnsToInscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodId",
                table: "Inscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Inscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: "En Attente");

            migrationBuilder.AddColumn<DateTime>(
                name: "DatePaiement",
                table: "Inscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantPaye",
                table: "Inscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Inscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FactureNumero",
                table: "Inscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFacture",
                table: "Inscriptions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "Inscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Inscriptions");

            migrationBuilder.DropColumn(
                name: "DatePaiement",
                table: "Inscriptions");

            migrationBuilder.DropColumn(
                name: "MontantPaye",
                table: "Inscriptions");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Inscriptions");

            migrationBuilder.DropColumn(
                name: "FactureNumero",
                table: "Inscriptions");

            migrationBuilder.DropColumn(
                name: "DateFacture",
                table: "Inscriptions");
        }
    }
}
