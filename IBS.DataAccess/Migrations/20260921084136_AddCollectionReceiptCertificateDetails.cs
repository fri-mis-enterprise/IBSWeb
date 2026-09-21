using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionReceiptCertificateDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "cw_vat_period_from",
                table: "filpride_collection_receipts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "cw_vat_period_to",
                table: "filpride_collection_receipts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cw_vat_reference1",
                table: "filpride_collection_receipts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cw_vat_reference2",
                table: "filpride_collection_receipts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ewt_period_from",
                table: "filpride_collection_receipts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ewt_period_to",
                table: "filpride_collection_receipts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ewt_reference1",
                table: "filpride_collection_receipts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ewt_reference2",
                table: "filpride_collection_receipts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cw_vat_period_from",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "cw_vat_period_to",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "cw_vat_reference1",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "cw_vat_reference2",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "ewt_period_from",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "ewt_period_to",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "ewt_reference1",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "ewt_reference2",
                table: "filpride_collection_receipts");
        }
    }
}
