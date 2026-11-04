using System.Text.Json;
using AccountScrapper.Core.Models;
using AccountScrapper.Export;
using AccountScrapper.Export.Json;
using Microsoft.Extensions.Options;

namespace AccountScrapper.Tests.Export;

public sealed class JsonTransactionExporterTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "AccountScrapperTests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_WritesReadableJsonNamedAfterThePeriod()
    {
        var exporter = new JsonTransactionExporter(Options.Create(new ExportOptions { OutputDirectory = _directory }));
        var report = new TransactionReport(
            new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)),
            [new Transaction(new DateOnly(2026, 10, 3), -1200.50m, "ש** ד****")],
            new DateTimeOffset(2026, 11, 4, 9, 0, 0, TimeSpan.Zero));

        var path = await exporter.ExportAsync(report);

        Assert.Equal(Path.Combine(_directory, "transactions_2026-10.json"), path);
        var text = await File.ReadAllTextAsync(path);
        Assert.Contains("ש** ד****", text); // Hebrew is written as-is, not \u-escaped

        using var json = JsonDocument.Parse(text);
        var root = json.RootElement;
        Assert.Equal("2026-10-01", root.GetProperty("periodStart").GetString());
        Assert.Equal("2026-10-31", root.GetProperty("periodEnd").GetString());
        Assert.Equal(1, root.GetProperty("transactionCount").GetInt32());

        var transaction = root.GetProperty("transactions")[0];
        Assert.Equal("2026-10-03", transaction.GetProperty("date").GetString());
        Assert.Equal(-1200.50m, transaction.GetProperty("amount").GetDecimal());
        Assert.Equal("ש** ד****", transaction.GetProperty("description").GetString());
    }

    [Fact]
    public async Task ExportAsync_LeavesNoTempFileBehind()
    {
        var exporter = new JsonTransactionExporter(Options.Create(new ExportOptions { OutputDirectory = _directory }));
        var report = new TransactionReport(new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31)), [], DateTimeOffset.UtcNow);

        await exporter.ExportAsync(report);

        Assert.Equal(["transactions_2026-10.json"], Directory.GetFiles(_directory).Select(Path.GetFileName));
    }
}
