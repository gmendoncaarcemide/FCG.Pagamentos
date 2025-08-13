using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Pagamentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JogoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moeda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TipoPagamento = table.Column<int>(type: "int", nullable: false),
                    CodigoAutorizacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoTransacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataProcessamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataConfirmacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErroProcessamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TentativasProcessamento = table.Column<int>(type: "int", nullable: false),
                    ProximaTentativa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DetalhesPagamento = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reembolsos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValorReembolso = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoReembolso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransacaoId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_Referencia",
                table: "Transacoes",
                column: "Referencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_Status",
                table: "Transacoes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_UsuarioId",
                table: "Transacoes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reembolsos");

            migrationBuilder.DropTable(
                name: "Transacoes");
        }
    }
}
