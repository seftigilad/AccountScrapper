using PuppeteerSharp;

namespace AccountScrapper.Scraping.Browser;

/// <summary>Creates configured browser instances, so scrapers don't deal with installation or launch flags.</summary>
public interface IBrowserLauncher
{
    Task<IBrowser> LaunchAsync(CancellationToken cancellationToken = default);
}
