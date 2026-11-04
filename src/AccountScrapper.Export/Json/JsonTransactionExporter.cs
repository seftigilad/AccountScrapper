using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using AccountScrapper.Core.Abstractions;
using AccountScrapper.Core.Models;
using Microsoft.Extensions.Options;

namespace AccountScrapper.Export.Json;

/// <summary>Writes a <see cref="TransactionReport"/> to a UTF-8 JSON file.</summary>
public sealed class JsonTransactionExporter : ITransactionExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        // Keep Hebrew readable in the file instead of \uXXXX escapes.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private readonly ExportOptions _options;

    public JsonTransactionExporter(IOptions<ExportOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> ExportAsync(TransactionReport report, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);

        var directory = Path.GetFullPath(_options.OutputDirectory);
        Directory.CreateDirectory(directory);

        var fileName = _options.FileNamePattern.Replace("{period}", report.Period.Start.ToString("yyyy-MM", CultureInfo.InvariantCulture), StringComparison.Ordinal);
        var path = Path.Combine(directory, fileName);

        // Write to a temp file and move it into place so a crash never leaves a half-written report behind.
        var tempPath = path + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, JsonTransactionDocument.From(report), SerializerOptions, cancellationToken);
        }

        File.Move(tempPath, path, overwrite: true);
        return path;
    }
}
