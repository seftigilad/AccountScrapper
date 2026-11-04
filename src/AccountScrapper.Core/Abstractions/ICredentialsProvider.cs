using AccountScrapper.Core.Models;

namespace AccountScrapper.Core.Abstractions;

/// <summary>Supplies login credentials at runtime, so they never have to live in code or config files.</summary>
public interface ICredentialsProvider
{
    BankCredentials GetCredentials();
}
