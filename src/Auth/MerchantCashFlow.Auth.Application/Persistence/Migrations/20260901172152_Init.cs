using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerchantCashFlow.Auth.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "merchant",
                columns: table => new
                {
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    document_hash = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    account_number = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    account_number_hash = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    scope = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_merchant_id", x => x.merchant_id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_merchant_account_number_hash",
                table: "merchant",
                column: "account_number_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_merchant_document_hash",
                table: "merchant",
                column: "document_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "merchant");
        }
    }
}
