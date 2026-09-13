using Microsoft.EntityFrameworkCore;
using MiniBankingSystem.Application.Interfaces;
using MiniBankingSystem.Application.Services;
using MiniBankingSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Register BankingDbContext and connect it to PostgreSQL
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Application dependencies
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<BankingDbContext>());

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();