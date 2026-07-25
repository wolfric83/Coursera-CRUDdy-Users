using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseInMemoryDatabase("UserManagementDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Middleware order matters because requests move through these components in this order,
// then responses travel back out through them in reverse order.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TokenAuthenticationMiddleware>();

// Authentication can short-circuit the pipeline before logging runs, so missing or
// invalid-token requests may not be logged by RequestResponseLoggingMiddleware.
app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Development-only endpoint for testing the global exception-handling middleware.
    app.MapGet("/api/test/error", static IResult () =>
        throw new InvalidOperationException("Test-only exception for middleware validation."));
}

app.MapControllers();

app.Run();
