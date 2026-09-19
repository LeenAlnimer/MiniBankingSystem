using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiniBankingSystem.Api.Middleware;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Application.Services;
using MiniBankingSystem.Infrastructure.BackgroundJobs;
using MiniBankingSystem.Infrastructure.Caching;
using MiniBankingSystem.Infrastructure.Data;
using MiniBankingSystem.Infrastructure.ExternalServices;
using MiniBankingSystem.Infrastructure.Messaging;
using MiniBankingSystem.Infrastructure.Security;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var databaseConnectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    databaseConnectionString =
        databaseConnectionString?
            .Replace("host.docker.internal", "localhost");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!))
            };
    });

builder.Services.AddHttpClient();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<BankingDbContext>(options =>
        options.UseNpgsql(databaseConnectionString));
}

builder.Services.AddScoped<
    IPasswordHasher,
    PasswordHasher>();

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    var redisConnectionString =
        builder.Configuration["Redis:ConnectionString"]
        ?? "localhost:6379";

    if (builder.Environment.IsDevelopment())
    {
        redisConnectionString =
            redisConnectionString.Replace(
                "minibanking-redis",
                "localhost");
    }

    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(
            redisConnectionString));

    builder.Services.AddScoped<
        ICacheService,
        RedisCacheService>();
}

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddScoped<
        IRabbitMqService,
        RabbitMqService>();

    builder.Services.AddHostedService<
        RabbitMqConsumer>();
}

builder.Services.AddScoped<IApplicationDbContext>(
    provider =>
        provider.GetRequiredService<
            BankingDbContext>());

builder.Services.AddScoped<
    ICustomerService,
    CustomerService>();

builder.Services.AddScoped<
    IAccountService,
    AccountService>();

builder.Services.AddScoped<
    ITransactionService,
    TransactionService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IExchangeRateService,
    ExchangeRateService>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHangfire(config =>
        config.UsePostgreSqlStorage(
            databaseConnectionString));

    builder.Services.AddHangfireServer();
}

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<
    GlobalExceptionMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHangfireDashboard("/hangfire");

    RecurringJob.AddOrUpdate<TransactionSummaryJob>(
        "daily-transaction-summary",
        job => job.RunAsync(),
        Cron.Daily);
}

app.MapControllers();

app.Run();

public partial class Program
{
}