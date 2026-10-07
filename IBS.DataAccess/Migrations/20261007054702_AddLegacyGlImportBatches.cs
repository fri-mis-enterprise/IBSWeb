using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyGlImportBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "import_batch_id",
                table: "filpride_general_ledger_books",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "filpride_gl_import_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    imported_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    imported_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    row_count = table.Column<int>(type: "integer", nullable: false),
                    journal_count = table.Column<int>(type: "integer", nullable: false),
                    date_from = table.Column<DateOnly>(type: "date", nullable: false),
                    date_to = table.Column<DateOnly>(type: "date", nullable: false),
                    total_debit = table.Column<decimal>(type: "numeric(28,4)", nullable: false),
                    total_credit = table.Column<decimal>(type: "numeric(28,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_gl_import_batches", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_filpride_general_ledger_books_import_batch_id",
                table: "filpride_general_ledger_books",
                column: "import_batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_gl_import_batches_file_hash",
                table: "filpride_gl_import_batches",
                column: "file_hash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_general_ledger_books_filpride_gl_import_batches_im",
                table: "filpride_general_ledger_books",
                column: "import_batch_id",
                principalTable: "filpride_gl_import_batches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_filpride_general_ledger_books_filpride_gl_import_batches_im",
                table: "filpride_general_ledger_books");

            migrationBuilder.DropTable(
                name: "filpride_gl_import_batches");

            migrationBuilder.DropIndex(
                name: "ix_filpride_general_ledger_books_import_batch_id",
                table: "filpride_general_ledger_books");

            migrationBuilder.DropColumn(
                name: "import_batch_id",
                table: "filpride_general_ledger_books");
        }
    }
}
