using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using IBS.DataAccess.Data;
using IBS.DTOs;
using IBS.Models.Filpride.Books;
using IBS.Models.Filpride.MasterFile;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace IBS.Services
{
    public class GlImportService
    {
        public const string CsvHeader = "Date,Reference,Description,AccountNo,AccountName,Debit,Credit";
        public const int MaximumFileBytes = 10 * 1024 * 1024;
        public const int MaximumRows = 50000;
        public const int MaximumErrors = 100;
        private readonly ApplicationDbContext _dbContext;

        public GlImportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GlImportPreview> PreviewAsync(byte[] contents, string fileName,
            CancellationToken cancellationToken = default)
        {
            GlImportPreview preview = Parse(contents, fileName, cancellationToken);
            if (!preview.CanImport)
            {
                return preview;
            }

            foreach (var journal in preview.Rows.GroupBy(row => (row.Date, row.Reference)))
            {
                decimal debit = journal.Sum(row => row.Debit);
                decimal credit = journal.Sum(row => row.Credit);
                if (debit != credit)
                {
                    AddError(preview, $"Journal {journal.Key.Reference} on {journal.Key.Date:yyyy-MM-dd} is unbalanced: debit {debit:N4}, credit {credit:N4}.");
                }
            }

            await ValidateAccountsAsync(preview, cancellationToken);
            await ValidatePeriodsAsync(preview, cancellationToken);
            await ValidateDuplicatesAsync(preview, cancellationToken);
            return preview;
        }

        public async Task<Guid> ImportAsync(byte[] contents, string fileName, string user,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(user) || user.Length > 100)
            {
                throw new InvalidOperationException("The importing username must contain 1 to 100 characters.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(742036001)", cancellationToken);
            GlImportPreview preview = await PreviewAsync(contents, fileName, cancellationToken);
            if (!preview.CanImport)
            {
                throw new InvalidOperationException(preview.Errors.FirstOrDefault() ?? "The file contains no GL lines.");
            }

            var batch = new FilprideGlImportBatch
            {
                Id = Guid.NewGuid(),
                FileName = preview.FileName,
                FileHash = preview.FileHash,
                ImportedBy = user,
                ImportedAt = DateTimeHelper.GetCurrentPhilippineTime(),
                RowCount = preview.Rows.Count,
                JournalCount = preview.JournalCount,
                DateFrom = preview.Rows.Min(row => row.Date),
                DateTo = preview.Rows.Max(row => row.Date),
                TotalDebit = preview.TotalDebit,
                TotalCredit = preview.TotalCredit
            };
            _dbContext.FilprideGlImportBatches.Add(batch);
            _dbContext.FilprideGeneralLedgerBooks.AddRange(preview.Rows.Select(row => new FilprideGeneralLedgerBook
            {
                Date = row.Date,
                Reference = row.Reference,
                Description = row.Description,
                AccountId = row.AccountId,
                AccountNo = row.AccountNo,
                AccountTitle = row.AccountName,
                Debit = row.Debit,
                Credit = row.Credit,
                CreatedBy = user,
                CreatedDate = batch.ImportedAt,
                IsPosted = true,
                ModuleType = "LegacyGL",
                ImportBatchId = batch.Id
            }));
            _dbContext.FilprideAuditTrails.Add(new FilprideAuditTrail(user,
                $"Imported GL batch {batch.Id}: {batch.RowCount} lines, {batch.JournalCount} journals.", "GL Import"));
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return batch.Id;
        }

        public async Task<List<FilprideGlImportBatch>> GetHistoryAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.FilprideGlImportBatches.AsNoTracking()
                .OrderByDescending(batch => batch.ImportedAt).Take(50).ToListAsync(cancellationToken);
        }

        private async Task ValidateAccountsAsync(GlImportPreview preview, CancellationToken cancellationToken)
        {
            Dictionary<string, FilprideChartOfAccount> accounts = await _dbContext.FilprideChartOfAccounts
                .AsNoTracking().ToDictionaryAsync(account => account.AccountNumber, cancellationToken);
            foreach (GlImportRow row in preview.Rows)
            {
                if (!accounts.TryGetValue(row.AccountNo, out FilprideChartOfAccount? account))
                {
                    AddError(preview, $"Row {row.RowNumber}: account {row.AccountNo} does not exist or is hidden.");
                }
                else if (account.HasChildren)
                {
                    AddError(preview, $"Row {row.RowNumber}: account {row.AccountNo} has children. Select a posting account.");
                }
                else if (!string.Equals(row.AccountName, account.AccountName, StringComparison.Ordinal))
                {
                    AddError(preview, $"Row {row.RowNumber}: AccountName for {row.AccountNo} must be '{account.AccountName}'.");
                }
                else
                {
                    row.AccountId = account.AccountId;
                }
            }
        }

        private async Task ValidatePeriodsAsync(GlImportPreview preview, CancellationToken cancellationToken)
        {
            DateOnly firstDate = preview.Rows.Min(row => row.Date);

            bool hasAffectedClosedBalances = await _dbContext.FilprideGlPeriodBalances.IgnoreQueryFilters()
                .AnyAsync(period => period.IsValid && period.IsClosed && period.PeriodEndDate >= firstDate, cancellationToken);
            if (hasAffectedClosedBalances)
            {
                AddError(preview, "Reopen the affected month and all later closed months before importing historical entries.");
            }
        }

        private async Task ValidateDuplicatesAsync(GlImportPreview preview, CancellationToken cancellationToken)
        {
            if (await _dbContext.FilprideGlImportBatches.AnyAsync(batch => batch.FileHash == preview.FileHash, cancellationToken))
            {
                AddError(preview, "This file has already been imported.");
                return;
            }

            DateOnly firstDate = preview.Rows.Min(row => row.Date);
            DateOnly lastDate = preview.Rows.Max(row => row.Date);
            HashSet<(DateOnly Date, string Reference)> journalKeys = preview.Rows
                .Select(row => (row.Date, row.Reference)).ToHashSet();
            foreach (string[] references in preview.Rows.Select(row => row.Reference).Distinct().Chunk(500))
            {
                var existingJournals = await _dbContext.FilprideGeneralLedgerBooks.AsNoTracking()
                    .Where(gl => gl.Date >= firstDate && gl.Date <= lastDate && references.Contains(gl.Reference))
                    .Select(gl => new { gl.Date, gl.Reference }).Distinct().ToListAsync(cancellationToken);
                foreach (var journal in existingJournals)
                {
                    if (journalKeys.Contains((journal.Date, journal.Reference)))
                    {
                        AddError(preview, $"Journal {journal.Reference} on {journal.Date:yyyy-MM-dd} already exists in the GL.");
                    }
                }
            }
        }

        public static GlImportPreview Parse(byte[] contents, string fileName, CancellationToken cancellationToken = default)
        {
            var preview = new GlImportPreview
            {
                FileName = Path.GetFileName(fileName.Replace('\\', '/')),
                FileHash = Convert.ToHexString(SHA256.HashData(contents))
            };
            if (contents.Length == 0 || contents.Length > MaximumFileBytes)
            {
                AddError(preview, "Select a nonempty CSV file no larger than 10 MB.");
                return preview;
            }
            if (preview.FileName.Length is 0 or > 255 ||
                !string.Equals(Path.GetExtension(preview.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
            {
                AddError(preview, "Select a .csv file with a filename no longer than 255 characters.");
                return preview;
            }

            try
            {
                using var stream = new MemoryStream(contents);
                using var reader = new StreamReader(stream, new UTF8Encoding(false, true), false);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    TrimOptions = TrimOptions.Trim,
                    ExceptionMessagesContainRawData = false
                });
                if (!csv.Read())
                {
                    AddError(preview, "The file is empty.");
                    return preview;
                }
                csv.ReadHeader();
                string[] headers = csv.HeaderRecord ?? [];
                if (headers.Length > 0)
                {
                    headers[0] = headers[0].TrimStart('\uFEFF');
                }
                if (!headers.SequenceEqual(CsvHeader.Split(',')))
                {
                    AddError(preview, $"The header must be {CsvHeader}.");
                    return preview;
                }

                int rowNumber = 1;
                while (csv.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rowNumber++;
                    if (rowNumber > (MaximumRows + 1))
                    {
                        AddError(preview, $"A file may contain at most {MaximumRows:N0} GL lines.");
                        break;
                    }
                    if (csv.Parser.Count != headers.Length)
                    {
                        AddError(preview, $"Row {rowNumber}: expected seven fields.");
                        continue;
                    }
                    ReadRow(csv, rowNumber, preview);
                }
            }
            catch (CsvHelperException)
            {
                AddError(preview, "The CSV is malformed. Check its quotes and separators.");
            }
            catch (DecoderFallbackException)
            {
                AddError(preview, "Save the CSV using UTF-8 encoding.");
            }
            if (preview.Rows.Count == 0)
            {
                AddError(preview, "The file contains no valid GL lines.");
            }
            return preview;
        }

        private static void ReadRow(CsvReader csv, int rowNumber, GlImportPreview preview)
        {
            string[] fields = Enumerable.Range(0, 7).Select(column => (csv.GetField(column) ?? string.Empty).Trim()).ToArray();
            bool valid = DateOnly.TryParseExact(fields[0], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateOnly date);
            if (!valid)
            {
                AddError(preview, $"Row {rowNumber}: Date must use yyyy-MM-dd.");
            }
            valid = ValidateText(fields[1], "Reference", 20, rowNumber, preview) && valid;
            valid = ValidateText(fields[2], "Description", int.MaxValue, rowNumber, preview) && valid;
            valid = ValidateText(fields[3], "AccountNo", 50, rowNumber, preview) && valid;
            valid = ValidateText(fields[4], "AccountName", 200, rowNumber, preview) && valid;
            bool hasDebit = TryParseAmount(fields[5], out decimal debit);
            bool hasCredit = TryParseAmount(fields[6], out decimal credit);
            if (!hasDebit || !hasCredit)
            {
                AddError(preview, $"Row {rowNumber}: Debit and Credit must be nonnegative numbers with at most four decimal places, no thousands separators, and at most 14 integer digits.");
                valid = false;
            }
            else if ((debit > 0) == (credit > 0))
            {
                AddError(preview, $"Row {rowNumber}: enter a positive amount on exactly one side and zero on the other.");
                valid = false;
            }
            if (!valid)
            {
                return;
            }
            preview.Rows.Add(new GlImportRow
            {
                RowNumber = rowNumber,
                Date = date,
                Reference = fields[1],
                Description = fields[2],
                AccountNo = fields[3],
                AccountName = fields[4],
                Debit = debit,
                Credit = credit
            });
        }

        private static bool ValidateText(string value, string column, int maximumLength, int rowNumber, GlImportPreview preview)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(preview, $"Row {rowNumber}: {column} is required.");
                return false;
            }
            if (value.Length > maximumLength)
            {
                AddError(preview, $"Row {rowNumber}: {column} cannot exceed {maximumLength} characters.");
                return false;
            }
            if (value.Contains('\0'))
            {
                AddError(preview, $"Row {rowNumber}: {column} cannot contain NUL characters.");
                return false;
            }
            return true;
        }

        private static bool TryParseAmount(string text, out decimal amount)
        {
            return decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount)
                   && (amount >= 0) && (amount <= 99999999999999.9999m) && (decimal.Round(amount, 4) == amount);
        }

        private static void AddError(GlImportPreview preview, string error)
        {
            if (preview.Errors.Count < MaximumErrors)
            {
                preview.Errors.Add(error);
            }
        }
    }
}
