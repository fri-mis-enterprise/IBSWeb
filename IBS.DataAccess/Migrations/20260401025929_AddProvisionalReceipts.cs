using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProvisionalReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "filpride_provisional_receipts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    series_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: false),
                    cash_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    check_no = table.Column<string>(type: "text", nullable: true),
                    check_bank = table.Column<string>(type: "text", nullable: true),
                    check_branch = table.Column<string>(type: "text", nullable: true),
                    managers_check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    managers_check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    managers_check_no = table.Column<string>(type: "text", nullable: true),
                    managers_check_bank = table.Column<string>(type: "text", nullable: true),
                    managers_check_branch = table.Column<string>(type: "text", nullable: true),
                    bank_id = table.Column<int>(type: "integer", nullable: true),
                    bank_account_no = table.Column<string>(type: "text", nullable: true),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    ewt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    wvat = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    deposited_date = table.Column<DateOnly>(type: "date", nullable: true),
                    cleared_date = table.Column<DateOnly>(type: "date", nullable: true),
                    batch_number = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_provisional_receipts", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_provisional_receipts_filpride_bank_accounts_bank_id",
                        column: x => x.bank_id,
                        principalTable: "filpride_bank_accounts",
                        principalColumn: "bank_account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_provisional_receipts_filpride_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "filpride_employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_bank_id",
                table: "filpride_provisional_receipts",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_company",
                table: "filpride_provisional_receipts",
                column: "company");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_employee_id",
                table: "filpride_provisional_receipts",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_series_number",
                table: "filpride_provisional_receipts",
                column: "series_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "filpride_provisional_receipts");

            migrationBuilder.DropColumn(
                name: "is_valid",
                table: "filpride_monthly_nibits");

            migrationBuilder.DropColumn(
                name: "is_valid",
                table: "filpride_gl_sub_account_balances");

            migrationBuilder.DropColumn(
                name: "is_valid",
                table: "filpride_gl_period_balances");
        }
    }
}
