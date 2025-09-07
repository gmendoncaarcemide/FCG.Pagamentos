using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Pagamentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBrazilianPaymentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reembolsos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reembolsos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransacaoId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CodigoReembolso = table.Column<string>(type: "text", nullable: true),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TransacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValorReembolso = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reembolsos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reembolsos_Transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reembolsos_Transacoes_TransacaoId1",
                        column: x => x.TransacaoId1,
                        principalTable: "Transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reembolsos_TransacaoId",
                table: "Reembolsos",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reembolsos_TransacaoId1",
                table: "Reembolsos",
                column: "TransacaoId1");
        }
    }
}
