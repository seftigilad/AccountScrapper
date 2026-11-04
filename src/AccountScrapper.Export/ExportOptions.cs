namespace AccountScrapper.Export;

public sealed class ExportOptions
{
    public const string SectionName = "Export";

    /// <summary>Directory for output files. Relative paths are resolved against the current directory.</summary>
    public string OutputDirectory { get; set; } = "output";

    /// <summary>File name pattern. <c>{period}</c> is replaced with the period's month, e.g. "2026-10".</summary>
    public string FileNamePattern { get; set; } = "transactions_{period}.json";
}
