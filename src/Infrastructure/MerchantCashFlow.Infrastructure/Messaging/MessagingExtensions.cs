using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MerchantCashFlow.Infrastructure.Messaging;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";
    public int RetryCount { get; set; } = 3;
    public int RetryIntervalSeconds { get; set; } = 5;
}

public static class MessagingExtensions
{
    public static IServiceCollection AddCashFlowMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>() ?? new MessagingOptions();

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.ConfigureHealthCheckOptions(options => options.MinimalFailureStatus = HealthStatus.Degraded);

            configure?.Invoke(bus);

            bus.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(connectionString));

                // Falha transitória é reprocessada em memória; esgotadas as tentativas, o MassTransit
                // move a mensagem para <fila>_error, que é a fila morta do consumidor.
                configurator.UseMessageRetry(retry => retry.Interval(messagingOptions.RetryCount, TimeSpan.FromSeconds(messagingOptions.RetryIntervalSeconds)));

                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
