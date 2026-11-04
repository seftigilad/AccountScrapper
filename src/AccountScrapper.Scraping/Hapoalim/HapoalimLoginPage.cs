using AccountScrapper.Core.Exceptions;
using AccountScrapper.Core.Models;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace AccountScrapper.Scraping.Hapoalim;

/// <summary>Page object for the Hapoalim login screen.</summary>
internal sealed class HapoalimLoginPage
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(500);

    private readonly IPage _page;
    private readonly HapoalimOptions _options;
    private readonly ILogger _logger;

    public HapoalimLoginPage(IPage page, HapoalimOptions options, ILogger logger)
    {
        _page = page;
        _options = options;
        _logger = logger;
    }

    public async Task LoginAsync(BankCredentials credentials, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Opening login page");
        await _page.GoToAsync(HapoalimSite.LoginUrl(_options.BaseUrl), new NavigationOptions
        {
            WaitUntil = [WaitUntilNavigation.Networkidle2],
        });

        await _page.WaitForSelectorAsync(HapoalimSite.UserCodeInput, new WaitForSelectorOptions { Visible = true });
        await _page.TypeAsync(HapoalimSite.UserCodeInput, credentials.UserCode);
        await _page.TypeAsync(HapoalimSite.PasswordInput, credentials.Password);

        _logger.LogInformation("Submitting credentials");
        await _page.ClickAsync(HapoalimSite.SubmitButton);

        var outcome = await WaitForLoginOutcomeAsync(cancellationToken);
        switch (outcome)
        {
            case LoginOutcome.Success:
                _logger.LogInformation("Logged in");
                return;
            case LoginOutcome.InvalidCredentials:
                throw new LoginFailedException("The bank rejected the user code or password.");
            case LoginOutcome.PasswordChangeRequired:
                throw new LoginFailedException("The bank requires a password change. Log in manually once, then run again.");
            default:
                throw new LoginFailedException(
                    $"Login did not complete within {_options.LoginTimeoutSeconds} seconds. " +
                    "If the bank asked for an SMS code, run with Browser:Headless=false and enter it in the browser window.");
        }
    }

    /// <summary>
    /// The login flow passes through several redirects (and possibly an OTP screen), so rather than waiting
    /// for a single navigation we poll the URL until it lands on a page we recognize.
    /// </summary>
    private async Task<LoginOutcome> WaitForLoginOutcomeAsync(CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(_options.LoginTimeoutSeconds);
        string? lastPath = null;
        while (DateTime.UtcNow < deadline)
        {
            var url = _page.Url;
            var outcome = HapoalimSite.ClassifyLoginUrl(url);
            if (outcome != LoginOutcome.Pending)
            {
                return outcome;
            }

            // Log each page the flow passes through, to diagnose an unrecognized landing page.
            // Only the path is logged - the query string may carry identifying data.
            var path = PathOnly(url);
            if (path != lastPath)
            {
                _logger.LogInformation("Waiting for login to complete, current page: {Path}", path);
                lastPath = path;
            }

            await Task.Delay(PollInterval, cancellationToken);
        }

        return LoginOutcome.Pending;
    }

    private static string PathOnly(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.AbsolutePath : "(unknown)";
}
