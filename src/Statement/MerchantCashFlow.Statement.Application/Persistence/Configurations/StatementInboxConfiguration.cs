using MerchantCashFlow.Statement.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerchantCashFlow.Statement.Application.Persistence.Configurations;

public sealed class StatementInboxConfiguration: IEntityTypeConfiguration<StatementInbox>
{
    public void Configure(EntityTypeBuilder<StatementInbox> builder)
    {
        builder.HasKey(e => e.LedgerId).HasName("pk_statement_inbox_ledger_id");

        builder.ToTable("statement_inbox");

        builder.Property(e => e.LedgerId)
            .ValueGeneratedNever()
            .HasColumnName("ledger_id");

        builder.Property(e => e.ProcessedAt)
            .IsRequired()
            .HasColumnName("processed_at");
    }
}
