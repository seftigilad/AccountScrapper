namespace AccountScrapper.Core.Exceptions;

/// <summary>The source site could not be scraped (unexpected page state, failed request, timeout...).</summary>
public class ScrapingException : Exception
{
    public ScrapingException(string message)
        : base(message)
    {
    }

    public ScrapingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>The site rejected the login, or requires an action the scraper can't perform (e.g. password change).</summary>
public sealed class LoginFailedException : ScrapingException
{
    public LoginFailedException(string message)
        : base(message)
    {
    }
}
