# GL import counterparty mock data

Use these files with the twelve-column GL import template. Dates are October 1-16, 2026. Company is the illustrative free-text name `Example Trading Company`. Account names and customer/supplier IDs were checked against the current isolated `ibs_gl_dev` database on October 19, 2026. Amounts and journals are fictional.

## Files

- `01-valid-counterparties.csv`: 18 GL lines in 9 balanced journals. Total debit and credit are each **78250.4321**. Includes opening capital, purchase, sale, customer collection, supplier payment, four-decimal cash expense, cash sale without sub-account, sales freight with a supplier sub-account/customer counterparty, and a journal without counterparty.
- `02-invalid-counterparty-pairs.csv`: 12 lines expected to fail with 12 pair-validation errors plus the summary that no valid GL lines remain (13 messages). Covers type-only, ID-only, numeric type, zero ID, negative ID, and unsupported BankAccount type. Do not confirm this file.
- `03-invalid-missing-counterparties.csv`: 4 balanced lines expected to fail with 4 missing-record errors. Customer and supplier ID 1408 were absent when generated. Do not confirm this file.

## How to use

1. Upload `01-valid-counterparties.csv` to GL Import and inspect the preview. Valid preview should show 18 lines, 9 journals, zero errors, and resolved names for populated counterparty/sub-account pairs.
2. If testing import, confirm that file once. Existing date/reference journals or a previously imported identical file are rejected. No data has been imported automatically.
3. Upload the invalid files separately to check rejection messages. A single invalid line blocks the complete file.

Users enter only CounterpartyType and CounterpartyId. The importer resolves CounterpartyName; it is not an input column. For journals with a counterparty, the sample repeats it on every line, including cash/revenue/expense lines without a sub-account. Both fields are blank on the opening and general adjustment journals.

The freight journal `MC261019-008` uses customer ID 3 as counterparty on both lines, while its payable line uses supplier ID 3 as sub-account. These identities and names must resolve independently. Same-number customer and supplier IDs also remain distinct by type.

## Referenced master records

| Type | Database ID | Expected resolved name at generation |
| --- | --- | --- |
| Customer | 1 | U88RTM GASOLINE STATION |
| Customer | 2 | MACHINE & COMPUTER WORLD INC |
| Customer | 3 | MOBILITY GROUP CORP.- CALATAGAN STATION |
| Supplier | 1 | FILPRIDE RESOURCES INC. |
| Supplier | 2 | CHEMICAL ALLOY CORPORATION |
| Supplier | 3 | CHEMREZ TECHNOLOGIES INC |

Preview and confirmation validate current records. Master renames appear in resolved names; missing/inactive records block the batch. Closed periods or existing journals can also block these files after generation. If using another database, replace account codes/names and master IDs with its registered active records.

## Expected balances from the valid file alone

| Account | Debit movement | Credit movement | Net movement |
| --- | ---: | ---: | --- |
| Cash on Hand | 61000.0000 | 2500.4321 | 58499.5679 debit |
| AR-TRADE RECEIVABLE | 12000.0000 | 7500.0000 | 4500.0000 debit |
| VOUCHERS PAYABLE -FRI | 1500.0000 | 2750.0000 | 1250.0000 credit |
| Retained Earnings - FRI | 0.0000 | 50000.0000 | 50000.0000 credit |
| SALES-BIODIESEL | 0.0000 | 15500.0000 | 15500.0000 credit |
| Office Supplies | 3500.4321 | 0.0000 | 3500.4321 debit |
| FREIGHT OUT - Biodiesel | 250.0000 | 0.0000 | 250.0000 debit |

These are test movements, not existing database balances. No operational invoice, voucher, inventory, or customer/supplier operational balance is created by GL import.
