using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using AngleSharp.Dom;
using MailParser.Domain;

namespace MailParser.EmailWorker;

public class EmailWorker : BackgroundService
{
    private readonly ILogger<EmailWorker> _logger;
    private readonly IConfiguration _config;

    public EmailWorker(ILogger<EmailWorker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string username = _config.GetValue<string>("Gmail:Username")!;
        string password = _config.GetValue<string>("Gmail:Password")!;
        string requiredSubject = _config.GetValue<string>("Parser:RequiredSubject") ?? string.Empty;

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var client = new ImapClient()) 
            {
                await client.ConnectAsync("pop.gmail.com", 993, true, stoppingToken);

                await client.AuthenticateAsync(username, password, stoppingToken);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite, stoppingToken);

                var ids = await inbox.SearchAsync(SearchOptions.All, SearchQuery.And(SearchQuery.NotSeen, SearchQuery.SubjectContains(requiredSubject)), stoppingToken);
                foreach (var id in ids.UniqueIds)
                {
                    var message = await inbox.GetMessageAsync(id);
                    _logger.LogInformation("New message at: {0}", message.Date);

                    await ParseTable(message.HtmlBody, stoppingToken);

                    //await inbox.SetFlagsAsync(id, MessageFlags.Seen, false, stoppingToken);
                }

                client.Disconnect(true, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ParseTable(string htmlBody, CancellationToken cancellationToken = default)
    {
        var dom = await new HtmlParser().ParseDocumentAsync(htmlBody, cancellationToken);

        var rows = dom.QuerySelectorAll<IHtmlTableRowElement>("tr");

        var info = new LeadInfo();

        foreach (var row in rows)
        {
            var propName = row?.QuerySelector<IHtmlTableHeaderCellElement>("th")?.Text();
            var propValue = row?.QuerySelector<IHtmlTableDataCellElement>("td")?.Text();

            if (propValue == null || propName == null)
                continue;

            switch (propName)
            {
                case "ID": 
                    info.Id = propValue; break;
                case "Имя":
                    info.Name = propValue; break;
                case "Email":
                    info.Email = propValue; break;
                case "Телефон":
                    info.Phone = propValue; break;
                case "Регион":
                    info.Region = propValue; break;
                case "Город":
                    info.City = propValue; break;
                case "Комментарий":
                    info.Comments = propValue; break;
                case "Дата":
                    info.Date = DateTime.Parse(propValue); break;
                case "Сумма долга":
                    info.AmountOfDebt = decimal.Parse(propValue); break;
            }
        }

        _logger.LogCritical(info.ToString());
    }
}
