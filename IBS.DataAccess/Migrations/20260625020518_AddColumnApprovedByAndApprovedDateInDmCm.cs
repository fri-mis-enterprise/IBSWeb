using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnApprovedByAndApprovedDateInDmCm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "approved_by",
                table: "filpride_debit_memos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "approved_date",
                table: "filpride_debit_memos",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "approved_by",
                table: "filpride_credit_memos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "approved_date",
                table: "filpride_credit_memos",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "approved_by",
                table: "filpride_debit_memos");

            migrationBuilder.DropColumn(
                name: "approved_date",
                table: "filpride_debit_memos");

            migrationBuilder.DropColumn(
                name: "approved_by",
                table: "filpride_credit_memos");

            migrationBuilder.DropColumn(
                name: "approved_date",
                table: "filpride_credit_memos");
        }
    }
}
