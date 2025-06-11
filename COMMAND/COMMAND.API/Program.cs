using Carter;
using COMMAND.API.DependencyInjection.Extensions;
using COMMAND.API.Middlewares;
using COMMAND.APPLICATION.DependencyInjection.Extensions;
using COMMAND.INFRASTRUCTURE.DependencyInjection.Extensions;
using COMMAND.PERSISTENCE.DependencyInjection.Extensions;
using CONTRACT.CONTRACT.API.DependencyInjection.Extensions;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.INFRASTRUCTURE.DependencyInjection.Options;
using CONTRACT.CONTRACT.PERSISTENCE.DependencyInjection.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom
    .Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging
    .ClearProviders()
    .AddSerilog();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console());

// Add Carter module with assembly scanning for presentation layer
builder.Services.AddCarter();

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
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddHttpContextAccessor();

// Application Layer
builder.Services.AddMediatRApplication();


// Persistence Layer
builder.Services.AddInterceptorPersistence();
builder.Services.ConfigureSqlServerRetryOptionsPersistence(
    builder.Configuration.GetSection(nameof(SqlServerRetryOptions)));
builder.Services.AddSqlServerPersistence();
builder.Services.AddRepositoryPersistence();

// Infrastructure Layer
builder.Services.AddServicesInfrastructure();
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddMasstransitRabbitMQInfrastructure(builder.Configuration);
builder.Services.AddQuartzInfrastructure();
builder.Services.AddMediatRInfrastructure();
builder.Services.ConfigureCloudinaryOptionsInfrastructure(builder.Configuration.GetSection(nameof(CloudinaryOptions)));
builder.Services.ConfigureMailOptionsInfrastructure(builder.Configuration.GetSection(nameof(MailOption)));


// Add Middleware => Remember using middleware
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddTransient<ICurrentUserService, CurrentUserService>();
builder.Services.AddAntiforgery(options =>
{
    // Optional: Configure anti-forgery options if needed
    options.HeaderName = "X-CSRF-TOKEN"; // Default header for token validation
});

builder.Services.AddSignalR();

var app = builder.Build();

// Using middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
// if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
app.UseSwaggerAPI(); // => After MapCarter => Show Version

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