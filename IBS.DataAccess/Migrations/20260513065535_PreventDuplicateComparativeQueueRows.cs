using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateComparativeQueueRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_filpride_sales_locked_records_queues_delivery_receipt_id",
                table: "filpride_sales_locked_records_queues");

            migrationBuilder.DropIndex(
                name: "ix_filpride_purchase_locked_records_queues_receiving_report_id",
                table: "filpride_purchase_locked_records_queues");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_locked_records_queues_delivery_receipt_id",
                table: "filpride_sales_locked_records_queues",
                column: "delivery_receipt_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_purchase_locked_records_queues_receiving_report_id",
                table: "filpride_purchase_locked_records_queues",
                column: "receiving_report_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_filpride_sales_locked_records_queues_delivery_receipt_id",
                table: "filpride_sales_locked_records_queues");

            migrationBuilder.DropIndex(
                name: "ix_filpride_purchase_locked_records_queues_receiving_report_id",
                table: "filpride_purchase_locked_records_queues");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_locked_records_queues_delivery_receipt_id",
                table: "filpride_sales_locked_records_queues",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_purchase_locked_records_queues_receiving_report_id",
                table: "filpride_purchase_locked_records_queues",
                column: "receiving_report_id");
        }
    }
}
