using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMMSIModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mmsi_dispatch_tickets");

            migrationBuilder.DropTable(
                name: "mmsi_tariff_rates");

            migrationBuilder.DropTable(
                name: "mmsi_user_accesses");

            migrationBuilder.DropTable(
                name: "mmsi_billings");

            migrationBuilder.DropTable(
                name: "mmsi_tug_masters");

            migrationBuilder.DropTable(
                name: "mmsi_tugboats");

            migrationBuilder.DropTable(
                name: "mmsi_services");

            migrationBuilder.DropTable(
                name: "mmsi_collections");

            migrationBuilder.DropTable(
                name: "mmsi_principals");

            migrationBuilder.DropTable(
                name: "mmsi_terminals");

            migrationBuilder.DropTable(
                name: "mmsi_vessels");

            migrationBuilder.DropTable(
                name: "mmsi_tugboat_owners");

            migrationBuilder.DropTable(
                name: "mmsi_ports");

            migrationBuilder.DropColumn(
                name: "is_mmsi",
                table: "filpride_customers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_mmsi",
                table: "filpride_customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "mmsi_collections",
                columns: table => new
                {
                    mmsi_collection_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    check_date = table.Column<DateOnly>(type: "date", nullable: false),
                    check_number = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    deposit_date = table.Column<DateOnly>(type: "date", nullable: false),
                    ewt = table.Column<decimal>(type: "numeric", nullable: false),
                    is_undocumented = table.Column<bool>(type: "boolean", nullable: false),
                    mmsi_collection_number = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_collections", x => x.mmsi_collection_id);
                    table.ForeignKey(
                        name: "fk_mmsi_collections_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_ports",
                columns: table => new
                {
                    port_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    has_sbma = table.Column<bool>(type: "boolean", nullable: false),
                    port_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    port_number = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_ports", x => x.port_id);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_principals",
                columns: table => new
                {
                    principal_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    address = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    business_type = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_vatable = table.Column<bool>(type: "boolean", nullable: false),
                    landline1 = table.Column<string>(type: "text", nullable: true),
                    landline2 = table.Column<string>(type: "text", nullable: true),
                    mobile1 = table.Column<string>(type: "text", nullable: true),
                    mobile2 = table.Column<string>(type: "text", nullable: true),
                    principal_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    principal_number = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    tin = table.Column<string>(type: "text", nullable: true),
                    terms = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_principals", x => x.principal_id);
                    table.ForeignKey(
                        name: "fk_mmsi_principals_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_services",
                columns: table => new
                {
                    service_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    service_number = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_services", x => x.service_id);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_tug_masters",
                columns: table => new
                {
                    tug_master_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tug_master_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    tug_master_number = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_tug_masters", x => x.tug_master_id);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_tugboat_owners",
                columns: table => new
                {
                    tugboat_owner_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fixed_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    tugboat_owner_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    tugboat_owner_number = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_tugboat_owners", x => x.tugboat_owner_id);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_user_accesses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    can_approve_tariff = table.Column<bool>(type: "boolean", nullable: false),
                    can_create_billing = table.Column<bool>(type: "boolean", nullable: false),
                    can_create_collection = table.Column<bool>(type: "boolean", nullable: false),
                    can_create_dispatch_ticket = table.Column<bool>(type: "boolean", nullable: false),
                    can_create_service_request = table.Column<bool>(type: "boolean", nullable: false),
                    can_post_service_request = table.Column<bool>(type: "boolean", nullable: false),
                    can_print_report = table.Column<bool>(type: "boolean", nullable: false),
                    can_set_tariff = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_user_accesses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_vessels",
                columns: table => new
                {
                    vessel_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vessel_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    vessel_number = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    vessel_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_vessels", x => x.vessel_id);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_terminals",
                columns: table => new
                {
                    terminal_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    port_id = table.Column<int>(type: "integer", nullable: false),
                    terminal_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    terminal_number = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_terminals", x => x.terminal_id);
                    table.ForeignKey(
                        name: "fk_mmsi_terminals_mmsi_ports_port_id",
                        column: x => x.port_id,
                        principalTable: "mmsi_ports",
                        principalColumn: "port_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_tugboats",
                columns: table => new
                {
                    tugboat_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tugboat_owner_id = table.Column<int>(type: "integer", nullable: true),
                    is_company_owned = table.Column<bool>(type: "boolean", nullable: false),
                    tugboat_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    tugboat_number = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_tugboats", x => x.tugboat_id);
                    table.ForeignKey(
                        name: "fk_mmsi_tugboats_mmsi_tugboat_owners_tugboat_owner_id",
                        column: x => x.tugboat_owner_id,
                        principalTable: "mmsi_tugboat_owners",
                        principalColumn: "tugboat_owner_id");
                });

            migrationBuilder.CreateTable(
                name: "mmsi_billings",
                columns: table => new
                {
                    mmsi_billing_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    port_id = table.Column<int>(type: "integer", nullable: true),
                    principal_id = table.Column<int>(type: "integer", nullable: true),
                    terminal_id = table.Column<int>(type: "integer", nullable: true),
                    vessel_id = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ap_other_tug = table.Column<decimal>(type: "numeric", nullable: false),
                    baf_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    billed_to = table.Column<string>(type: "varchar(10)", nullable: false),
                    collection_id = table.Column<int>(type: "integer", nullable: true),
                    collection_number = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    dispatch_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    is_principal = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    is_undocumented = table.Column<bool>(type: "boolean", nullable: false),
                    is_vatable = table.Column<bool>(type: "boolean", nullable: false),
                    last_edited_by = table.Column<string>(type: "text", nullable: true),
                    last_edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    mmsi_billing_number = table.Column<string>(type: "varchar(10)", nullable: false),
                    mmsi_collection_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    voyage_number = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_billings", x => x.mmsi_billing_id);
                    table.ForeignKey(
                        name: "fk_mmsi_billings_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "fk_mmsi_billings_mmsi_collections_mmsi_collection_id",
                        column: x => x.mmsi_collection_id,
                        principalTable: "mmsi_collections",
                        principalColumn: "mmsi_collection_id");
                    table.ForeignKey(
                        name: "fk_mmsi_billings_mmsi_ports_port_id",
                        column: x => x.port_id,
                        principalTable: "mmsi_ports",
                        principalColumn: "port_id");
                    table.ForeignKey(
                        name: "fk_mmsi_billings_mmsi_principals_principal_id",
                        column: x => x.principal_id,
                        principalTable: "mmsi_principals",
                        principalColumn: "principal_id");
                    table.ForeignKey(
                        name: "fk_mmsi_billings_mmsi_terminals_terminal_id",
                        column: x => x.terminal_id,
                        principalTable: "mmsi_terminals",
                        principalColumn: "terminal_id");
                    table.ForeignKey(
                        name: "fk_mmsi_billings_mmsi_vessels_vessel_id",
                        column: x => x.vessel_id,
                        principalTable: "mmsi_vessels",
                        principalColumn: "vessel_id");
                });

            migrationBuilder.CreateTable(
                name: "mmsi_tariff_rates",
                columns: table => new
                {
                    tariff_rate_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    terminal_id = table.Column<int>(type: "integer", nullable: false),
                    as_of_date = table.Column<DateOnly>(type: "date", nullable: false),
                    baf = table.Column<decimal>(type: "numeric", nullable: false),
                    baf_discount = table.Column<decimal>(type: "numeric", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    dispatch = table.Column<decimal>(type: "numeric", nullable: false),
                    dispatch_discount = table.Column<decimal>(type: "numeric", nullable: false),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_tariff_rates", x => x.tariff_rate_id);
                    table.ForeignKey(
                        name: "fk_mmsi_tariff_rates_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mmsi_tariff_rates_mmsi_services_service_id",
                        column: x => x.service_id,
                        principalTable: "mmsi_services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mmsi_tariff_rates_mmsi_terminals_terminal_id",
                        column: x => x.terminal_id,
                        principalTable: "mmsi_terminals",
                        principalColumn: "terminal_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mmsi_dispatch_tickets",
                columns: table => new
                {
                    dispatch_ticket_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    billing_id = table.Column<int>(type: "integer", nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    service_id = table.Column<int>(type: "integer", nullable: true),
                    terminal_id = table.Column<int>(type: "integer", nullable: true),
                    tug_boat_id = table.Column<int>(type: "integer", nullable: true),
                    tug_master_id = table.Column<int>(type: "integer", nullable: true),
                    vessel_id = table.Column<int>(type: "integer", nullable: true),
                    ap_other_tugs = table.Column<decimal>(type: "numeric", nullable: false),
                    baf_billing_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    baf_charge_type = table.Column<string>(type: "text", nullable: true),
                    baf_discount = table.Column<decimal>(type: "numeric", nullable: false),
                    baf_net_revenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    baf_rate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    base_or_station = table.Column<string>(type: "varchar(100)", nullable: true),
                    billing_number = table.Column<string>(type: "text", nullable: true),
                    cos_number = table.Column<string>(type: "varchar(10)", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: true),
                    date_arrived = table.Column<DateOnly>(type: "date", nullable: true),
                    date_left = table.Column<DateOnly>(type: "date", nullable: true),
                    dispatch_billing_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    dispatch_charge_type = table.Column<string>(type: "text", nullable: true),
                    dispatch_discount = table.Column<decimal>(type: "numeric", nullable: false),
                    dispatch_net_revenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    dispatch_number = table.Column<string>(type: "varchar(20)", nullable: false),
                    dispatch_rate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    edited_by = table.Column<string>(type: "text", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    image_name = table.Column<string>(type: "text", nullable: true),
                    image_saved_url = table.Column<string>(type: "text", nullable: true),
                    image_signed_url = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "varchar(100)", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    tariff_by = table.Column<string>(type: "text", nullable: true),
                    tariff_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tariff_edited_by = table.Column<string>(type: "text", nullable: true),
                    tariff_edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    time_arrived = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    time_left = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    total_billing = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_hours = table.Column<decimal>(type: "numeric", nullable: false),
                    total_net_revenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    video_name = table.Column<string>(type: "text", nullable: true),
                    video_saved_url = table.Column<string>(type: "text", nullable: true),
                    video_signed_url = table.Column<string>(type: "text", nullable: true),
                    voyage_number = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mmsi_dispatch_tickets", x => x.dispatch_ticket_id);
                    table.ForeignKey(
                        name: "fk_mmsi_dispatch_tickets_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "fk_mmsi_dispatch_tickets_mmsi_billings_billing_id",
                        column: x => x.billing_id,
                        principalTable: "mmsi_billings",
                        principalColumn: "mmsi_billing_id");
                    table.ForeignKey(
                        name: "fk_mmsi_dispatch_tickets_mmsi_services_service_id",
                        column: x => x.service_id,
                        principalTable: "mmsi_services",
                        principalColumn: "service_id");
                    table.ForeignKey(
                        name: "fk_mmsi_dispatch_tickets_mmsi_terminals_terminal_id",
                        column: x => x.terminal_id,
                        principalTable: "mmsi_terminals",
                        principalColumn: "terminal_id");
                    table.ForeignKey(
                        name: "fk_mmsi_dispatch_tickets_mmsi_tug_masters_tug_master_id",
                        column: x => x.tug_master_id,
                        principalTable: "mmsi_tug_masters",
                        principalColumn: "tug_master_id");
                    table.ForeignKey(
                        name: "fk_mmsi_dispatch_tickets_mmsi_tugboats_tug_boat_id",
                        column: x => x.tug_boat_id,
                        principalTable: "mmsi_tugboats",
                        principalColumn: "tugboat_id");
                    table.ForeignKey(
                        name: "fk_mmsi_dispatch_tickets_mmsi_vessels_vessel_id",
                        column: x => x.vessel_id,
                        principalTable: "mmsi_vessels",
                        principalColumn: "vessel_id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_billings_customer_id",
                table: "mmsi_billings",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_billings_mmsi_collection_id",
                table: "mmsi_billings",
                column: "mmsi_collection_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_billings_port_id",
                table: "mmsi_billings",
                column: "port_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_billings_principal_id",
                table: "mmsi_billings",
                column: "principal_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_billings_terminal_id",
                table: "mmsi_billings",
                column: "terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_billings_vessel_id",
                table: "mmsi_billings",
                column: "vessel_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_collections_customer_id",
                table: "mmsi_collections",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_dispatch_tickets_billing_id",
                table: "mmsi_dispatch_tickets",
                column: "billing_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_dispatch_tickets_customer_id",
                table: "mmsi_dispatch_tickets",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_dispatch_tickets_service_id",
                table: "mmsi_dispatch_tickets",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_dispatch_tickets_terminal_id",
                table: "mmsi_dispatch_tickets",
                column: "terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_dispatch_tickets_tug_boat_id",
                table: "mmsi_dispatch_tickets",
                column: "tug_boat_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_dispatch_tickets_tug_master_id",
                table: "mmsi_dispatch_tickets",
                column: "tug_master_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_dispatch_tickets_vessel_id",
                table: "mmsi_dispatch_tickets",
                column: "vessel_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_principals_customer_id",
                table: "mmsi_principals",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_tariff_rates_customer_id",
                table: "mmsi_tariff_rates",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_tariff_rates_service_id",
                table: "mmsi_tariff_rates",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_tariff_rates_terminal_id",
                table: "mmsi_tariff_rates",
                column: "terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_terminals_port_id",
                table: "mmsi_terminals",
                column: "port_id");

            migrationBuilder.CreateIndex(
                name: "ix_mmsi_tugboats_tugboat_owner_id",
                table: "mmsi_tugboats",
                column: "tugboat_owner_id");
        }
    }
}
