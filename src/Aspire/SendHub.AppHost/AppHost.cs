var builder = DistributedApplication.CreateBuilder(args);

builder
    .AddProject<Projects.SendHub_Web>("web");

builder.Build().Run();
