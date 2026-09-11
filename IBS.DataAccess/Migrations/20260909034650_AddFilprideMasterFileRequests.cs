using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFilprideMasterFileRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "filpride_master_file_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    master_file_type = table.Column<int>(type: "integer", nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    requested_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    requested_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    requested_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    approved_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_master_file_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_filpride_master_file_requests_master_file_type_requested_da",
                table: "filpride_master_file_requests",
                columns: new[] { "master_file_type", "requested_date" });

            migrationBuilder.CreateIndex(
                name: "ix_filpride_master_file_requests_requested_by_requested_date",
                table: "filpride_master_file_requests",
                columns: new[] { "requested_by", "requested_date" });

            migrationBuilder.CreateIndex(
                name: "ix_filpride_master_file_requests_status_requested_date",
                table: "filpride_master_file_requests",
                columns: new[] { "status", "requested_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "filpride_master_file_requests");
        }
    }
}
