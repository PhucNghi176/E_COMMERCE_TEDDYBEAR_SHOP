using Carter;
using CONTRACT.CONTRACT.API.DependencyInjection.Extensions;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.PERSISTENCE.DependencyInjection.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using QUERY.API.DependencyInjection.Extensions;
using QUERY.API.Middlewares;
using QUERY.APPLICATION.DependencyInjection.Extensions;
using QUERY.INFRASTRUCTURE.DependencyInjection.Extensions;
using QUERY.PERSISTENCE.DependencyInjection.Extensions;
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
    .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console());

// Add Carter module
builder.Services.AddCarter();

// Optimize: Add response compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

// Optimize: Add output caching for query responses
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("*"));
    
    options.AddPolicy("ProductCache", builder => builder
        .Expire(TimeSpan.FromMinutes(5))
        .SetVaryByQuery("searchTerm", "sortColumn", "sortOrder", "tag", "pageIndex", "pageSize"));
        
    options.AddPolicy("TagCache", builder => builder
        .Expire(TimeSpan.FromHours(1))); // Tags change less frequently
});

builder.Services
    .AddSwaggerGenNewtonsoftSupport()
    .AddFluentValidationRulesToSwagger()
    .AddEndpointsApiExplorer()
    .AddSwaggerAPI();

builder.Services
    .AddApiVersioning(options => options.ReportApiVersions = true)
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.ConfigureCors();

//API Layer
builder.Services.AddJwtAuthenticationAPI1(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddHttpContextAccessor();

// Application Layer
builder.Services.AddMediatRApplication();

// Persistence Layer
builder.Services.ConfigureSqlServerRetryOptionsPersistence(
    builder.Configuration.GetSection(nameof(SqlServerRetryOptions))
);
builder.Services.AddSqlServerPersistence();
builder.Services.AddRepositoryPersistence();
builder.Services.ConfigureServicesInfrastructure(builder.Configuration);

// Infrastructure Layer
builder.Services.AddServicesInfrastructure();
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddMediatRInfrastructure();

builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MapDefaultEndpoints();

// Optimize: Add compression before other middleware
app.UseResponseCompression();

// Optimize: Add output caching
app.UseOutputCache();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline. 
// if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
app.UseSwaggerAPI(); // => After MapCarter => Show Version

app.UseCors("CorsPolicy");

// app.UseHttpsRedirection();
// app.UseRouting();

app.UseAuthentication(); // Need to be before app.UseAuthorization();
app.UseAuthorization();


// 7. Map Carter endpoints
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