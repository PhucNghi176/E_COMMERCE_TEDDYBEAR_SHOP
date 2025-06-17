using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureAppServiceEnvironment("appsvc");

builder.AddProject<COMMAND_API>("command-api")
    .WithHealthCheck("/health-command");

builder.AddProject<QUERY_API>("query-api").WithHealthCheck("/health-query");

builder.AddProject<AUTHORIZATION_API>("authorization-api").WithHealthCheck("/health-authorization");

builder.Build().Run();