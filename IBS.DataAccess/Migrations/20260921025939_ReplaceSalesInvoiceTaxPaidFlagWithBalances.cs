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
