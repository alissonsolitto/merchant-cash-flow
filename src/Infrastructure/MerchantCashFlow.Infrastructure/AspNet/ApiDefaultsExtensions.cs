using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Serilog;

namespace MerchantCashFlow.Infrastructure.AspNet;

public static class ApiDefaultsExtensions
{
    public static WebApplicationBuilder AddCashFlowApiDefaults<TContext>(this WebApplicationBuilder builder)
        where TContext : DbContext
    {
        builder.AddCashFlowApiDefaults();
        builder.Services.AddHealthChecks().AddDbContextCheck<TContext>(typeof(TContext).Name);

        return builder;
    }

    public static WebApplicationBuilder AddCashFlowApiDefaults(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(context.Configuration));

        builder.Services
            .AddGzipCompression()
            .AddCashFlowProblemDetails()
            .AddSwagger();

        builder.Services.AddHealthChecks();

        return builder;
    }

    public static WebApplication UseCashFlowApiDefaults(this WebApplication app)
    {
        app.UseForwardedHeaders();

        app.UseSerilogRequestLogging();
        app.UseResponseCompression();

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        app.MapHealthChecks("/health").AllowAnonymous();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    private static IServiceCollection AddGzipCompression(this IServiceCollection services) =>
        services
            .AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            })
            .Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);

    private static IServiceCollection AddCashFlowProblemDetails(this IServiceCollection services) =>
        services
            .AddProblemDetails()
            .AddExceptionHandler<AppExceptionHandler>();

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swagger =>
        {
            const string NameSecurityDefinition = "JWT";
            const string HeaderSecurityDefinition = "Authorization";
            const string SchemeSecurityDefinition = "bearer";

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = HeaderSecurityDefinition,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = SchemeSecurityDefinition
            };

            swagger.AddSecurityDefinition(NameSecurityDefinition, jwtSecurityScheme);

            swagger.AddSecurityRequirement(document =>
            {
                var scheme = new OpenApiSecuritySchemeReference(NameSecurityDefinition, document);
                return new OpenApiSecurityRequirement { { scheme, new List<string>() } };
            });
        });

        return services;
    }
}
