# GL import

Load users, company details, and the chart of accounts before importing. Imports can append journal entries to an empty or populated GL. The importer never clears existing data or updates operational balances.

Open **Reports > General Ledger > GL Import**. Download the template, preview the file, correct any errors, then confirm the complete batch.

## File format

```csv
Date,Reference,Description,AccountNo,AccountName,Debit,Credit
2024-01-15,LEG-2024-0001,Illustrative legacy journal,SAMPLE-CASH,Legacy cash,1000.0000,0.0000
2024-01-15,LEG-2024-0001,Illustrative legacy journal,SAMPLE-REVENUE,Legacy revenue,0.0000,1000.0000
```

Replace the sample codes with existing visible posting accounts without children. Save as UTF-8 CSV; a UTF-8 BOM is supported. Preserve leading zeros in account codes. The headers and column order must match exactly.

Each row is one GL line. A journal is identified by its date and reference and must balance exactly. Use `yyyy-MM-dd` dates, positive amounts on one side, and zero on the other. Amounts must fit `numeric(18,4)`, with no currency symbols or thousands separators. Do not include totals or subtotal rows.

Reference is limited to 20 characters, AccountNo to 50, and AccountName to 200. All text fields are required. AccountName must exactly match the chart-of-accounts name for AccountNo, including capitalization; leading and trailing CSV whitespace is trimmed before comparison. This check runs during preview and confirmation. AccountNo resolves the internal account ID, and the chart of accounts supplies financial classifications. No accounts, invoices, vouchers, operational balances, sub-account records, or inventory transactions are created.

A file may contain at most 50,000 GL lines and 10 MB. Split larger files at journal boundaries. Preview shows up to 100 valid lines and 100 errors; any error blocks the entire batch. The original file stays in the uploading user's server-side preview cache for up to 15 minutes. Restarting the server or evicting the cache requires another upload.

## Posting and history

Confirmation revalidates the original file and current database state. Identical files and existing date/reference journals are rejected. Concurrent imports are serialized, and the file hash has a unique database index. Batch metadata, GL rows, and the audit entry are saved in one transaction.

History retains the batch ID, filename, hash, importing user and time, date range, line and journal counts, and debit/credit totals. Each GL line has a batch link. The original file is not retained after confirmation or cache expiry. No batch deletion or replacement action is provided.

## Financial reports

Supply the complete agreed coverage, including opening entries when required. Do not combine cumulative balances with the underlying movements for the same period.

Closed balances at or after the first imported date block the file: reopen affected and later closed months before adding earlier entries. After importing all entries, lock and close months in chronological order. The existing closure sequence requires preceding months in each fiscal year to be closed, including empty months before the first imported month.

The existing trial balance uses stored period balances, and some financial reports use stored monthly earnings. Importing alone does not generate these snapshots. Reconcile GL totals, trial balance, profit and loss, and balance sheet against the source reports. Existing maintenance/rebuild operations are intended for operational documents; imported journals have no corresponding operational documents.

Apply the migration to the intended database. If a database already has the earlier discarded implementation's migration, reconcile its migration history and schema before applying a newly generated migration with the same purpose.
