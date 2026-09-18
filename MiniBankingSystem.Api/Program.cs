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


// Database Connection

var databaseConnectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

// When running from Visual Studio,
// use localhost instead of the Docker hostname.
if (builder.Environment.IsDevelopment())
{
    databaseConnectionString =
        databaseConnectionString?
            .Replace("host.docker.internal", "localhost");
}


// JWT Authentication

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


// HttpClient

builder.Services.AddHttpClient();



// Entity Framework Core / PostgreSQL

builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseNpgsql(databaseConnectionString));


// Password Hashing

builder.Services.AddScoped<
    IPasswordHasher,
    PasswordHasher>();


// JWT Token Service

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();


// Redis

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


// Redis Cache Service

builder.Services.AddScoped<
    ICacheService,
    RedisCacheService>();


// RabbitMQ Producer

builder.Services.AddScoped<
    IRabbitMqService,
    RabbitMqService>();


// RabbitMQ Consumer

builder.Services.AddHostedService<
    RabbitMqConsumer>();


// Application DbContext

builder.Services.AddScoped<IApplicationDbContext>(
    provider =>
        provider.GetRequiredService<
            BankingDbContext>());


// Application Services

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


// External Exchange Rate Service

builder.Services.AddScoped<
    IExchangeRateService,
    ExchangeRateService>();


// Hangfire


builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(
        databaseConnectionString));

builder.Services.AddHangfireServer();



// Controllers

builder.Services.AddControllers();



// Swagger / OpenAPI

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // JWT Bearer definition
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter your JWT token."
        });


    // Apply JWT security globally in Swagger
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


// Build Application

var app = builder.Build();



// Swagger


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// HTTPS


app.UseHttpsRedirection();



// Global Exception Middleware


app.UseMiddleware<
    GlobalExceptionMiddleware>();



// Authentication


app.UseAuthentication();



// Authorization


app.UseAuthorization();



// Hangfire Dashboard


app.UseHangfireDashboard("/hangfire");



// Recurring Job


RecurringJob.AddOrUpdate<TransactionSummaryJob>(
    "daily-transaction-summary",
    job => job.RunAsync(),
    Cron.Daily);



// Map Controllers


app.MapControllers();



// Run Application


app.Run();