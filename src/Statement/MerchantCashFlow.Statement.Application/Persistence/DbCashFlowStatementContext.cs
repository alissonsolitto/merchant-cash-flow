using System.Reflection;
using MerchantCashFlow.Statement.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace MerchantCashFlow.Statement.Application.Persistence;

public partial class DbCashFlowStatementContext: DbContext
{
    public DbCashFlowStatementContext(DbContextOptions<DbCashFlowStatementContext> options) : base(options)
    {
    }

    public virtual DbSet<DailyStatement> Daily { get; set; } = null!;

    public virtual DbSet<StatementInbox> Inbox { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(), x => x.Namespace?.Contains("Configurations") == true);

        base.OnModelCreating(modelBuilder);
    }
}
