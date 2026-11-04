using AccountScrapper.Core.Models;

namespace AccountScrapper.Core.Abstractions;

/// <summary>Data persistence: writes a report to some destination (JSON file, XML file, ...).</summary>
public interface ITransactionExporter
{
    /// <returns>A description of where the report was written (e.g. the file path).</returns>
    Task<string> ExportAsync(TransactionReport report, CancellationToken cancellationToken = default);
}
