using AccountScrapper.Core.Abstractions;
using AccountScrapper.Core.Exceptions;
using AccountScrapper.Core.Models;
using AccountScrapper.Scraping.Browser;
using AccountScrapper.Scraping.Hapoalim.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace AccountScrapper.Scraping.Hapoalim;

/// <summary>Collects current-account (עו"ש) transactions from Bank Hapoalim's online banking.</summary>
public sealed class HapoalimTransactionSource : ITransactionSource
{
    private readonly IBrowserLauncher _browserLauncher;
    private readonly HapoalimOptions _options;
    private readonly BrowserOptions _browserOptions;
    private readonly ILogger<HapoalimTransactionSource> _logger;

    public HapoalimTransactionSource(
        IBrowserLauncher browserLauncher,
        IOptions<HapoalimOptions> options,
        IOptions<BrowserOptions> browserOptions,
        ILogger<HapoalimTransactionSource> logger)
    {
        _browserLauncher = browserLauncher;
        _options = options.Value;
        _browserOptions = browserOptions.Value;
        _logger = logger;
    }

    public string Name => "Bank Hapoalim";

    public async Task<IReadOnlyList<RawTransaction>> FetchTransactionsAsync(
        BankCredentials credentials,
        DateRange period,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        ArgumentNullException.ThrowIfNull(period);

        try
        {
            await using var browser = await _browserLauncher.LaunchAsync(cancellationToken);
            await using var page = await browser.NewPageAsync();
            page.DefaultTimeout = (int)TimeSpan.FromSeconds(_browserOptions.DefaultTimeoutSeconds).TotalMilliseconds;

            await new HapoalimLoginPage(page, _options, _logger).LoginAsync(credentials, cancellationToken);

            var api = new HapoalimApiClient(page, _options.BaseUrl);
            var restContext = await api.GetRestContextAsync();
            var account = SelectAccount(await api.GetAccountsAsync());
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Fetching transactions for {Period}", period);
            var transactions = await api.GetTransactionsAsync(restContext, account.Id, period);

            if (transactions.Count >= HapoalimSite.MaxTransactionsPerRequest)
            {
                _logger.LogWarning("Received the maximum page size ({Max}); some transactions may be missing", HapoalimSite.MaxTransactionsPerRequest);
            }

            var posted = transactions.Where(t => !t.IsPending).ToList();
            _logger.LogInformation("Fetched {Count} transactions ({Pending} pending skipped)", posted.Count, transactions.Count - posted.Count);

            return posted.Select(HapoalimTransactionMapper.ToRaw).ToList();
        }
        catch (PuppeteerException ex)
        {
            throw new ScrapingException($"Browser automation failed: {ex.Message}", ex);
        }
    }

    private HapoalimAccountDto SelectAccount(IReadOnlyList<HapoalimAccountDto> accounts)
    {
        _logger.LogInformation("Found {Count} account(s)", accounts.Count);

        if (!string.IsNullOrWhiteSpace(_options.AccountId))
        {
            return accounts.FirstOrDefault(a => a.Id == _options.AccountId.Trim())
                ?? throw new ScrapingException("The configured Hapoalim:AccountId was not found among the user's accounts.");
        }

        var open = accounts.Where(a => a.IsOpen).ToList();
        if (open.Count > 1)
        {
            _logger.LogWarning(
                "Several open accounts found ({Accounts}); using the first. Set Hapoalim:AccountId to choose another",
                string.Join(", ", open.Select(a => a.MaskedId)));
        }

        return open.FirstOrDefault() ?? throw new ScrapingException("No open current account was found.");
    }
}
