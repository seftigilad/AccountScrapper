using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace AccountScrapper.Scraping.Browser;

public sealed class PuppeteerBrowserLauncher : IBrowserLauncher
{
    private readonly BrowserOptions _options;
    private readonly ILogger<PuppeteerBrowserLauncher> _logger;

    public PuppeteerBrowserLauncher(IOptions<BrowserOptions> options, ILogger<PuppeteerBrowserLauncher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IBrowser> LaunchAsync(CancellationToken cancellationToken = default)
    {
        var executablePath = await ResolveExecutablePathAsync();
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Launching browser (headless: {Headless})", _options.Headless);
        return await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = _options.Headless,
            ExecutablePath = executablePath,
            SlowMo = _options.SlowMoMilliseconds,
            DefaultViewport = null, // use the real window size when headful
            Args = ["--lang=he-IL"],
        });
    }

    private async Task<string> ResolveExecutablePathAsync()
    {
        if (!string.IsNullOrWhiteSpace(_options.ExecutablePath))
        {
            return _options.ExecutablePath;
        }

        _logger.LogInformation("Ensuring a compatible Chrome build is installed (downloaded once on first run)");
        var installed = await new BrowserFetcher().DownloadAsync();
        return installed.GetExecutablePath();
    }
}
