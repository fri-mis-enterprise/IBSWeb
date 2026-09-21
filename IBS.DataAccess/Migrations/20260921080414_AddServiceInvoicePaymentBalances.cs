using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceInvoicePaymentBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "credit_amount",
                table: "filpride_service_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cw_vat_amount_paid",
                table: "filpride_service_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cw_vat_balance",
                table: "filpride_service_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cwt_amount_paid",
                table: "filpride_service_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cwt_balance",
                table: "filpride_service_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "debit_amount",
                table: "filpride_service_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE filpride_collection_receipt_details AS detail
                SET ewt = receipt.ewt,
                    wvat = receipt.wvat
                FROM filpride_collection_receipts AS receipt
                WHERE detail.collection_receipt_id = receipt.collection_receipt_id
                  AND receipt.service_invoice_id IS NOT NULL;

                UPDATE filpride_service_invoices AS invoice
                SET debit_amount = COALESCE((
                        SELECT SUM(memo.debit_amount)
                        FROM filpride_debit_memos AS memo
                        WHERE memo.service_invoice_id = invoice.service_invoice_id
                          AND memo.status = 'Posted'
                          AND memo.posted_by IS NOT NULL
                    ), 0),
                    credit_amount = COALESCE((
                        SELECT SUM(ABS(memo.credit_amount))
                        FROM filpride_credit_memos AS memo
                        WHERE memo.service_invoice_id = invoice.service_invoice_id
                          AND memo.status = 'Posted'
                          AND memo.posted_by IS NOT NULL
                    ), 0);

                UPDATE filpride_service_invoices AS invoice
                SET cwt_amount_paid = COALESCE((
                        SELECT SUM(detail.ewt)
                        FROM filpride_collection_receipt_details AS detail
                        INNER JOIN filpride_collection_receipts AS receipt
                            ON receipt.collection_receipt_id = detail.collection_receipt_id
                        WHERE receipt.service_invoice_id = invoice.service_invoice_id
                          AND detail.invoice_no = invoice.service_invoice_no
                          AND receipt.status NOT IN ('Canceled', 'Voided')
                    ), 0),
                    cw_vat_amount_paid = COALESCE((
                        SELECT SUM(detail.wvat)
                        FROM filpride_collection_receipt_details AS detail
                        INNER JOIN filpride_collection_receipts AS receipt
                            ON receipt.collection_receipt_id = detail.collection_receipt_id
                        WHERE receipt.service_invoice_id = invoice.service_invoice_id
                          AND detail.invoice_no = invoice.service_invoice_no
                          AND receipt.status NOT IN ('Canceled', 'Voided')
                    ), 0);

                UPDATE filpride_service_invoices AS invoice
                SET cwt_balance = CASE WHEN invoice.has_ewt THEN
                        ROUND((CASE WHEN invoice.vat_type = 'Vatable'
                            THEN (invoice.total - invoice.discount + invoice.debit_amount - invoice.credit_amount) / 1.12
                            ELSE invoice.total - invoice.discount + invoice.debit_amount - invoice.credit_amount
                        END) * (invoice.service_percent / 100), 4) - invoice.cwt_amount_paid
                    ELSE 0 END,
                    cw_vat_balance = CASE WHEN invoice.has_wvat THEN
                        ROUND((CASE WHEN invoice.vat_type = 'Vatable'
                            THEN (invoice.total - invoice.discount + invoice.debit_amount - invoice.credit_amount) / 1.12
                            ELSE invoice.total - invoice.discount + invoice.debit_amount - invoice.credit_amount
                        END) * 0.05, 4) - invoice.cw_vat_amount_paid
                    ELSE 0 END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "credit_amount",
                table: "filpride_service_invoices");

            migrationBuilder.DropColumn(
                name: "cw_vat_amount_paid",
                table: "filpride_service_invoices");

            migrationBuilder.DropColumn(
                name: "cw_vat_balance",
                table: "filpride_service_invoices");

            migrationBuilder.DropColumn(
                name: "cwt_amount_paid",
                table: "filpride_service_invoices");

            migrationBuilder.DropColumn(
                name: "cwt_balance",
                table: "filpride_service_invoices");

            migrationBuilder.DropColumn(
                name: "debit_amount",
                table: "filpride_service_invoices");
        }
    }
}
