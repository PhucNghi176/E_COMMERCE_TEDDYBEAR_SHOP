using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<COMMAND_API>("command-api");

builder.AddProject<QUERRY_API>("querry-api");

builder.AddProject<AUTHORIZATION_API>("authorization-api");

builder.Build().Run();