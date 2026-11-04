using System.Text;
using AccountScrapper.App;
using AccountScrapper.App.Credentials;
using AccountScrapper.Core.Abstractions;
using AccountScrapper.Core.Exceptions;
using AccountScrapper.Export;
using AccountScrapper.Processing;
using AccountScrapper.Scraping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory, // find appsettings.json regardless of the working directory
});

// Loaded in every environment (not only Development) so credentials can be kept outside the repository.
// Stored under %APPDATA%\Microsoft\UserSecrets - see README.
builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services
    .AddHapoalimScraper(builder.Configuration)       // collect
    .AddTransactionProcessing(builder.Configuration) // process
    .AddJsonExport(builder.Configuration);           // save

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ICredentialsProvider, ConfiguredOrConsoleCredentialsProvider>();
builder.Services.AddSingleton<ScrapeJob>();

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};

try
{
    var result = await host.Services.GetRequiredService<ScrapeJob>().RunAsync(cancellation.Token);
    logger.LogInformation("Saved {Count} transactions for {Period} to {Location}", result.TransactionCount, result.Period, result.OutputLocation);
    return 0;
}
catch (LoginFailedException ex)
{
    logger.LogError("Login failed: {Reason}", ex.Message);
    return 2;
}
catch (ScrapingException ex)
{
    logger.LogError(ex, "Scraping failed");
    return 3;
}
catch (TransactionFormatException ex)
{
    logger.LogError("Could not process the collected data: {Reason}", ex.Message);
    return 4;
}
catch (OperationCanceledException)
{
    logger.LogWarning("Cancelled");
    return 130;
}
