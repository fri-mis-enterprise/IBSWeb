using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckVoucherDocumentationCompanyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "documented_by_company_name",
                table: "filpride_check_voucher_headers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_documented_by_other_company",
                table: "filpride_check_voucher_headers",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "documented_by_company_name",
                table: "filpride_check_voucher_headers");

            migrationBuilder.DropColumn(
                name: "is_documented_by_other_company",
                table: "filpride_check_voucher_headers");
        }
    }
}
