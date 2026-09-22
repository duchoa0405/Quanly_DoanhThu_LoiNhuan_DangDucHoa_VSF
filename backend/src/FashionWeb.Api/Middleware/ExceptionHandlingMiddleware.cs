using System.Net;
using System.Text.Json;
using FashionWeb.Business.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on request {Method} {Path} [TraceId: {TraceId}]: {Message}", 
                context.Request.Method, context.Request.Path, context.TraceIdentifier, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException ve => (HttpStatusCode.UnprocessableEntity, "Unprocessable Entity", ve.Message),
            BusinessRuleException bre => (HttpStatusCode.UnprocessableEntity, "Business Rule Violation", bre.Message),
            NotFoundException nfe => (HttpStatusCode.NotFound, "Resource Not Found", nfe.Message),
            ConflictException ce => (HttpStatusCode.Conflict, "Conflict Violation", ce.Message),
            KeyNotFoundException knf => (HttpStatusCode.NotFound, "Resource Not Found", knf.Message),
            ArgumentNullException ane => (HttpStatusCode.BadRequest, "Bad Request", ane.Message),
            ArgumentException ae => (HttpStatusCode.BadRequest, "Bad Request", ae.Message),
            InvalidOperationException ioe => (HttpStatusCode.Conflict, "Conflict", ioe.Message),
            UnauthorizedAccessException uae => (HttpStatusCode.Forbidden, "Access Forbidden", uae.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }
}
