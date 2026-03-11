using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsValidInCalculatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_valid",
                table: "filpride_monthly_nibits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_valid",
                table: "filpride_gl_sub_account_balances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_valid",
                table: "filpride_gl_period_balances",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
