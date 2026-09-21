using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using FashionWeb.Api.Authorization;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Services;
using FashionWeb.Business.Strategies;
using FashionWeb.Data;
using FashionWeb.Data.Context;
using FashionWeb.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FashionWeb.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Clock / Time Abstraction
        services.AddSingleton(TimeProvider.System);

        // Dynamic Fee Engine & Strategy Pattern
        services.AddTransient<IPlatformFeeStrategy, TikTokShopFeeStrategy>();
        services.AddTransient<IPlatformFeeStrategy, ShopeeFeeStrategy>();
        services.AddTransient<IPlatformFeeStrategy, PosFeeStrategy>();
        services.AddSingleton<FeeStrategyFactory>();
        services.AddScoped<IDynamicFeeEngine, DynamicFeeEngine>();

        // Domain & Application Services
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IFeeScheduleService, FeeScheduleService>();
        services.AddScoped<ISettlementService, SettlementService>();
        services.AddScoped<IDiscrepancyService, DiscrepancyService>();
        services.AddScoped<IAnalyticsCsvExporter, AnalyticsCsvExporter>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        return services;
    }

    public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Critical configuration error: ConnectionString 'DefaultConnection' is missing or empty. Please set it in appsettings.json or environment variables.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Data Repositories & Unit of Work
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IFeeScheduleRepository, FeeScheduleRepository>();
        services.AddScoped<IReconciliationRepository, ReconciliationRepository>();
        services.AddScoped<IDiscrepancyRepository, DiscrepancyRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
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

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        var jwtSecret = configuration["Jwt:SecretKey"] ?? "VSF_Default_Development_Super_Secret_Key_2026_Minimum_32_Bytes!";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "FashionWeb.Api";
        var jwtAudience = configuration["Jwt:Audience"] ?? "FashionWeb.Client";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.RequireSalesOps, policy =>
                policy.RequireRole(Roles.SalesOps, Roles.ShopOwner));
            options.AddPolicy(Policies.RequireFinanceManager, policy =>
                policy.RequireRole(Roles.FinanceManager, Roles.ShopOwner));
            options.AddPolicy(Policies.RequireShopOwner, policy =>
                policy.RequireRole(Roles.ShopOwner));
        });

        return services;
    }
}
