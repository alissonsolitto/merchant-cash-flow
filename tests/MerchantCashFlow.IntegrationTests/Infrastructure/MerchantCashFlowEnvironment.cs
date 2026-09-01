using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace MerchantCashFlow.IntegrationTests.Infrastructure;

public sealed class MerchantCashFlowEnvironment: IAsyncLifetime
{
    public const string SigningKey = "integration-tests-signing-key-with-enough-entropy-0123456789";

    public const string FullDocument = "11111111000191";
    public const string FullAccount = "0001-1";

    public const string LedgerOnlyDocument = "22222222000172";
    public const string LedgerOnlyAccount = "0002-2";

    public const string StatementOnlyDocument = "33333333000153";
    public const string StatementOnlyAccount = "0003-3";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("auth")
        .WithUsername("cashflow")
        .WithPassword("cashflow")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:4-alpine")
        .Build();

    public string AuthConnectionString => this._postgres.GetConnectionString();

    public string LedgerConnectionString => this.ConnectionStringFor("ledger");

    public string StatementConnectionString => this.ConnectionStringFor("statement");

    public string BrokerConnectionString => this._rabbitMq.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(this._postgres.StartAsync(), this._rabbitMq.StartAsync());
        await this._postgres.ExecScriptAsync("CREATE DATABASE ledger; CREATE DATABASE statement;");
    }

    public async Task DisposeAsync()
    {
        await this._postgres.DisposeAsync();
        await this._rabbitMq.DisposeAsync();
    }

    private string ConnectionStringFor(string database) =>
        this._postgres.GetConnectionString().Replace("Database=auth", $"Database={database}", StringComparison.OrdinalIgnoreCase);
}

[CollectionDefinition(Name)]
public sealed class MerchantCashFlowCollection: ICollectionFixture<MerchantCashFlowEnvironment>
{
    public const string Name = "merchantcashflow";
}
