

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Tests.IntegrationTests;

public class WebApplicationTestingFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public WebApplicationTestingFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            ServiceDescriptor? descriptor = services.SingleOrDefault(
                s => s.ServiceType.Equals(typeof(DbContextOptions<MinhasFinancasDbContext>))
            );

            if (descriptor is not null) 
                services.Remove(descriptor);

            services.AddDbContext<MinhasFinancasDbContext>(
                opt => opt.UseSqlite(_connection));
        });
    }

    public override ValueTask DisposeAsync()
    {
        _connection.Dispose();
        return base.DisposeAsync();
    }
}
