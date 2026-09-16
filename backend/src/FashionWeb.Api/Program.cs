using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Services;
using FashionWeb.Business.Strategies;
using FashionWeb.Data.Context;
using FashionWeb.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Frontend React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Database Context (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=fashionweb_db;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Dependency Injection - Strategies
builder.Services.AddTransient<TikTokShopFeeStrategy>();
builder.Services.AddTransient<ShopeeFeeStrategy>();
builder.Services.AddTransient<POSFeeStrategy>();
builder.Services.AddSingleton<FeeStrategyFactory>();

// Dependency Injection - Business Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDynamicFeeEngine, DynamicFeeEngine>();
builder.Services.AddScoped<ISettlementService, StatementMatchingService>();
builder.Services.AddScoped<IDiscrepancyService, DiscrepancyService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Dependency Injection - Data Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReconciliationRepository, ReconciliationRepository>();
builder.Services.AddScoped<IFeeScheduleRepository, FeeScheduleRepository>();
builder.Services.AddScoped<IDiscrepancyRepository, DiscrepancyRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
