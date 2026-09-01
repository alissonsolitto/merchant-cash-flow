using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace MerchantCashFlow.Infrastructure.DataProtection;

public static class DataProtectionExtensions
{
    public static IServiceCollection AddCashFlowDataProtection(this IServiceCollection services, string keyRingPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyRingPath);

        services.AddDataProtection()
            .SetApplicationName("MerchantCashFlow")
            .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));

        return services;
    }
}
