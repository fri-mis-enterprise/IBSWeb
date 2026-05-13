using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedDateToComparativeQueues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "updated_date",
                table: "filpride_sales_locked_records_queues",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "updated_date",
                table: "filpride_purchase_locked_records_queues",
                type: "date",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE filpride_sales_locked_records_queues
                SET updated_date = (locked_date + INTERVAL '1 month')::date
                WHERE updated_date IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE filpride_purchase_locked_records_queues
                SET updated_date = (locked_date + INTERVAL '1 month')::date
                WHERE updated_date IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_locked_records_queues_updated_date",
                table: "filpride_sales_locked_records_queues",
                column: "updated_date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_purchase_locked_records_queues_updated_date",
                table: "filpride_purchase_locked_records_queues",
                column: "updated_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_filpride_sales_locked_records_queues_updated_date",
                table: "filpride_sales_locked_records_queues");

            migrationBuilder.DropIndex(
                name: "ix_filpride_purchase_locked_records_queues_updated_date",
                table: "filpride_purchase_locked_records_queues");

            migrationBuilder.DropColumn(
                name: "updated_date",
                table: "filpride_sales_locked_records_queues");

            migrationBuilder.DropColumn(
                name: "updated_date",
                table: "filpride_purchase_locked_records_queues");
        }
    }
}
