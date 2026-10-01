using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeReJournalProcessing: Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_filpride_general_ledger_books_reference",
                table: "filpride_general_ledger_books",
                column: "reference");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_check_voucher_headers_date",
                table: "filpride_check_voucher_headers",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_service_invoices_period",
                table: "filpride_service_invoices",
                column: "period");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_receiving_reports_date",
                table: "filpride_receiving_reports",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_transaction_date",
                table: "filpride_provisional_receipts",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_journal_voucher_headers_date",
                table: "filpride_journal_voucher_headers",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_inventories_reference",
                table: "filpride_inventories",
                column: "reference");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_delivered_date",
                table: "filpride_delivery_receipts",
                column: "delivered_date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_debit_memos_transaction_date",
                table: "filpride_debit_memos",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_credit_memos_transaction_date",
                table: "filpride_credit_memos",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipts_transaction_date",
                table: "filpride_collection_receipts",
                column: "transaction_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_filpride_general_ledger_books_reference",
                table: "filpride_general_ledger_books");

            migrationBuilder.DropIndex(
                name: "ix_filpride_check_voucher_headers_date",
                table: "filpride_check_voucher_headers");

            migrationBuilder.DropIndex(
                name: "ix_filpride_service_invoices_period",
                table: "filpride_service_invoices");

            migrationBuilder.DropIndex(
                name: "ix_filpride_receiving_reports_date",
                table: "filpride_receiving_reports");

            migrationBuilder.DropIndex(
                name: "ix_filpride_provisional_receipts_transaction_date",
                table: "filpride_provisional_receipts");

            migrationBuilder.DropIndex(
                name: "ix_filpride_journal_voucher_headers_date",
                table: "filpride_journal_voucher_headers");

            migrationBuilder.DropIndex(
                name: "ix_filpride_inventories_reference",
                table: "filpride_inventories");

            migrationBuilder.DropIndex(
                name: "ix_filpride_delivery_receipts_delivered_date",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropIndex(
                name: "ix_filpride_debit_memos_transaction_date",
                table: "filpride_debit_memos");

            migrationBuilder.DropIndex(
                name: "ix_filpride_credit_memos_transaction_date",
                table: "filpride_credit_memos");

            migrationBuilder.DropIndex(
                name: "ix_filpride_collection_receipts_transaction_date",
                table: "filpride_collection_receipts");
        }
    }
}
