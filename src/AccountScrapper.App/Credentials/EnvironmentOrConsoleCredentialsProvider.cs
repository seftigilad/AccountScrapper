using System.Text;
using AccountScrapper.Core.Abstractions;
using AccountScrapper.Core.Models;

namespace AccountScrapper.App.Credentials;

/// <summary>
/// Reads credentials from environment variables, or prompts for them (password input is hidden).
/// Credentials are never read from config files, so they can't be committed by accident.
/// </summary>
public sealed class EnvironmentOrConsoleCredentialsProvider : ICredentialsProvider
{
    public const string UserCodeVariable = "ACCOUNTSCRAPPER_USERCODE";
    public const string PasswordVariable = "ACCOUNTSCRAPPER_PASSWORD";

    public BankCredentials GetCredentials()
    {
        var userCode = Environment.GetEnvironmentVariable(UserCodeVariable);
        var password = Environment.GetEnvironmentVariable(PasswordVariable);

        if (string.IsNullOrWhiteSpace(userCode))
        {
            Console.Write("User code: ");
            userCode = Console.ReadLine();
        }

        if (string.IsNullOrEmpty(password))
        {
            Console.Write("Password: ");
            password = ReadHidden();
        }

        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                $"Credentials are required. Set {UserCodeVariable} / {PasswordVariable} or enter them when prompted.");
        }

        return new BankCredentials(userCode.Trim(), password);
    }

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
