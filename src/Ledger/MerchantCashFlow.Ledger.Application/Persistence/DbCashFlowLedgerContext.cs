using System.Reflection;
using MerchantCashFlow.Ledger.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace MerchantCashFlow.Ledger.Application.Persistence;

public partial class DbCashFlowLedgerContext: DbContext
{
    public DbCashFlowLedgerContext(DbContextOptions<DbCashFlowLedgerContext> options) : base(options)
    {
    }

    public virtual DbSet<LedgerEntry> Ledger { get; set; } = null!;

    public virtual DbSet<OutboxMessage> Outbox { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(), x => x.Namespace?.Contains("Configurations") == true);

        base.OnModelCreating(modelBuilder);
    }
}
