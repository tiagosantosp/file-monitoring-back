using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FileMonitoring.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arquivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeArquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CaminhoBackup = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HashMD5 = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    TipoAdquirente = table.Column<int>(type: "integer", nullable: false),
                    MensagemErro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arquivos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransacoesArquivo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArquivoId = table.Column<int>(type: "integer", nullable: false),
                    TipoRegistro = table.Column<int>(type: "integer", nullable: false),
                    Estabelecimento = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodoInicial = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodoFinal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sequencia = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Empresa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransacoesArquivo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransacoesArquivo_Arquivos_ArquivoId",
                        column: x => x.ArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Arquivos_HashMD5",
                table: "Arquivos",
                column: "HashMD5",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesArquivo_ArquivoId",
                table: "TransacoesArquivo",
                column: "ArquivoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransacoesArquivo_Estabelecimento",
                table: "TransacoesArquivo",
                column: "Estabelecimento");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransacoesArquivo");

            migrationBuilder.DropTable(
                name: "Arquivos");
        }
    }
}
