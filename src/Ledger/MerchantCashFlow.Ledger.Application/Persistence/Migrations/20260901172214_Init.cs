using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerchantCashFlow.Ledger.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ledger",
                columns: table => new
                {
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    document_hash = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    account_number_hash = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(19,2)", nullable: false),
                    inserted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ledger_id", x => x.ledger_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox",
                columns: table => new
                {
                    outbox_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_id", x => x.outbox_id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_ledger_document_idempotency_key",
                table: "ledger",
                columns: new[] { "document_hash", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_pending",
                table: "outbox",
                column: "occurred_at",
                filter: "published_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ledger");

            migrationBuilder.DropTable(
                name: "outbox");
        }
    }
}
