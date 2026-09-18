using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MiniBankingSystem.Infrastructure.Data
{
    public class BankingDbContextFactory
        : IDesignTimeDbContextFactory<BankingDbContext>
    {
        public BankingDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            connectionString = connectionString?
                .Replace("host.docker.internal", "localhost");

            var optionsBuilder =
                new DbContextOptionsBuilder<BankingDbContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new BankingDbContext(optionsBuilder.Options);
        }
    }
}