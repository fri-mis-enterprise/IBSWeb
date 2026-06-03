using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateLockedPeriodAdjustment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "locked_period_adjustments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    entity_type = table.Column<int>(type: "integer", nullable: false),
                    entity_type_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    adjustment_type = table.Column<int>(type: "integer", nullable: false),
                    old_value = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    new_value = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    adjustment_value = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    reason = table.Column<string>(type: "varchar(100)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locked_period_adjustments", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "locked_period_adjustments");
        }
    }
}
