using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure;
using MinhasFinancas.Infrastructure.Data;
using MinhasFinancas.Tests.Common;

namespace MinhasFinancas.Tests.IntegrationTests;

public abstract class BaseIntegrationTest(WebApplicationTestingFactory factory) : IClassFixture<WebApplicationTestingFactory>, IAsyncLifetime
{
    protected WebApplicationTestingFactory Factory => factory;
    protected HttpClient TestClient { get; private set; }

    protected IServiceProvider ScopedServices { get; private set; }

    private IServiceScope _serviceScope;
    protected MinhasFinancasDbContext DbContext {  get; private set; }

    protected Faker Faker {  get; } = new("pt_BR");

    protected TestsFixture Fixtures {  get; } = new();

    public async Task InitializeAsync()
    {
        TestClient = factory.CreateDefaultClient();
        _serviceScope = factory.Services.CreateScope();
        ScopedServices = _serviceScope.ServiceProvider;
        DbContext = ScopedServices.GetRequiredService<MinhasFinancasDbContext>();

        await InitDatabaseAsync()
            .ConfigureAwait(false);
    }

    private async Task InitDatabaseAsync()
    {
        ArgumentNullException.ThrowIfNull(DbContext);

        await DbContext.Database.EnsureDeletedAsync()
            .ConfigureAwait(false);

        await DbContext.Database.EnsureCreatedAsync()
           .ConfigureAwait(false);

        await SeedData.InitializeAsync(
              ScopedServices.GetRequiredService<IUnitOfWork>())
            .ConfigureAwait(false);
    }
    
    public async Task DisposeAsync()
    {
        TestClient.Dispose();
        await DbContext.DisposeAsync()
            .ConfigureAwait(false);

        _serviceScope.Dispose();
    }
}
