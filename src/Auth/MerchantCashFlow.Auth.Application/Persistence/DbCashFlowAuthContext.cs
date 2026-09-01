using System.Reflection;
using MerchantCashFlow.Auth.Application.Entities;
using MerchantCashFlow.Infrastructure.DataProtection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace MerchantCashFlow.Auth.Application.Persistence;

public partial class DbCashFlowAuthContext: DbContext
{
    private readonly IDataProtectionProvider _dataProtection;

    public DbCashFlowAuthContext(DbContextOptions<DbCashFlowAuthContext> options, IDataProtectionProvider dataProtection) : base(options)
    {
        this._dataProtection = dataProtection;
    }

    public virtual DbSet<Merchant> Merchant { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica as configurações de entidade usando a convenção de namespace "Configurations"
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(), x => x.Namespace?.Contains("Configurations") == true);

        var secretConverter = new PiiValueConverter(this._dataProtection);
        var hashConverter = new PiiHashConverter();

        // Percorre o modelo já para encontrar Secret e PiiHash e aplicar os conversores.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(PiiValue))
                {
                    property.SetValueConverter(secretConverter);
                }
                else if (property.ClrType == typeof(PiiHash))
                {
                    property.SetValueConverter(hashConverter);
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
