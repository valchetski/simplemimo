using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleMimo.Data.Entities;

namespace SimpleMimo.IntegrationTests.Fixtures;

public class MimoWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();
        RemoveDatabase(host.Services);
        host.Start();
        return host;
    }

    private static void RemoveDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();
        dbContext.Database.EnsureDeleted();
    }
}