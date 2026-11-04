# AccountScrapper

Pulls last month's current-account (עו"ש) transactions from **Bank Hapoalim** online banking using
**PuppeteerSharp**, normalizes them, masks the descriptions, and saves them as **JSON**.

## Running

Requires the .NET 10 SDK. On the first run PuppeteerSharp downloads a matching Chrome build, which takes a minute.

```powershell
dotnet run --project src/AccountScrapper.App
```

You are prompted for the user code and password (the password is hidden as you type). For unattended runs, set them
as environment variables instead:

```powershell
$env:ACCOUNTSCRAPPER_USERCODE = "..."
$env:ACCOUNTSCRAPPER_PASSWORD = "..."
```

Output goes to `output/transactions_<yyyy-MM>.json`:

```json
{
  "periodStart": "2026-10-01",
  "periodEnd": "2026-10-31",
  "generatedAt": "2026-11-04T09:00:00+00:00",
  "transactionCount": 2,
  "transactions": [
    { "date": "2026-10-03", "amount": -1200.50, "description": "ש** ד****" },
    { "date": "2026-10-10", "amount": 9500.00, "description": "מ*****" }
  ]
}
```

Amounts are signed: positive is money in, negative is money out.

### Configuration (`src/AccountScrapper.App/appsettings.json`)

You can override any setting with an environment variable (`Section__Key`) or a command-line argument (`--Section:Key=value`).

| Setting | Default | Meaning |
|---|---|---|
| `Browser:Headless` | `false` | When false, the browser window is visible, so you can type an SMS code if the bank asks for one. |
| `Browser:ExecutablePath` | empty | Use an installed Chrome instead of downloading one. |
| `Hapoalim:AccountId` | empty | `bank-branch-account` to pick an account. When empty, the first open account is used. |
| `Hapoalim:LoginTimeoutSeconds` | `180` | How long to wait for the login, including any SMS code step. |
| `Masking:Mode` | `Partial` | `Partial`: first letter of each word kept, every digit hidden. `Full`: `***`. `None`: no masking. |
| `Export:OutputDirectory` | `output` | Where the JSON file is written. |

Exit codes: `0` success, `2` login failed, `3` scraping failed, `4` the data could not be parsed, `130` cancelled.

## Architecture

```
src/
  AccountScrapper.Core         Models + contracts. No dependencies.
  AccountScrapper.Scraping     Collect:  PuppeteerSharp, Hapoalim login page object, data extraction
  AccountScrapper.Processing   Process:  date/amount parsing, text cleanup, sensitive-data masking
  AccountScrapper.Export       Save:     JSON exporter
  AccountScrapper.App          Composition root (DI, config, credentials prompt) + ScrapeJob orchestration
tests/
  AccountScrapper.Tests        Unit tests for every layer + an end-to-end pipeline test using fakes
```

Data flows in one direction through the interfaces in `Core`:

```
ITransactionSource  ──RawTransaction──▶  ITransactionProcessor  ──Transaction──▶  ITransactionExporter
(site-specific, strings as scraped)      (uniform, typed, masked)                 (JSON file)
```

- **The layers depend only on `Core`, not on each other.** The processing layer never sees PuppeteerSharp, and the
  exporter doesn't know which bank the data came from. Each project registers itself with one extension method
  (`AddHapoalimScraper`, `AddTransactionProcessing`, `AddJsonExport`). Supporting another bank or adding an XML
  exporter means adding one class and changing one line in `Program.cs`.
- **`RawTransaction` holds the date and amount as text.** A scraper only copies what the site shows, and all format
  handling lives in one place: `DateParser` and `AmountParser`. They support `yyyyMMdd`, `dd/MM/yyyy`, `dd.MM.yy`,
  ISO dates, `₪`/`ש"ח` symbols, thousands separators, a trailing minus (`120.00-`), negatives in parentheses, and
  the invisible RTL marks that Hebrew pages insert. Credit/debit is a separate field, because some sites show the
  absolute amount plus a direction.
- **Code tied to the site is kept in one place.** `HapoalimSite` holds every Hapoalim URL, selector and redirect
  pattern, so when the bank changes its site, that is the file to update.

### How the Hapoalim scraper works

1. `HapoalimLoginPage` (a page object) opens the login page, fills in `#userCode` and `#password`, and submits.
   Login goes through several redirects and sometimes an SMS step, so instead of waiting for one navigation it
   checks the URL until it reaches a known page: the home page, the invalid-credentials page or the
   password-change page.
2. After login, the bank's Angular app loads the transactions grid from a JSON endpoint. `HapoalimApiClient` calls
   that same endpoint with `fetch` from inside the logged-in page (`EvaluateFunctionAsync`). The request carries the
   browser's own session, and no cookies or tokens are copied out of the browser. This is more reliable than parsing
   the rendered grid, which uses virtual scrolling and generated class names. The response is deserialized into
   typed DTOs in .NET.
3. `HapoalimTransactionMapper` converts the DTOs to `RawTransaction`. Pending (not yet posted) entries are skipped.

## Privacy

- Credentials come only from the prompt or from environment variables, never from code or config files.
  `BankCredentials.ToString()` hides the values.
- Logs contain only counts and steps. They never include amounts, descriptions or account numbers. Error messages
  about bad rows give the row number, not the value.
- No screenshots, HTML dumps or trace files are written.
- Descriptions are masked before export (`Partial` by default).
- `output/` is in `.gitignore`.

## Tests

```powershell
dotnet test
```

The tests cover date ranges, every supported date and amount format, the masking modes, the processor (mixed
formats, period filtering, failing on a bad row without leaking its value), JSON output, Hapoalim URL
classification and mapping, and a full `ScrapeJob` run with fake source and exporter. None of them touch the
network.
