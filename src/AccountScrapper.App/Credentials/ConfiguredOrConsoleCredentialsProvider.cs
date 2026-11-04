using System.Text;
using AccountScrapper.Core.Abstractions;
using AccountScrapper.Core.Models;
using Microsoft.Extensions.Configuration;

namespace AccountScrapper.App.Credentials;

/// <summary>
/// Resolves credentials in this order:
/// <list type="number">
///   <item>Configuration keys <c>Credentials:UserCode</c> / <c>Credentials:Password</c> - intended for
///         .NET User Secrets (stored in the user profile, outside the repository) or
///         <c>Credentials__UserCode</c>-style environment variables.</item>
///   <item>The <c>ACCOUNTSCRAPPER_USERCODE</c> / <c>ACCOUNTSCRAPPER_PASSWORD</c> environment variables.</item>
///   <item>An interactive prompt (password input is hidden).</item>
/// </list>
/// Do not put credentials in appsettings.json - it is part of the repository.
/// </summary>
public sealed class ConfiguredOrConsoleCredentialsProvider : ICredentialsProvider
{
    public const string UserCodeKey = "Credentials:UserCode";
    public const string PasswordKey = "Credentials:Password";
    public const string UserCodeVariable = "ACCOUNTSCRAPPER_USERCODE";
    public const string PasswordVariable = "ACCOUNTSCRAPPER_PASSWORD";

    private readonly IConfiguration _configuration;

    public ConfiguredOrConsoleCredentialsProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public BankCredentials GetCredentials()
    {
        var userCode = FirstNonEmpty(_configuration[UserCodeKey], Environment.GetEnvironmentVariable(UserCodeVariable));
        var password = FirstNonEmpty(_configuration[PasswordKey], Environment.GetEnvironmentVariable(PasswordVariable));

        if (userCode is null)
        {
            Console.Write("User code: ");
            userCode = Console.ReadLine();
        }

        if (password is null)
        {
            Console.Write("Password: ");
            password = ReadHidden();
        }

        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                $"Credentials are required. Use user-secrets ({UserCodeKey} / {PasswordKey}), " +
                $"set {UserCodeVariable} / {PasswordVariable}, or enter them when prompted.");
        }

        return new BankCredentials(userCode.Trim(), password);
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrEmpty(value));

    private static string ReadHidden()
    {
        // Redirected input (e.g. piped) can't be masked; just read it.
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? string.Empty;
        }

        var buffer = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return buffer.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.Length--;
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
                Console.Write('*');
            }
        }
    }
}
