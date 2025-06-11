var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.COMMAND_API>("command-api");

builder.AddProject<Projects.QUERRY_API>("querry-api");

builder.AddProject<Projects.AUTHORIZATION_API>("authorization-api");

builder.Build().Run();
