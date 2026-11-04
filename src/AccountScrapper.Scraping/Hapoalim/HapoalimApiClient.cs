using System.Text.Json;
using System.Text.Json.Serialization;
using AccountScrapper.Core.Exceptions;
using AccountScrapper.Core.Models;
using AccountScrapper.Scraping.Hapoalim.Dto;
using PuppeteerSharp;

namespace AccountScrapper.Scraping.Hapoalim;

/// <summary>
/// Reads data through the same JSON endpoints Hapoalim's own Angular pages use to render the account screens.
/// Requests are issued with <c>fetch</c> from inside the logged-in page, so they carry the browser session
/// (cookies, headers) exactly like the site's own calls, and nothing about the session leaves the browser.
/// </summary>
internal sealed class HapoalimApiClient
{
    /// <summary>Returns the response body as text (null for 204), so deserialization happens in .NET with strong types.</summary>
    private const string FetchJsonScript = """
        async (url) => {
            const response = await fetch(url, { credentials: 'include', headers: { 'Accept': 'application/json' } });
            if (response.status === 204) return null;
            if (!response.ok) throw new Error('HTTP ' + response.status);
            return await response.text();
        }
        """;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private readonly IPage _page;
    private readonly string _baseUrl;

    public HapoalimApiClient(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task<string> GetRestContextAsync()
    {
        await _page.WaitForFunctionAsync(HapoalimSite.AppReadyExpression);
        var restContext = await _page.EvaluateExpressionAsync<string>(HapoalimSite.RestContextExpression);
        return restContext.Trim('/');
    }

    public async Task<IReadOnlyList<HapoalimAccountDto>> GetAccountsAsync()
    {
        var accounts = await GetJsonAsync<List<HapoalimAccountDto>>(HapoalimSite.AccountsUrl(_baseUrl), "accounts");
        return accounts ?? [];
    }

    public async Task<IReadOnlyList<HapoalimTransactionDto>> GetTransactionsAsync(string restContext, string accountId, DateRange period)
    {
        var url = HapoalimSite.TransactionsUrl(_baseUrl, restContext, accountId, period.Start, period.End);
        var response = await GetJsonAsync<HapoalimTransactionsResponseDto>(url, "transactions");
        return response?.Transactions ?? [];
    }

    private async Task<T?> GetJsonAsync<T>(string url, string resourceName)
    {
        string? json;
        try
        {
            json = await _page.EvaluateFunctionAsync<string?>(FetchJsonScript, url);
        }
        catch (EvaluationFailedException ex)
        {
            // The URL may contain the account number, so it is deliberately left out of the message.
            throw new ScrapingException($"Request for {resourceName} failed: {ex.Message}", ex);
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new ScrapingException($"Unexpected {resourceName} response format - the site's API may have changed.", ex);
        }
    }
}
