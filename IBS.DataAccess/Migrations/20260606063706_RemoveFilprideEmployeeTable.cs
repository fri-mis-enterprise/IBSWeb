using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFilprideEmployeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "filpride_employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "filpride_employees",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    date_hired = table.Column<DateOnly>(type: "date", nullable: false),
                    date_resigned = table.Column<DateOnly>(type: "date", nullable: true),
                    department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    employee_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    initial = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_managerial = table.Column<bool>(type: "boolean", nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pagibig_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    paygrade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    philhealth_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    sss_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    suffix = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    supervisor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tel_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tin_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_employees", x => x.employee_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_filpride_employees_employee_number",
                table: "filpride_employees",
                column: "employee_number");
        }
    }
}
