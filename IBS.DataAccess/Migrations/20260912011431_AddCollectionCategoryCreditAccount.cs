using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionCategoryCreditAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "credit_account_id",
                table: "filpride_collection_categories",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_categories_credit_account_id",
                table: "filpride_collection_categories",
                column: "credit_account_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_collection_categories_filpride_chart_of_accounts_c",
                table: "filpride_collection_categories",
                column: "credit_account_id",
                principalTable: "filpride_chart_of_accounts",
                principalColumn: "account_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_filpride_collection_categories_filpride_chart_of_accounts_c",
                table: "filpride_collection_categories");

            migrationBuilder.DropIndex(
                name: "ix_filpride_collection_categories_credit_account_id",
                table: "filpride_collection_categories");

            migrationBuilder.DropColumn(
                name: "credit_account_id",
                table: "filpride_collection_categories");
        }
    }
}
