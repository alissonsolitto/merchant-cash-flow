using MerchantCashFlow.Ledger.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerchantCashFlow.Ledger.Application.Persistence.Configurations;

public sealed class OutboxMessageConfiguration: IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(e => e.OutboxId).HasName("pk_outbox_id");

        builder.ToTable("outbox");

        builder.Property(e => e.OutboxId)
            .ValueGeneratedNever()
            .HasColumnName("outbox_id");

        builder.Property(e => e.LedgerId)
            .IsRequired()
            .HasColumnName("ledger_id");

        builder.Property(e => e.Payload)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasColumnName("payload");

        builder.Property(e => e.OccurredAt)
            .IsRequired()
            .HasColumnName("occurred_at");

        builder.Property(e => e.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(e => e.Attempts)
            .IsRequired()
            .HasColumnName("attempts");

        builder.Property(e => e.LastError)
            .HasMaxLength(1000)
            .HasColumnName("last_error");

        builder.HasIndex(e => e.OccurredAt)
            .HasFilter("published_at IS NULL")
            .HasDatabaseName("ix_outbox_pending");
    }
}
