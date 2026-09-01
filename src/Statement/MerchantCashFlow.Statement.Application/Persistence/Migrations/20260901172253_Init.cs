using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerchantCashFlow.Statement.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "statement_daily",
                columns: table => new
                {
                    document_hash = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    statement_date = table.Column<DateOnly>(type: "date", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    balance = table.Column<decimal>(type: "numeric", nullable: false, computedColumnSql: "credit - debit", stored: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_statement_daily", x => new { x.document_hash, x.statement_date });
                });

            migrationBuilder.CreateTable(
                name: "statement_inbox",
                columns: table => new
                {
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_statement_inbox_ledger_id", x => x.ledger_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "statement_daily");

            migrationBuilder.DropTable(
                name: "statement_inbox");
        }
    }
}
