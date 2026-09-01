using MerchantCashFlow.Auth.Application.Entities;
using MerchantCashFlow.Infrastructure.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerchantCashFlow.Auth.Application.Persistence.Configurations;

public sealed class MerchantConfiguration: IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.HasKey(e => e.MerchantId).HasName("pk_merchant_id");

        builder.ToTable("merchant");

        builder.Property(e => e.MerchantId)
            .ValueGeneratedNever()
            .HasColumnName("merchant_id");

        builder.OwnsOne(e => e.Document, document =>
        {
            document.Property(d => d.Value)
                .IsRequired()
                .HasMaxLength(ProtectedValue.ValueLength)
                .HasColumnName("document");

            document.Property(d => d.Hash)
                .IsRequired()
                .HasMaxLength(ProtectedValue.HashLength)
                .HasColumnName("document_hash");

            document.HasIndex(d => d.Hash)
                .IsUnique()
                .HasDatabaseName("ux_merchant_document_hash");
        });

        builder.Navigation(e => e.Document).IsRequired();

        builder.OwnsOne(e => e.AccountNumber, account =>
        {
            account.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(ProtectedValue.ValueLength)
                .HasColumnName("account_number");

            account.Property(a => a.Hash)
                .IsRequired()
                .HasMaxLength(ProtectedValue.HashLength)
                .HasColumnName("account_number_hash");

            account.HasIndex(a => a.Hash)
                .IsUnique()
                .HasDatabaseName("ux_merchant_account_number_hash");
        });

        builder.Navigation(e => e.AccountNumber).IsRequired();

        builder.Property(e => e.Scope)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("scope");

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");
    }
}
