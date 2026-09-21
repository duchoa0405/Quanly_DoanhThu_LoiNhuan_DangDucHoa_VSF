using System.Text.Json.Serialization;
using FashionWeb.Api.Authorization;
using FashionWeb.Api.Middleware;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Services;
using FashionWeb.Business.Strategies;
using FashionWeb.Data;
using FashionWeb.Data.Context;
using FashionWeb.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers & JSON String Enum serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fashion Revenue & Profit Management API",
        Version = "v1",
        Description = "ASP.NET Core 8 Web API implementing multi-channel revenue recognition, fee calculation engine, settlement reconciliation, discrepancy audit, and executive analytics."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 2. CORS Policy for Frontend (Vite 5173 / React 3000)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 3. Database Context (PostgreSQL via Npgsql)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=fashionweb_db;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 4. Role-Based Access Control Policies
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireSalesOps, policy =>
        policy.RequireRole(Roles.SalesOps, Roles.ShopOwner));
    options.AddPolicy(Policies.RequireFinanceManager, policy =>
        policy.RequireRole(Roles.FinanceManager, Roles.ShopOwner));
    options.AddPolicy(Policies.RequireShopOwner, policy =>
        policy.RequireRole(Roles.ShopOwner));
});

// 5. Dependency Injection - Strategy Pattern & Dynamic Fee Engine
builder.Services.AddTransient<IPlatformFeeStrategy, TikTokShopFeeStrategy>();
builder.Services.AddTransient<IPlatformFeeStrategy, ShopeeFeeStrategy>();
builder.Services.AddTransient<IPlatformFeeStrategy, PosFeeStrategy>();
builder.Services.AddSingleton<FeeStrategyFactory>();
builder.Services.AddScoped<IDynamicFeeEngine, DynamicFeeEngine>();

// 6. Dependency Injection - Data Repositories & Unit of Work
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IFeeScheduleRepository, FeeScheduleRepository>();
builder.Services.AddScoped<IReconciliationRepository, ReconciliationRepository>();
builder.Services.AddScoped<IDiscrepancyRepository, DiscrepancyRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 7. Dependency Injection - Business Services
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFeeScheduleService, FeeScheduleService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IDiscrepancyService, DiscrepancyService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

var app = builder.Build();

// 8. Global RFC 7807 Exception Handling Middleware (First in pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 9. Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fashion Revenue & Profit API v1");
    });
}

// 10. HTTP Pipeline
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 11. Optional Database Seeding (applied safely if DB is accessible in development)
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.CanConnect())
        {
            await DataSeeder.SeedAsync(db);
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not seed database during startup: {Message}", ex.Message);
    }
}

app.Run();
