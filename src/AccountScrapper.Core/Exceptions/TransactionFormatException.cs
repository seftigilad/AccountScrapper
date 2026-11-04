namespace AccountScrapper.Core.Exceptions;

/// <summary>
/// A raw value couldn't be interpreted. The message intentionally names the field and row index
/// but not the value itself, so financial data never ends up in logs.
/// </summary>
public sealed class TransactionFormatException : FormatException
{
    public TransactionFormatException(string message)
        : base(message)
    {
    }

    public TransactionFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
