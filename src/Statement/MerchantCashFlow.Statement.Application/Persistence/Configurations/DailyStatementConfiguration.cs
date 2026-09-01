using MerchantCashFlow.Statement.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerchantCashFlow.Statement.Application.Persistence.Configurations;

public sealed class DailyStatementConfiguration: IEntityTypeConfiguration<DailyStatement>
{
    public void Configure(EntityTypeBuilder<DailyStatement> builder)
    {
        builder.HasKey(e => new { e.DocumentHash, e.AccountNumberHash, e.StatementDate }).HasName("pk_statement_daily");

        builder.ToTable("statement_daily");

        builder.Property(e => e.DocumentHash)
            .IsRequired()
            .HasMaxLength(44)
            .HasColumnName("document_hash");

        builder.Property(e => e.AccountNumberHash)
            .IsRequired()
            .HasMaxLength(44)
            .HasColumnName("account_number_hash");

        builder.Property(e => e.StatementDate)
            .IsRequired()
            .HasColumnName("statement_date");

        builder.Property(e => e.Credit)
            .IsRequired()
            .HasColumnType("numeric(19,2)")
            .HasColumnName("credit");

        builder.Property(e => e.Debit)
            .IsRequired()
            .HasColumnType("numeric(19,2)")
            .HasColumnName("debit");

        builder.Property(e => e.Balance)
            .HasComputedColumnSql("credit - debit", stored: true)
            .ValueGeneratedOnAddOrUpdate()
            .HasColumnName("balance");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");
    }
}
