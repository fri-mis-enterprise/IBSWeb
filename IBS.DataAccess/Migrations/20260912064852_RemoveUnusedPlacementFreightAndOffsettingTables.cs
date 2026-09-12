using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedPlacementFreightAndOffsettingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bienes_placements");

            migrationBuilder.DropTable(
                name: "filpride_freights");

            migrationBuilder.DropTable(
                name: "filpride_offsettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bienes_placements",
                columns: table => new
                {
                    placement_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<int>(type: "integer", nullable: false),
                    rolled_from_id = table.Column<int>(type: "integer", nullable: true),
                    settlement_account_id = table.Column<int>(type: "integer", nullable: false),
                    swapped_from_id = table.Column<int>(type: "integer", nullable: true),
                    account_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    bank = table.Column<string>(type: "varchar(20)", nullable: false),
                    batch_number = table.Column<string>(type: "varchar(50)", nullable: true),
                    branch = table.Column<string>(type: "varchar(100)", nullable: false),
                    cv_no = table.Column<string>(type: "varchar(100)", nullable: false),
                    cheque_number = table.Column<string>(type: "varchar(100)", nullable: false),
                    @class = table.Column<string>(name: "class", type: "varchar(10)", nullable: false),
                    control_number = table.Column<string>(type: "varchar(20)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date_from = table.Column<DateOnly>(type: "date", nullable: false),
                    date_to = table.Column<DateOnly>(type: "date", nullable: false),
                    disposition = table.Column<string>(type: "varchar(5)", nullable: false),
                    ewt_rate = table.Column<decimal>(type: "numeric(7,4)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: false),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    frequency_of_payment = table.Column<string>(type: "varchar(20)", nullable: true),
                    has_ewt = table.Column<bool>(type: "boolean", nullable: false),
                    has_trust_fee = table.Column<bool>(type: "boolean", nullable: false),
                    interest_deposited = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    interest_deposited_date = table.Column<DateOnly>(type: "date", nullable: true),
                    interest_deposited_to = table.Column<string>(type: "varchar(100)", nullable: true),
                    interest_rate = table.Column<decimal>(type: "numeric(13,10)", nullable: false),
                    interest_status = table.Column<string>(type: "varchar(50)", nullable: true),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    is_posted = table.Column<bool>(type: "boolean", nullable: false),
                    is_rolled = table.Column<bool>(type: "boolean", nullable: false),
                    is_swapped = table.Column<bool>(type: "boolean", nullable: false),
                    locked_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    number_of_years = table.Column<int>(type: "integer", nullable: false),
                    placement_type = table.Column<int>(type: "integer", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    principal_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    principal_disposition = table.Column<string>(type: "varchar(100)", nullable: true),
                    remarks = table.Column<string>(type: "varchar(255)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", nullable: false),
                    td_account_number = table.Column<string>(type: "varchar(50)", nullable: false),
                    terminated_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    terminated_date = table.Column<DateOnly>(type: "date", nullable: true),
                    termination_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    trust_fee_rate = table.Column<decimal>(type: "numeric(11,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bienes_placements", x => x.placement_id);
                    table.ForeignKey(
                        name: "fk_bienes_placements_bienes_placements_rolled_from_id",
                        column: x => x.rolled_from_id,
                        principalTable: "bienes_placements",
                        principalColumn: "placement_id");
                    table.ForeignKey(
                        name: "fk_bienes_placements_bienes_placements_swapped_from_id",
                        column: x => x.swapped_from_id,
                        principalTable: "bienes_placements",
                        principalColumn: "placement_id");
                    table.ForeignKey(
                        name: "fk_bienes_placements_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bienes_placements_filpride_bank_accounts_bank_id",
                        column: x => x.bank_id,
                        principalTable: "filpride_bank_accounts",
                        principalColumn: "bank_account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bienes_placements_filpride_bank_accounts_settlement_account",
                        column: x => x.settlement_account_id,
                        principalTable: "filpride_bank_accounts",
                        principalColumn: "bank_account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "filpride_freights",
                columns: table => new
                {
                    freight_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pick_up_point_id = table.Column<int>(type: "integer", nullable: false),
                    cluster_code = table.Column<int>(type: "integer", nullable: false),
                    freight = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_freights", x => x.freight_id);
                    table.ForeignKey(
                        name: "fk_filpride_freights_filpride_pick_up_points_pick_up_point_id",
                        column: x => x.pick_up_point_id,
                        principalTable: "filpride_pick_up_points",
                        principalColumn: "pick_up_point_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "filpride_offsettings",
                columns: table => new
                {
                    off_setting_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_no = table.Column<string>(type: "text", nullable: false),
                    account_title = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    reference = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_offsettings", x => x.off_setting_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bienes_placements_bank_id",
                table: "bienes_placements",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_bienes_placements_company_id",
                table: "bienes_placements",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_bienes_placements_control_number",
                table: "bienes_placements",
                column: "control_number");

            migrationBuilder.CreateIndex(
                name: "ix_bienes_placements_rolled_from_id",
                table: "bienes_placements",
                column: "rolled_from_id");

            migrationBuilder.CreateIndex(
                name: "ix_bienes_placements_settlement_account_id",
                table: "bienes_placements",
                column: "settlement_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_bienes_placements_swapped_from_id",
                table: "bienes_placements",
                column: "swapped_from_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_freights_pick_up_point_id",
                table: "filpride_freights",
                column: "pick_up_point_id");
        }
    }
}
