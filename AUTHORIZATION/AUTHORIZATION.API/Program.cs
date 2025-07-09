using AUTHORIZATION.API.DependencyInjection.Extensions;
using AUTHORIZATION.API.Middlewares;
using AUTHORIZATION.APPLICATION.DependencyInjection.Extensions;
using AUTHORIZATION.INFRASTRUCTURE.DependencyInjection.Extensions;
using Carter;
using COMMAND.PERSISTENCE.DependencyInjection.Extensions;
using CONTRACT.CONTRACT.API.DependencyInjection.Extensions;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.PERSISTENCE.DependencyInjection.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Serilog.Events;
using ServiceDefaults;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Logging
    .ClearProviders()
    .AddSerilog();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.File("logs/logs.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console());

// Add Carter module with assembly scanning for presentation layer
builder.Services.AddCarter();

// Optimize: Add response compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

builder.Services
    .AddSwaggerGenNewtonsoftSupport()
    .AddFluentValidationRulesToSwagger()
    .AddEndpointsApiExplorer()
    .AddSwaggerAPI1();

builder.Services
    .AddApiVersioning(options => options.ReportApiVersions = true)
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.ConfigureCors();

// API Layer
builder.Services.AddJwtAuthenticationAPI1(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddHttpContextAccessor();

// Application Layer
builder.Services.AddMediatRApplication();

// Persistence Layer
builder.Services.AddInterceptorPersistence();
builder.Services.ConfigureSqlServerRetryOptionsPersistence(builder.Configuration.GetSection(nameof(SqlServerRetryOptions)));
builder.Services.AddSqlServerPersistence();
builder.Services.AddRepositoryPersistence();

// Infrastructure Layer
builder.Services.AddServicesInfrastructure();
builder.Services.AddMediatRInfrastructure();
//builder.Services.ConfigureMailOptionsInfrastructure(builder.Configuration.GetSection(nameof(MailOption)));

// Optimize: Remove duplicate service registrations and consolidate
builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();
builder.Services.AddAntiforgery(options =>
{
    // Optional: Configure anti-forgery options if needed
    options.HeaderName = "X-CSRF-TOKEN"; // Default header for token validation
});

var app = builder.Build();

// Map Aspire default endpoints (health checks, etc.)
app.MapDefaultEndpoints();

// Optimize: Add compression before other middleware
app.UseResponseCompression();

// Using middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
// if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
app.UseSwaggerAPI1(); // => After MapCarter => Show Version

app.UseCors("CorsPolicy");

// app.UseHttpsRedirection();

app.UseAuthentication(); // Need to be before app.UseAuthorization();
app.UseAuthorization();

app.UseAntiforgery();

// Add API Endpoint with carter module
app.MapCarter();

try
{
    await app.RunAsync();
    Log.Information("Stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
    await app.StopAsync();
}
finally
{
    Log.CloseAndFlush();
    await app.DisposeAsync();
}