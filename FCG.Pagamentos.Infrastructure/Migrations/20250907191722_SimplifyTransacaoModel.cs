using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Pagamentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTransacaoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetalhesPagamento",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "Moeda",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "ProximaTentativa",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "TentativasProcessamento",
                table: "Transacoes");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_JogoId",
                table: "Transacoes",
                column: "JogoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transacoes_JogoId",
                table: "Transacoes");

            migrationBuilder.AddColumn<string>(
                name: "DetalhesPagamento",
                table: "Transacoes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Moeda",
                table: "Transacoes",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximaTentativa",
                table: "Transacoes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TentativasProcessamento",
                table: "Transacoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
