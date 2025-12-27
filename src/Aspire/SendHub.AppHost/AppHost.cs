var builder = DistributedApplication.CreateBuilder(args);

builder
    .AddProject<Projects.SendHub_Daemon>("Daemon");

builder.Build().Run();
