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
                UPDATE filpride_service_invoices AS invoice
                SET cwt_balance = CASE WHEN invoice.has_ewt THEN
                        ROUND((CASE WHEN invoice.vat_type = 'Vatable'
                            THEN ROUND((invoice.total - invoice.discount) / 1.12, 4)
                            ELSE ROUND(invoice.total - invoice.discount, 4)
                        END) * (invoice.service_percent / 100), 4)
                    ELSE 0 END,
                    cw_vat_balance = CASE WHEN invoice.has_wvat THEN
                        ROUND((CASE WHEN invoice.vat_type = 'Vatable'
                            THEN ROUND((invoice.total - invoice.discount) / 1.12, 4)
                            ELSE ROUND(invoice.total - invoice.discount, 4)
                        END) * 0.05, 4)
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
