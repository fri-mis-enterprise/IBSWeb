using System.Drawing;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using IBS.DataAccess.Data;
using IBS.DTOs;
using IBS.Models.Enums;
using IBS.Models.Filpride.Books;
using IBS.Models.Filpride.MasterFile;
using IBS.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace IBS.Services
{
    public class GlImportService
    {
        public const string CsvHeader = "Date,Reference,Description,AccountNo,AccountName,Debit,Credit";
        public const string SubAccountCsvHeader = CsvHeader + ",SubAccountType,SubAccountId";
        public const string CompanyCsvHeader = SubAccountCsvHeader + ",Company";
        public const string CounterpartyCsvHeader = CompanyCsvHeader + ",CounterpartyType,CounterpartyId";
        public const int MaximumFileBytes = 10 * 1024 * 1024;
        public const int MaximumRows = 50000;
        public const int MaximumErrors = 100;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISubAccountResolver _subAccountResolver;

        public GlImportService(ApplicationDbContext dbContext, ISubAccountResolver subAccountResolver)
        {
            _dbContext = dbContext;
            _subAccountResolver = subAccountResolver;
        }

        public async Task<byte[]> CreateTemplateAsync(CancellationToken cancellationToken = default)
        {
            var accounts = await _dbContext.FilprideChartOfAccounts.AsNoTracking()
                .Include(account => account.ParentAccount)
                .Where(account => !account.HasChildren)
                .OrderBy(account => account.AccountNumber)
                .Select(account => new
                {
                    account.AccountNumber,
                    account.AccountName,
                    ParentAccountNo = account.ParentAccount != null ? account.ParentAccount.AccountNumber : string.Empty,
                    ParentAccountName = account.ParentAccount != null ? account.ParentAccount.AccountName : string.Empty
                })
                .ToListAsync(cancellationToken);
            var customers = await _dbContext.FilprideCustomers.AsNoTracking()
                .Where(customer => customer.IsActive)
                .OrderBy(customer => customer.CustomerName).ThenBy(customer => customer.CustomerId)
                .Select(customer => new { customer.CustomerId, customer.CustomerCode, customer.CustomerName })
                .ToListAsync(cancellationToken);
            var suppliers = await _dbContext.FilprideSuppliers.AsNoTracking()
                .Where(supplier => supplier.IsActive)
                .OrderBy(supplier => supplier.SupplierName).ThenBy(supplier => supplier.SupplierId)
                .Select(supplier => new { supplier.SupplierId, supplier.SupplierCode, supplier.SupplierName })
                .ToListAsync(cancellationToken);

            using var package = new ExcelPackage();
            var upload = CreateTemplateSheet(package.Workbook, "GL Upload", CounterpartyCsvHeader.Split(','));
            upload.Column(1).Style.Numberformat.Format = "yyyy-MM-dd";
            upload.Column(2).Style.Numberformat.Format = "@";
            upload.Column(3).Width = 55;
            upload.Column(4).Style.Numberformat.Format = "@";
            upload.Column(5).Width = 55;
            upload.Column(6).Style.Numberformat.Format = "0.0000";
            upload.Column(7).Style.Numberformat.Format = "0.0000";
            upload.Column(9).Style.Numberformat.Format = "0";
            upload.Column(10).Style.Numberformat.Format = "@";
            upload.Column(10).Width = 55;
            upload.Column(12).Style.Numberformat.Format = "0";
            var counterparties = upload.DataValidations.AddListValidation($"K2:K{MaximumRows + 1}");
            counterparties.Formula.Values.Add(nameof(CounterpartyType.Customer));
            counterparties.Formula.Values.Add(nameof(CounterpartyType.Supplier));
            counterparties.AllowBlank = true;
            counterparties.ShowErrorMessage = true;
            counterparties.ErrorTitle = "Invalid counterparty type";
            counterparties.Error = "Select Customer or Supplier, or leave both counterparty fields blank.";
            var types = upload.DataValidations.AddListValidation($"H2:H{MaximumRows + 1}");
            types.Formula.Values.Add(nameof(SubAccountType.Supplier));
            types.Formula.Values.Add(nameof(SubAccountType.Customer));
            types.AllowBlank = true;
            types.ShowErrorMessage = true;
            types.ErrorTitle = "Invalid sub-account type";
            types.Error = "Select Supplier or Customer, or leave both sub-account fields blank.";

            var accountSheet = CreateTemplateSheet(package.Workbook, "Chart of Accounts", ["AccountNo", "AccountName", "MainAccountNo", "MainAccountName"]);
            accountSheet.Column(1).Style.Numberformat.Format = "@";
            accountSheet.Column(2).Width = 70;
            if (accounts.Count > 0)
            {
                accountSheet.Cells[2, 1].LoadFromArrays(accounts.Select(account => new object[]
                {
                    account.AccountNumber, account.AccountName, account.ParentAccountNo, account.ParentAccountName
                }));
            }

            var customerSheet = CreateTemplateSheet(package.Workbook, "Customers",
                ["SubAccountType", "SubAccountId", "CustomerCode", "CustomerName"]);
            customerSheet.Column(3).Style.Numberformat.Format = "@";
            customerSheet.Column(4).Width = 70;
            if (customers.Count > 0)
            {
                customerSheet.Cells[2, 1].LoadFromArrays(customers.Select(customer => new object[]
                {
                    nameof(SubAccountType.Customer), customer.CustomerId, customer.CustomerCode ?? string.Empty, customer.CustomerName
                }));
            }

            var supplierSheet = CreateTemplateSheet(package.Workbook, "Suppliers",
                ["SubAccountType", "SubAccountId", "SupplierCode", "SupplierName"]);
            supplierSheet.Column(3).Style.Numberformat.Format = "@";
            supplierSheet.Column(4).Width = 70;
            if (suppliers.Count > 0)
            {
                supplierSheet.Cells[2, 1].LoadFromArrays(suppliers.Select(supplier => new object[]
                {
                    nameof(SubAccountType.Supplier), supplier.SupplierId, supplier.SupplierCode ?? string.Empty, supplier.SupplierName
                }));
            }

            foreach (var reference in new[] { accountSheet, customerSheet, supplierSheet })
            {
                reference.Cells[reference.Dimension.Address].AutoFilter = true;
            }

            var instructions = CreateTemplateSheet(package.Workbook, "Instructions", ["Step", "Instructions"]);
            instructions.Column(1).Width = 8;
            instructions.Column(2).Width = 110;
            instructions.Column(2).Style.WrapText = true;
            instructions.Cells[2, 1].LoadFromArrays(new object[][]
            {
                [1, "Fill GL Upload starting at row 2. Keep the twelve headers in row 1 unchanged; each date/reference journal must balance."],
                [2, "Copy AccountNo and the exact AccountName from Chart of Accounts. Account numbers and customer/supplier codes are text to preserve leading zeros."],
                [3, "For sub-accounts, copy SubAccountType and SubAccountId from Customers or Suppliers. Use the ID column, not CustomerCode or SupplierCode. Otherwise leave both fields blank."],
                [4, "Use yyyy-MM-dd dates and amounts with up to four decimal places, without thousands separators. Enter zero on the unused debit or credit side."],
                [5, "Select GL Upload, then Save As CSV UTF-8 (current sheet only). Upload that CSV in GL Import from the main menu, review the preview, and confirm. Keep this workbook separately for reference."],
                [6, "Reference sheets show registered active customers/suppliers and visible posting accounts at download time. Download again after master data changes; preview and confirmation validate current records."],
                [7, "Company is required on every GL line (up to 50 characters). Enter the company name directly; no Company master record is needed. Use consistent names for future per-company reports."],
                [8, "For counterparties, copy the type and database ID from Customers or Suppliers into CounterpartyType and CounterpartyId. The name is resolved automatically. Leave both blank when not applicable. Counterparty is separate from sub-account; for sales use the customer, and for purchases use the supplier."]
            });
            instructions.Cells[2, 1, 9, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            instructions.Rows[2, 9].Height = 45;
            package.Workbook.View.ActiveTab = 0;
            return await package.GetAsByteArrayAsync(cancellationToken);
        }

        private static ExcelWorksheet CreateTemplateSheet(ExcelWorkbook workbook, string name, string[] headers)
        {
            var sheet = workbook.Worksheets.Add(name);
            sheet.Cells[1, 1].LoadFromArrays(new[] { headers.Cast<object>().ToArray() });
            sheet.Cells[1, 1, 1, headers.Length].Style.Font.Bold = true;
            sheet.Cells[1, 1, 1, headers.Length].Style.Font.Color.SetColor(Color.White);
            sheet.Cells[1, 1, 1, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, 1, 1, headers.Length].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 78, 120));
            sheet.Cells[1, 1, 1, headers.Length].Style.WrapText = true;
            sheet.Row(1).Height = 30;
            sheet.View.FreezePanes(2, 1);
            for (var column = 1; column <= headers.Length; column++)
            {
                sheet.Column(column).Width = 22;
            }
            return sheet;
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
            await ValidateSubAccountsAsync(preview, cancellationToken);
            await ValidateCounterpartiesAsync(preview, cancellationToken);
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
                Company = row.Company,
                CounterpartyType = row.CounterpartyType,
                CounterpartyId = row.CounterpartyId,
                CounterpartyName = row.CounterpartyName,
                AccountId = row.AccountId,
                AccountNo = row.AccountNo,
                AccountTitle = row.AccountName,
                Debit = row.Debit,
                Credit = row.Credit,
                CreatedBy = user,
                CreatedDate = batch.ImportedAt,
                IsPosted = true,
                ModuleType = "LegacyGL",
                ImportBatchId = batch.Id,
                SubAccountType = row.SubAccountType,
                SubAccountId = row.SubAccountId,
                SubAccountName = row.SubAccountName
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

        private async Task ValidateSubAccountsAsync(GlImportPreview preview, CancellationToken cancellationToken)
        {
            var groups = preview.Rows.Where(row => row.SubAccountType.HasValue && row.SubAccountId.HasValue)
                .GroupBy(row => (Type: row.SubAccountType!.Value, Id: row.SubAccountId!.Value)).ToList();
            if (groups.Count == 0)
            {
                return;
            }

            var supplierIds = groups.Where(group => group.Key.Type == SubAccountType.Supplier)
                .Select(group => group.Key.Id).ToArray();
            var customerIds = groups.Where(group => group.Key.Type == SubAccountType.Customer)
                .Select(group => group.Key.Id).ToArray();
            var activeSupplierIds = await _dbContext.FilprideSuppliers.AsNoTracking()
                .Where(supplier => supplier.IsActive && supplierIds.Contains(supplier.SupplierId))
                .Select(supplier => supplier.SupplierId).ToHashSetAsync(cancellationToken);
            var activeCustomerIds = await _dbContext.FilprideCustomers.AsNoTracking()
                .Where(customer => customer.IsActive && customerIds.Contains(customer.CustomerId))
                .Select(customer => customer.CustomerId).ToHashSetAsync(cancellationToken);

            foreach (var group in groups)
            {
                var isActive = group.Key.Type == SubAccountType.Supplier
                    ? activeSupplierIds.Contains(group.Key.Id)
                    : activeCustomerIds.Contains(group.Key.Id);
                var subAccount = isActive
                    ? await _subAccountResolver.ResolveAsync(group.Key.Type, group.Key.Id, cancellationToken)
                    : null;
                foreach (var row in group)
                {
                    if (subAccount == null)
                    {
                        AddError(preview, $"Row {row.RowNumber}: {group.Key.Type} ID {group.Key.Id} does not exist or is inactive.");
                    }
                    else if (ValidateText(subAccount.Name, "SubAccountName", 200, row.RowNumber, preview))
                    {
                        row.SubAccountName = subAccount.Name;
                    }
                }
            }
        }

        private async Task ValidateCounterpartiesAsync(GlImportPreview preview, CancellationToken cancellationToken)
        {
            var rows = preview.Rows.Where(row => row.CounterpartyType.HasValue && row.CounterpartyId.HasValue).ToList();
            if (rows.Count == 0)
            {
                return;
            }

            var customerIds = rows.Where(row => row.CounterpartyType == CounterpartyType.Customer)
                .Select(row => row.CounterpartyId!.Value).Distinct().ToArray();
            var supplierIds = rows.Where(row => row.CounterpartyType == CounterpartyType.Supplier)
                .Select(row => row.CounterpartyId!.Value).Distinct().ToArray();
            var customers = await _dbContext.FilprideCustomers.AsNoTracking()
                .Where(customer => customer.IsActive && customerIds.Contains(customer.CustomerId))
                .ToDictionaryAsync(customer => customer.CustomerId, customer => customer.CustomerName, cancellationToken);
            var suppliers = await _dbContext.FilprideSuppliers.AsNoTracking()
                .Where(supplier => supplier.IsActive && supplierIds.Contains(supplier.SupplierId))
                .ToDictionaryAsync(supplier => supplier.SupplierId, supplier => supplier.SupplierName, cancellationToken);

            foreach (var row in rows)
            {
                var names = row.CounterpartyType == CounterpartyType.Customer ? customers : suppliers;
                if (!names.TryGetValue(row.CounterpartyId!.Value, out var name))
                {
                    AddError(preview, $"Row {row.RowNumber}: counterparty {row.CounterpartyType} ID {row.CounterpartyId} does not exist or is inactive.");
                }
                else if (ValidateText(name, "CounterpartyName", 200, row.RowNumber, preview))
                {
                    row.CounterpartyName = name;
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
                if (!headers.SequenceEqual((CsvHeader + ",Company").Split(',')) &&
                    !headers.SequenceEqual(CompanyCsvHeader.Split(',')) &&
                    !headers.SequenceEqual((CsvHeader + ",Company,CounterpartyType,CounterpartyId").Split(',')) &&
                    !headers.SequenceEqual(CounterpartyCsvHeader.Split(',')))
                {
                    AddError(preview, $"The header must be {CsvHeader},Company or {CompanyCsvHeader}. Company is required on every GL line. Either format may append CounterpartyType and CounterpartyId; supply both values or leave both blank. Names are resolved automatically.");
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
                        AddError(preview, $"Row {rowNumber}: expected {headers.Length} fields.");
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
            var fields = Enumerable.Range(0, csv.Parser.Count).Select(column => (csv.GetField(column) ?? string.Empty).Trim()).ToArray();
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
            var hasSubAccounts = csv.HeaderRecord!.Contains("SubAccountType");
            var company = fields[hasSubAccounts ? 9 : 7];
            valid = ValidateText(company, "Company", 50, rowNumber, preview) && valid;
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
            var subAccountType = (SubAccountType?)null;
            var subAccountId = (int?)null;
            if (hasSubAccounts && (fields[7].Length > 0 || fields[8].Length > 0))
            {
                subAccountType = fields[7] switch
                {
                    nameof(SubAccountType.Supplier) => SubAccountType.Supplier,
                    nameof(SubAccountType.Customer) => SubAccountType.Customer,
                    _ => null
                };
                if (!subAccountType.HasValue ||
                    !int.TryParse(fields[8], NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
                {
                    AddError(preview, $"Row {rowNumber}: supply both SubAccountType (Supplier or Customer) and a positive integer SubAccountId, or leave both blank.");
                    valid = false;
                }
                else
                {
                    subAccountId = id;
                }
            }
            var counterpartyType = (CounterpartyType?)null;
            var counterpartyId = (int?)null;
            if (csv.HeaderRecord.Contains("CounterpartyType") && (fields[^2].Length > 0 || fields[^1].Length > 0))
            {
                counterpartyType = fields[^2] switch
                {
                    nameof(CounterpartyType.Customer) => CounterpartyType.Customer,
                    nameof(CounterpartyType.Supplier) => CounterpartyType.Supplier,
                    _ => null
                };
                if (!counterpartyType.HasValue ||
                    !int.TryParse(fields[^1], NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
                {
                    AddError(preview, $"Row {rowNumber}: supply both CounterpartyType (Customer or Supplier) and a positive integer CounterpartyId, or leave both blank.");
                    valid = false;
                }
                else
                {
                    counterpartyId = id;
                }
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
                Company = company,
                CounterpartyType = counterpartyType,
                CounterpartyId = counterpartyId,
                Debit = debit,
                Credit = credit,
                SubAccountType = subAccountType,
                SubAccountId = subAccountId
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
