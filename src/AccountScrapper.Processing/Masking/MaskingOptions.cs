namespace AccountScrapper.Processing.Masking;

public enum MaskingMode
{
    /// <summary>Description is exported as-is. Use only for local debugging.</summary>
    None,

    /// <summary>Every word keeps its first character; the rest, and every digit, is replaced with '*'.</summary>
    Partial,

    /// <summary>The whole description is replaced with a fixed placeholder.</summary>
    Full,
}

public sealed class MaskingOptions
{
    public const string SectionName = "Masking";

    public MaskingMode Mode { get; set; } = MaskingMode.Partial;
}
