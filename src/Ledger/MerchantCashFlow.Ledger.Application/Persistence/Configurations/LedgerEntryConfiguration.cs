using MerchantCashFlow.Ledger.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerchantCashFlow.Ledger.Application.Persistence.Configurations;

public sealed class LedgerEntryConfiguration: IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.HasKey(e => e.LedgerId).HasName("pk_ledger_id");

        builder.ToTable("ledger");

        builder.Property(e => e.LedgerId)
            .ValueGeneratedNever()
            .HasColumnName("ledger_id");

        builder.Property(e => e.DocumentHash)
            .IsRequired()
            .HasMaxLength(44)
            .HasColumnName("document_hash");

        builder.Property(e => e.AccountNumberHash)
            .IsRequired()
            .HasMaxLength(44)
            .HasColumnName("account_number_hash");

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<short>()
            .HasColumnName("type");

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasColumnType("numeric(19,2)")
            .HasColumnName("amount");

        builder.Property(e => e.InsertedAt)
            .IsRequired()
            .HasColumnName("inserted_at");

        builder.Property(e => e.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("idempotency_key");

        builder.HasIndex(e => new { e.DocumentHash, e.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_ledger_document_idempotency_key");
    }
}
