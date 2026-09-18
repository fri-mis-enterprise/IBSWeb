using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralLedgerCounterparty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "counterparty_id",
                table: "filpride_general_ledger_books",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "counterparty_name",
                table: "filpride_general_ledger_books",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "counterparty_type",
                table: "filpride_general_ledger_books",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_filpride_general_ledger_books_counterparty_type_counterpart",
                table: "filpride_general_ledger_books");

            migrationBuilder.DropColumn(
                name: "counterparty_id",
                table: "filpride_general_ledger_books");

            migrationBuilder.DropColumn(
                name: "counterparty_name",
                table: "filpride_general_ledger_books");

            migrationBuilder.DropColumn(
                name: "counterparty_type",
                table: "filpride_general_ledger_books");
        }
    }
}
