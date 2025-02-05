var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.spa_server>("spa-server");

builder.Build().Run();
