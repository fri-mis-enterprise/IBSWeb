using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedTheProvisionalReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_filpride_provisional_receipts_filpride_employees_employee_id",
                table: "filpride_provisional_receipts");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "filpride_provisional_receipts",
                newName: "supplier_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_provisional_receipts_employee_id",
                table: "filpride_provisional_receipts",
                newName: "ix_filpride_provisional_receipts_supplier_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_provisional_receipts_filpride_suppliers_supplier_id",
                table: "filpride_provisional_receipts",
                column: "supplier_id",
                principalTable: "filpride_suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_filpride_provisional_receipts_filpride_suppliers_supplier_id",
                table: "filpride_provisional_receipts");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                table: "filpride_provisional_receipts",
                newName: "employee_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_provisional_receipts_supplier_id",
                table: "filpride_provisional_receipts",
                newName: "ix_filpride_provisional_receipts_employee_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_provisional_receipts_filpride_employees_employee_id",
                table: "filpride_provisional_receipts",
                column: "employee_id",
                principalTable: "filpride_employees",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
