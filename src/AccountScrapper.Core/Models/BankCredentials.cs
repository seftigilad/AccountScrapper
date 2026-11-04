namespace AccountScrapper.Core.Models;

/// <summary>Login credentials. Held in memory only for the duration of a run, never persisted or logged.</summary>
public sealed class BankCredentials
{
    public BankCredentials(string userCode, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userCode);
        ArgumentException.ThrowIfNullOrEmpty(password);
        UserCode = userCode;
        Password = password;
    }

    public string UserCode { get; }

    public string Password { get; }

    /// <summary>Deliberately hides the values so credentials can't leak through logs or debugger output.</summary>
    public override string ToString() => "BankCredentials { *** }";
}
