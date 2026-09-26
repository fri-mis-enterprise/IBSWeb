using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleServiceInvoiceCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "multiple_sv",
                table: "filpride_collection_receipts",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "multiple_sv_id",
                table: "filpride_collection_receipts",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<decimal[]>(
                name: "sv_multiple_amount",
                table: "filpride_collection_receipts",
                type: "numeric[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "multiple_sv",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "multiple_sv_id",
                table: "filpride_collection_receipts");

            migrationBuilder.DropColumn(
                name: "sv_multiple_amount",
                table: "filpride_collection_receipts");
        }
    }
}
