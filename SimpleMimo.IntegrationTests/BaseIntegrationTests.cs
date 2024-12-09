using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleMimo.Data.Entities;
using SimpleMimo.IntegrationTests.Fixtures;

namespace SimpleMimo.IntegrationTests;

public abstract class BaseIntegrationTests
{
    protected HttpClient Client { get; }
    protected IServiceProvider Services { get; }
    
    protected BaseIntegrationTests(MimoWebApplicationFactory webApplicationFactory)
    {
        Client = webApplicationFactory.CreateClient();
        Services = webApplicationFactory.Services;
        CleanupDatabase();
    }

    private void CleanupDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MimoDbContext>();
        dbContext.UserAchievements.ExecuteDelete();
        dbContext.UserLessons.ExecuteDelete();
        dbContext.UserChapters.ExecuteDelete();
        dbContext.UserCourses.ExecuteDelete();
    }
}