namespace AccountScrapper.Scraping.Hapoalim;

public sealed class HapoalimOptions
{
    public const string SectionName = "Hapoalim";

    public string BaseUrl { get; set; } = "https://login.bankhapoalim.co.il";

    /// <summary>
    /// Account to scrape, formatted "bank-branch-account" (e.g. "12-345-678901").
    /// When empty, the first open account is used. Prefer setting it via an environment variable
    /// (Hapoalim__AccountId) rather than committing it to appsettings.json.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>How long to wait for the login to complete. Generous, to allow entering an SMS code by hand.</summary>
    public int LoginTimeoutSeconds { get; set; } = 180;
}
