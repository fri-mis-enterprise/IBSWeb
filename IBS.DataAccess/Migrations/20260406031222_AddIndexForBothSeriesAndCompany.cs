using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForBothSeriesAndCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_filpride_provisional_receipts_company",
                table: "filpride_provisional_receipts");

            migrationBuilder.DropIndex(
                name: "ix_filpride_provisional_receipts_series_number",
                table: "filpride_provisional_receipts");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_series_number_company",
                table: "filpride_provisional_receipts",
                columns: new[] { "series_number", "company" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_filpride_provisional_receipts_series_number_company",
                table: "filpride_provisional_receipts");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_company",
                table: "filpride_provisional_receipts",
                column: "company");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_series_number",
                table: "filpride_provisional_receipts",
                column: "series_number");
        }
    }
}
