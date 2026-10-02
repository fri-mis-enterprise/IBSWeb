using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSalesInvoiceTaxPaidFlagWithBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_tax_and_vat_paid",
                table: "filpride_sales_invoices");

            migrationBuilder.AddColumn<decimal>(
                name: "cw_vat_amount_paid",
                table: "filpride_sales_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cw_vat_balance",
                table: "filpride_sales_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cwt_amount_paid",
                table: "filpride_sales_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cwt_balance",
                table: "filpride_sales_invoices",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ewt",
                table: "filpride_collection_receipt_details",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "wvat",
                table: "filpride_collection_receipt_details",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                WITH invoice_tax AS
                (
                    SELECT
                        invoice.sales_invoice_id,
                        CASE WHEN COALESCE(cos.has_ewt, customer.with_holding_tax, false) THEN
                            ROUND((CASE WHEN COALESCE(cos.vat_type, customer.vat_type) = 'Vatable'
                                THEN ROUND((invoice.amount - invoice.discount) / 1.12, 4)
                                ELSE ROUND(invoice.amount - invoice.discount, 4)
                            END) * invoice.cwt_percent, 4)
                        ELSE 0 END AS cwt_amount,
                        CASE WHEN COALESCE(cos.has_wvat, customer.with_holding_vat, false) THEN
                            ROUND((CASE WHEN COALESCE(cos.vat_type, customer.vat_type) = 'Vatable'
                                THEN ROUND((invoice.amount - invoice.discount) / 1.12, 4)
                                ELSE ROUND(invoice.amount - invoice.discount, 4)
                            END) * invoice.cw_vat_percent, 4)
                        ELSE 0 END AS cw_vat_amount
                    FROM filpride_sales_invoices AS invoice
                    INNER JOIN filpride_customers AS customer
                        ON customer.customer_id = invoice.customer_id
                    LEFT JOIN filpride_customer_order_slips AS cos
                        ON cos.customer_order_slip_id = invoice.customer_order_slip_id
                )
                UPDATE filpride_sales_invoices AS invoice
                SET cwt_balance = tax.cwt_amount,
                    cw_vat_balance = tax.cw_vat_amount
                FROM invoice_tax AS tax
                WHERE tax.sales_invoice_id = invoice.sales_invoice_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cw_vat_amount_paid",
                table: "filpride_sales_invoices");

            migrationBuilder.DropColumn(
                name: "cw_vat_balance",
                table: "filpride_sales_invoices");

            migrationBuilder.DropColumn(
                name: "cwt_amount_paid",
                table: "filpride_sales_invoices");

            migrationBuilder.DropColumn(
                name: "cwt_balance",
                table: "filpride_sales_invoices");

            migrationBuilder.DropColumn(
                name: "ewt",
                table: "filpride_collection_receipt_details");

            migrationBuilder.DropColumn(
                name: "wvat",
                table: "filpride_collection_receipt_details");

            migrationBuilder.AddColumn<bool>(
                name: "is_tax_and_vat_paid",
                table: "filpride_sales_invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
