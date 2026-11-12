using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewColumnsForHaulerDetailsInDeliveryReceiptTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "hauler_invoice_amount",
                table: "filpride_delivery_receipts",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "hauler_invoice_date",
                table: "filpride_delivery_receipts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hauler_invoice_no",
                table: "filpride_delivery_receipts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hauler_invoice_amount",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropColumn(
                name: "hauler_invoice_date",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropColumn(
                name: "hauler_invoice_no",
                table: "filpride_delivery_receipts");
        }
    }
}
