using System.Text.RegularExpressions;

namespace AccountScrapper.Scraping.Hapoalim;

/// <summary>
/// Everything that is coupled to Bank Hapoalim's markup and URLs, in one place.
/// When the bank changes its site, this file is the first (and hopefully only) place to update.
/// </summary>
internal static partial class HapoalimSite
{
    public const string UserCodeInput = "#userCode";
    public const string PasswordInput = "#password";
    public const string SubmitButton = ".login-btn";

    /// <summary>The Angular portal exposes its REST context on this global once the app has bootstrapped.</summary>
    public const string AppReadyExpression = "() => !!(window.bnhpApp && window.bnhpApp.restContext)";
    public const string RestContextExpression = "window.bnhpApp.restContext";

    public static string LoginUrl(string baseUrl) => $"{baseUrl}/cgi-bin/poalwwwc?reqName=getLogonPage";

    public static string AccountsUrl(string baseUrl) => $"{baseUrl}/ServerServices/general/accounts";

    public static string TransactionsUrl(string baseUrl, string restContext, string accountId, DateOnly from, DateOnly to) =>
        $"{baseUrl}/{restContext}/current-account/transactions" +
        $"?accountId={Uri.EscapeDataString(accountId)}" +
        $"&numItemsPerPage={MaxTransactionsPerRequest}" +
        $"&retrievalStartDate={from:yyyyMMdd}" +
        $"&retrievalEndDate={to:yyyyMMdd}" +
        "&sortCode=1";

    public const int MaxTransactionsPerRequest = 1000;

    /// <summary>Works out where the login flow ended up, based on the URL the site redirected to.</summary>
    public static LoginOutcome ClassifyLoginUrl(string url)
    {
        if (HomePageUrl().IsMatch(url))
        {
            return LoginOutcome.Success;
        }

        if (InvalidCredentialsUrl().IsMatch(url))
        {
            return LoginOutcome.InvalidCredentials;
        }

        if (PasswordChangeUrl().IsMatch(url))
        {
            return LoginOutcome.PasswordChangeRequired;
        }

        return LoginOutcome.Pending;
    }

    [GeneratedRegex(@"/(portalserver/HomePage|ng-portals(-bt)?/rb/he/homepage)", RegexOptions.IgnoreCase)]
    private static partial Regex HomePageUrl();

    [GeneratedRegex(@"/AUTHENTICATE/LOGON\?.*errorcode=", RegexOptions.IgnoreCase)]
    private static partial Regex InvalidCredentialsUrl();

    [GeneratedRegex(@"/(MCP|ABOUTTOEXPIRE)/START", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordChangeUrl();
}

internal enum LoginOutcome
{
    Pending,
    Success,
    InvalidCredentials,
    PasswordChangeRequired,
}
