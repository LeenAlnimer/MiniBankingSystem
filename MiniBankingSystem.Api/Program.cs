using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Api.Middleware;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Application.Services;
using MiniBankingSystem.Infrastructure.BackgroundJobs;
using MiniBankingSystem.Infrastructure.Caching;
using MiniBankingSystem.Infrastructure.Data;
using MiniBankingSystem.Infrastructure.Messaging;
using MiniBankingSystem.Infrastructure.ExternalServices;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Register BankingDbContext and connect it to PostgreSQL
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));

// Register Redis Cache Service
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Register RabbitMQ Producer
builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();

// Register RabbitMQ Consumer
builder.Services.AddHostedService<RabbitMqConsumer>();

// Register Application dependencies
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<BankingDbContext>());

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// Register External Exchange Rate Service
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

// Register Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Hangfire Server
builder.Services.AddHangfireServer();

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// Register Recurring Job
RecurringJob.AddOrUpdate<TransactionSummaryJob>(
    "daily-transaction-summary",
    job => job.RunAsync(),
    Cron.Daily);

app.MapControllers();

app.Run();