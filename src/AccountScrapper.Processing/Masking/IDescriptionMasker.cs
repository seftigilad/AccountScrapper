namespace AccountScrapper.Processing.Masking;

/// <summary>Hides personal details (names, account/card/reference numbers) inside a transaction description.</summary>
public interface IDescriptionMasker
{
    string Mask(string description);
}
