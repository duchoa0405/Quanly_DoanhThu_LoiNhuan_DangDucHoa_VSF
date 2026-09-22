using FashionWeb.Api.Extensions;
using FashionWeb.Api.Middleware;
using FashionWeb.Data;
using FashionWeb.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// Modular DI Registrations
builder.Services.AddApiInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddBusinessServices();

var app = builder.Build();

// RFC 7807 Exception Handling Middleware (First in pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger UI in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fashion Revenue & Profit API v1");
    });
}

// HTTP Pipeline
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Optional Database Seeding in Development
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
