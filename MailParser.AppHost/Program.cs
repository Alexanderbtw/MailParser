var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MailParser_Worker>("mailparser.worker");

builder.Build().Run();
