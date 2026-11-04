namespace AccountScrapper.Scraping.Browser;

public sealed class BrowserOptions
{
    public const string SectionName = "Browser";

    /// <summary>
    /// Run without a visible window. Defaults to false so the user can watch the run and complete
    /// an SMS/OTP challenge if the bank asks for one.
    /// </summary>
    public bool Headless { get; set; }

    /// <summary>Path to an installed Chrome/Chromium. When empty, PuppeteerSharp downloads a matching build on first run.</summary>
    public string? ExecutablePath { get; set; }

    /// <summary>Delay between browser operations, useful for watching/debugging a run.</summary>
    public int SlowMoMilliseconds { get; set; }

    /// <summary>Default timeout for navigation and waits.</summary>
    public int DefaultTimeoutSeconds { get; set; } = 60;
}
