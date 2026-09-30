using IRM.Settlements.Infrastructure.Postgres.DbContexts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace IRM.Settlements.IntegrationTests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    public const string Username = "testadmin";
    public const string Password = "testsecret";
    public const string Database = "testdb";

    private readonly PostgreSqlContainer _container;

    private NpgsqlConnection _connection = default!;
    private Respawner _respawner = default!;

    public string ConnectionString => _container.GetConnectionString() + ";Include Error Detail=true;";

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase(Database)
            .WithUsername(Username)
            .WithPassword(Password)
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<SettlementsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using (var db = new SettlementsDbContext(options))
        {
            await db.Database.MigrateAsync();
        }

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["settlements"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
