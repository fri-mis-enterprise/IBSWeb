using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCheckVoucherEmployeeIdWithEmployeeAdvanceFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_employee_advance",
                table: "filpride_check_voucher_headers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE filpride_check_voucher_headers
                SET is_employee_advance = TRUE
                WHERE employee_id IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_check_voucher_headers_filpride_employees_employee_",
                table: "filpride_check_voucher_headers");

            migrationBuilder.DropIndex(
                name: "ix_filpride_check_voucher_headers_employee_id",
                table: "filpride_check_voucher_headers");

            migrationBuilder.DropColumn(
                name: "employee_id",
                table: "filpride_check_voucher_headers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_employee_advance",
                table: "filpride_check_voucher_headers");

            migrationBuilder.AddColumn<int>(
                name: "employee_id",
                table: "filpride_check_voucher_headers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_check_voucher_headers_employee_id",
                table: "filpride_check_voucher_headers",
                column: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_check_voucher_headers_filpride_employees_employee_",
                table: "filpride_check_voucher_headers",
                column: "employee_id",
                principalTable: "filpride_employees",
                principalColumn: "employee_id");
        }
    }
}
