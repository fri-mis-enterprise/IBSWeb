# GL import

This workflow imports GL entries against existing chart-of-accounts, supplier, and customer records. Imports can append journal entries to an empty or populated GL. The importer never clears existing data or updates operational balances.

Sign in as an Admin and open **Utility > GL Import**. Download the template, preview the file, correct any errors, then confirm the complete batch.

## File format

```csv
Date,Reference,Description,AccountNo,AccountName,Debit,Credit
2024-01-15,LEG-2024-0001,Illustrative legacy journal,SAMPLE-CASH,Legacy cash,1000.0000,0.0000
2024-01-15,LEG-2024-0001,Illustrative legacy journal,SAMPLE-REVENUE,Legacy revenue,0.0000,1000.0000
```

Replace the sample codes with existing visible posting accounts without children. Save as UTF-8 CSV; a UTF-8 BOM is supported. Preserve leading zeros in account codes. The headers and column order must match exactly.

Each row is one GL line. A journal is identified by its date and reference and must balance exactly. Use `yyyy-MM-dd` dates, positive amounts on one side, and zero on the other. Amounts must fit `numeric(18,4)`, with no currency symbols or thousands separators. Do not include totals or subtotal rows.

Reference is limited to 20 characters, AccountNo to 50, and AccountName to 200. All text fields are required. AccountName must exactly match the chart-of-accounts name for AccountNo, including capitalization; leading and trailing CSV whitespace is trimmed before comparison. This check runs during preview and confirmation. AccountNo resolves the internal account ID, and the chart of accounts supplies financial classifications. No accounts, invoices, vouchers, operational balances, sub-account records, or inventory transactions are created.

### Supplier and customer sub-accounts

The original seven-column format remains supported. Alternatively, download the GL template with sub-accounts:

```csv
Date,Reference,Description,AccountNo,AccountName,Debit,Credit,SubAccountType,SubAccountId
2024-01-15,LEG-2024-0002,Illustrative supplier journal,SAMPLE-EXPENSE,Legacy expense,1000.0000,0.0000,,
2024-01-15,LEG-2024-0002,Illustrative supplier journal,SAMPLE-PAYABLE,Legacy payable,0.0000,1000.0000,Supplier,123
```

Replace the account codes, names, and ID with actual registered records. `SubAccountType` must be exactly `Supplier` or `Customer`; `SubAccountId` must be a positive integer for an active record of that type. Supply both fields or leave both blank. The same numeric ID may identify different supplier and customer records, so type is required. Bank/company types and numeric enum values are not accepted by this template.

Use the existing supplier/customer database ID as `SubAccountId`. GL import is independent of the format or process used to create those master records.

Preview resolves and displays registered names. Confirmation checks activity/existence again and stores the current name with the type and ID in the GL. The importer does not create master records or populate document-level counterparty fields. Seven-column rows and nine-column rows with blank pairs have no sub-account.

A file may contain at most 50,000 GL lines and 10 MB. Split larger files at journal boundaries. Preview shows up to 100 valid lines and 100 errors; any error blocks the entire batch. The original file stays in the uploading user's server-side preview cache for up to 15 minutes. Restarting the server or evicting the cache requires another upload.

## Posting and history

Confirmation revalidates the original file and current database state. Identical files and existing date/reference journals are rejected. Concurrent imports are serialized, and the file hash has a unique database index. Batch metadata, GL rows, and the audit entry are saved in one transaction.

History retains the batch ID, filename, hash, importing user and time, date range, line and journal counts, and debit/credit totals. Each GL line has a batch link. The original file is not retained after confirmation or cache expiry. No batch deletion or replacement action is provided.

## Financial reports

Supply the complete agreed coverage, including opening entries when required. Do not combine cumulative balances with the underlying movements for the same period.

Closed balances at or after the first imported date block the file: reopen affected and later closed months before adding earlier entries. After importing all entries, lock and close months in chronological order. The existing closure sequence requires preceding months in each fiscal year to be closed, including empty months before the first imported month.

The existing trial balance uses stored period balances, and some financial reports use stored monthly earnings. Importing alone does not generate these snapshots. Reconcile GL totals, trial balance, profit and loss, and balance sheet against the source reports. Existing maintenance/rebuild operations are intended for operational documents; imported journals have no corresponding operational documents.

Apply the migration to the intended database. If a database already has the earlier discarded implementation's migration, reconcile its migration history and schema before applying a newly generated migration with the same purpose.

## GL import verification

- Preview and import both seven-column and nine-column files, including blank sub-account pairs and supplier/customer records sharing the same numeric ID.
- Missing/inactive IDs, wrong types, incomplete pairs, account-name mismatches, unbalanced journals, closed periods, and duplicate journals must block the whole batch. Recheck changes to master records between preview and confirmation.
- Verify both template downloads as Admin and confirm non-Admins cannot access the upload actions.
- Reconcile imported GL totals and supplier/customer subsidiary movements. After closing months chronologically, verify financial reports and following-month subsidiary openings, including PDF and Excel outputs.
