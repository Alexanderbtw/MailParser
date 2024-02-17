using MailParser.EmailWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(
        "appsettings.json", 
        optional: true, 
        reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true, 
        reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddServiceDefaults();
builder.Services.AddHostedService<EmailWorker>();

var host = builder.Build();
host.Run();
