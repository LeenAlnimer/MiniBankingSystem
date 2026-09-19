using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MiniBankingSystem.Infrastructure.Data;

namespace MiniBankingSystem.Tests.Infrastructure;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the real PostgreSQL DbContext
            var dbContextDescriptor =
                services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<BankingDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Add InMemory database for integration tests
            services.AddDbContext<BankingDbContext>(options =>
            {
                options.UseInMemoryDatabase(
                    "MiniBankingIntegrationTestDb");
            });
        });
    }
}