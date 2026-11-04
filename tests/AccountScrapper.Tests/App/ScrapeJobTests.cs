using AccountScrapper.App;
using AccountScrapper.Core.Abstractions;
using AccountScrapper.Core.Models;
using AccountScrapper.Processing;
using AccountScrapper.Processing.Masking;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AccountScrapper.Tests.App;

/// <summary>End-to-end run of the pipeline with the browser and the file system replaced by fakes.</summary>
public class ScrapeJobTests
{
    [Fact]
    public async Task RunAsync_RequestsPreviousMonth_AndExportsProcessedTransactions()
    {
        var source = new FakeSource([new RawTransaction("20261015", "99.90", "סופר", TransactionDirection.Debit)]);
        var exporter = new CapturingExporter();
        var job = new ScrapeJob(
            new FixedCredentials(),
            source,
            new TransactionProcessor(new DescriptionMasker(Options.Create(new MaskingOptions { Mode = MaskingMode.None }))),
            exporter,
            new FixedTimeProvider(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero)),
            NullLogger<ScrapeJob>.Instance);

        var result = await job.RunAsync();

        var expectedPeriod = new DateRange(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31));
        Assert.Equal(expectedPeriod, source.RequestedPeriod);
        Assert.Equal(expectedPeriod, result.Period);
        Assert.Equal(1, result.TransactionCount);
        Assert.Equal("memory", result.OutputLocation);
        Assert.Equal(new Transaction(new DateOnly(2026, 10, 15), -99.90m, "סופר"), Assert.Single(exporter.Report!.Transactions));
    }

    private sealed class FakeSource(IReadOnlyList<RawTransaction> rows) : ITransactionSource
    {
        public DateRange? RequestedPeriod { get; private set; }

        public string Name => "Fake";

        public Task<IReadOnlyList<RawTransaction>> FetchTransactionsAsync(BankCredentials credentials, DateRange period, CancellationToken cancellationToken = default)
        {
            RequestedPeriod = period;
            return Task.FromResult(rows);
        }
    }

    private sealed class CapturingExporter : ITransactionExporter
    {
        public TransactionReport? Report { get; private set; }

        public Task<string> ExportAsync(TransactionReport report, CancellationToken cancellationToken = default)
        {
            Report = report;
            return Task.FromResult("memory");
        }
    }

    private sealed class FixedCredentials : ICredentialsProvider
    {
        public BankCredentials GetCredentials() => new("user", "password");
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
