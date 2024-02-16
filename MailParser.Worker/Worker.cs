using MailKit.Net.Pop3;
using MailKit;
using MimeKit;
using System.Text;
using MailKit.Net.Imap;

namespace MailParser.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string username = _config.GetValue<string>("Gmail:Username")!;
        string password = _config.GetValue<string>("Gmail:Password")!;

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var client = new ImapClient()) 
            {
                client.Connect("pop.gmail.com", 993, true);

                client.Authenticate(username, password, stoppingToken);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);

                for (int i = 0; i < inbox.Count; i++) 
                {
                    var message = inbox.GetMessage(i);
                    _logger.LogInformation("Subject: {0}", message.Subject);
                }

                client.Disconnect(true);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
