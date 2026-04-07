using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovedMobilityEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobility_chart_of_accounts");

            migrationBuilder.DropTable(
                name: "mobility_check_voucher_details");

            migrationBuilder.DropTable(
                name: "mobility_collection_receipts");

            migrationBuilder.DropTable(
                name: "mobility_credit_memos");

            migrationBuilder.DropTable(
                name: "mobility_customer_order_slips");

            migrationBuilder.DropTable(
                name: "mobility_customer_purchase_orders");

            migrationBuilder.DropTable(
                name: "mobility_cv_trade_payments");

            migrationBuilder.DropTable(
                name: "mobility_debit_memos");

            migrationBuilder.DropTable(
                name: "mobility_fms_calibrations");

            migrationBuilder.DropTable(
                name: "mobility_fms_cashier_shifts");

            migrationBuilder.DropTable(
                name: "mobility_fms_deposits");

            migrationBuilder.DropTable(
                name: "mobility_fms_fuel_sales");

            migrationBuilder.DropTable(
                name: "mobility_fms_lube_sales");

            migrationBuilder.DropTable(
                name: "mobility_fms_po_sales");

            migrationBuilder.DropTable(
                name: "mobility_fuel_deliveries");

            migrationBuilder.DropTable(
                name: "mobility_fuel_purchase");

            migrationBuilder.DropTable(
                name: "mobility_fuels");

            migrationBuilder.DropTable(
                name: "mobility_general_ledgers");

            migrationBuilder.DropTable(
                name: "mobility_inventories");

            migrationBuilder.DropTable(
                name: "mobility_journal_voucher_details");

            migrationBuilder.DropTable(
                name: "mobility_log_reports");

            migrationBuilder.DropTable(
                name: "mobility_lube_deliveries");

            migrationBuilder.DropTable(
                name: "mobility_lube_purchase_details");

            migrationBuilder.DropTable(
                name: "mobility_lubes");

            migrationBuilder.DropTable(
                name: "mobility_multiple_check_voucher_payments");

            migrationBuilder.DropTable(
                name: "mobility_offlines");

            migrationBuilder.DropTable(
                name: "mobility_offsettings");

            migrationBuilder.DropTable(
                name: "mobility_po_sales");

            migrationBuilder.DropTable(
                name: "mobility_po_sales_raw");

            migrationBuilder.DropTable(
                name: "mobility_receiving_reports");

            migrationBuilder.DropTable(
                name: "mobility_safe_drops");

            migrationBuilder.DropTable(
                name: "mobility_sales_details");

            migrationBuilder.DropTable(
                name: "mobility_station_pumps");

            migrationBuilder.DropTable(
                name: "mobility_service_invoices");

            migrationBuilder.DropTable(
                name: "mobility_journal_voucher_headers");

            migrationBuilder.DropTable(
                name: "mobility_lube_purchase_headers");

            migrationBuilder.DropTable(
                name: "mobility_purchase_orders");

            migrationBuilder.DropTable(
                name: "mobility_sales_headers");

            migrationBuilder.DropTable(
                name: "mobility_customers");

            migrationBuilder.DropTable(
                name: "mobility_services");

            migrationBuilder.DropTable(
                name: "mobility_check_voucher_headers");

            migrationBuilder.DropTable(
                name: "mobility_pick_up_points");

            migrationBuilder.DropTable(
                name: "mobility_products");

            migrationBuilder.DropTable(
                name: "mobility_stations");

            migrationBuilder.DropTable(
                name: "mobility_bank_accounts");

            migrationBuilder.DropTable(
                name: "mobility_station_employees");

            migrationBuilder.DropTable(
                name: "mobility_suppliers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobility_bank_accounts",
                columns: table => new
                {
                    bank_account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_name = table.Column<string>(type: "text", nullable: false),
                    account_no = table.Column<string>(type: "text", nullable: false),
                    bank = table.Column<string>(type: "text", nullable: false),
                    branch = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_bank_accounts", x => x.bank_account_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_chart_of_accounts",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    account_number = table.Column<string>(type: "varchar(15)", nullable: true),
                    account_type = table.Column<string>(type: "varchar(25)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    normal_balance = table.Column<string>(type: "varchar(20)", nullable: true),
                    parent = table.Column<string>(type: "varchar(15)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_chart_of_accounts", x => x.account_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_customers",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    business_style = table.Column<string>(type: "varchar(100)", nullable: true),
                    cluster_code = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit_limit_as_of_today = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    customer_address = table.Column<string>(type: "varchar(200)", nullable: false),
                    customer_code = table.Column<string>(type: "varchar(7)", nullable: true),
                    customer_code_name = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    customer_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    customer_terms = table.Column<string>(type: "varchar(10)", nullable: false),
                    customer_tin = table.Column<string>(type: "text", nullable: false),
                    customer_type = table.Column<string>(type: "varchar(10)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    has_multiple_terms = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_check_details_required = table.Column<bool>(type: "boolean", nullable: false),
                    quantity_limit = table.Column<decimal>(type: "numeric", nullable: false),
                    retention_rate = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    station_code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    vat_type = table.Column<string>(type: "varchar(10)", nullable: false),
                    with_holding_tax = table.Column<bool>(type: "boolean", nullable: false),
                    with_holding_vat = table.Column<bool>(type: "boolean", nullable: false),
                    zip_code = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_customers", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fms_calibrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    product_code = table.Column<string>(type: "text", nullable: false),
                    pump_number = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    shift_date = table.Column<DateOnly>(type: "date", nullable: false),
                    shift_number = table.Column<int>(type: "integer", nullable: false),
                    shift_record_id = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fms_calibrations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fms_cashier_shifts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    biodiesel_price = table.Column<decimal>(type: "numeric", nullable: false),
                    cash_on_hand = table.Column<decimal>(type: "numeric", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    econogas_price = table.Column<decimal>(type: "numeric", nullable: false),
                    employee_number = table.Column<string>(type: "text", nullable: false),
                    envirogas_price = table.Column<decimal>(type: "numeric", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    next_day = table.Column<bool>(type: "boolean", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    shift_number = table.Column<int>(type: "integer", nullable: false),
                    shift_record_id = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    time_in = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    time_out = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fms_cashier_shifts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fms_deposits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_number = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    approved_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    shift_date = table.Column<DateOnly>(type: "date", nullable: false),
                    shift_number = table.Column<int>(type: "integer", nullable: false),
                    shift_record_id = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fms_deposits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fms_fuel_sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    closing = table.Column<decimal>(type: "numeric", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    opening = table.Column<decimal>(type: "numeric", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    product_code = table.Column<string>(type: "text", nullable: false),
                    pump_number = table.Column<int>(type: "integer", nullable: false),
                    shift_date = table.Column<DateOnly>(type: "date", nullable: false),
                    shift_number = table.Column<int>(type: "integer", nullable: false),
                    shift_record_id = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fms_fuel_sales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fms_lube_sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actual_price = table.Column<decimal>(type: "numeric", nullable: false),
                    cost = table.Column<decimal>(type: "numeric", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    product_code = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    shift_date = table.Column<DateOnly>(type: "date", nullable: false),
                    shift_number = table.Column<int>(type: "integer", nullable: false),
                    shift_record_id = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fms_lube_sales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fms_po_sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_price = table.Column<decimal>(type: "numeric", nullable: false),
                    customer_code = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    dr_number = table.Column<string>(type: "text", nullable: false),
                    driver = table.Column<string>(type: "text", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    plate_no = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    product_code = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    shift_date = table.Column<DateOnly>(type: "date", nullable: false),
                    shift_number = table.Column<int>(type: "integer", nullable: false),
                    shift_record_id = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    trip_ticket = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fms_po_sales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fuel_deliveries",
                columns: table => new
                {
                    fuel_delivery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cashiercode = table.Column<string>(type: "text", nullable: false),
                    createdby = table.Column<string>(type: "varchar(50)", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    driver = table.Column<string>(type: "varchar(100)", nullable: false),
                    drnumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    hauler = table.Column<string>(type: "varchar(100)", nullable: false),
                    pagenumber = table.Column<int>(type: "integer", nullable: false),
                    platenumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    productcode = table.Column<string>(type: "varchar(10)", nullable: false),
                    purchaseprice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    receivedby = table.Column<string>(type: "varchar(50)", nullable: false),
                    sellprice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    shiftdate = table.Column<DateOnly>(type: "date", nullable: false),
                    shiftnumber = table.Column<int>(type: "integer", nullable: false),
                    stncode = table.Column<string>(type: "varchar(5)", nullable: false),
                    tanknumber = table.Column<int>(type: "integer", nullable: false),
                    timein = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    timeout = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    volumeafter = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    volumebefore = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    wcnumber = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fuel_deliveries", x => x.fuel_delivery_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fuel_purchase",
                columns: table => new
                {
                    fuel_purchase_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    cashier_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    dr_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    driver = table.Column<string>(type: "varchar(100)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    fuel_purchase_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    gain_or_loss = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    hauler = table.Column<string>(type: "varchar(100)", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    plate_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    product_code = table.Column<string>(type: "varchar(10)", nullable: false),
                    purchase_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity_after = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity_before = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    received_by = table.Column<string>(type: "varchar(50)", nullable: false),
                    selling_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    shift_date = table.Column<DateOnly>(type: "date", nullable: false),
                    shift_no = table.Column<int>(type: "integer", nullable: false),
                    should_be = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    tank_no = table.Column<int>(type: "integer", nullable: false),
                    time_in = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    time_out = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    wc_no = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fuel_purchase", x => x.fuel_purchase_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_fuels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    amount_db = table.Column<decimal>(type: "numeric", nullable: false),
                    business_date = table.Column<DateOnly>(type: "date", nullable: false),
                    calibration = table.Column<decimal>(type: "numeric", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    closing = table.Column<decimal>(type: "numeric", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    detail_group = table.Column<int>(type: "integer", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    end = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    inv_date = table.Column<DateOnly>(type: "date", nullable: false),
                    in_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    item_code = table.Column<string>(type: "varchar(16)", nullable: false),
                    liters = table.Column<decimal>(type: "numeric", nullable: true),
                    opening = table.Column<decimal>(type: "numeric", nullable: true),
                    out_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    particulars = table.Column<string>(type: "varchar(32)", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    shift = table.Column<int>(type: "integer", nullable: false),
                    start = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    trans_count = table.Column<int>(type: "integer", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    volume = table.Column<decimal>(type: "numeric", nullable: false),
                    cust = table.Column<string>(type: "varchar(20)", nullable: true),
                    nozdown = table.Column<string>(type: "varchar(20)", nullable: false),
                    plateno = table.Column<string>(type: "varchar(20)", nullable: true),
                    pono = table.Column<string>(type: "varchar(20)", nullable: true),
                    x_corpcode = table.Column<int>(type: "integer", nullable: true),
                    x_day = table.Column<int>(type: "integer", nullable: true),
                    x_month = table.Column<int>(type: "integer", nullable: true),
                    x_nozzle = table.Column<int>(type: "integer", nullable: true),
                    x_oid = table.Column<string>(type: "varchar(20)", nullable: true),
                    x_oname = table.Column<string>(type: "varchar(20)", nullable: false),
                    x_pump = table.Column<int>(type: "integer", nullable: false),
                    x_sitecode = table.Column<int>(type: "integer", nullable: false),
                    x_tank = table.Column<int>(type: "integer", nullable: true),
                    x_transaction = table.Column<int>(type: "integer", nullable: true),
                    x_ticket_id = table.Column<string>(type: "text", nullable: false),
                    x_year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_fuels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_general_ledgers",
                columns: table => new
                {
                    general_ledger_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_number = table.Column<string>(type: "varchar(15)", nullable: false),
                    account_title = table.Column<string>(type: "varchar(200)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    customer_code = table.Column<string>(type: "varchar(200)", nullable: true),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_validated = table.Column<bool>(type: "boolean", nullable: false),
                    journal_reference = table.Column<string>(type: "varchar(50)", nullable: false),
                    particular = table.Column<string>(type: "varchar(200)", nullable: false),
                    product_code = table.Column<string>(type: "varchar(20)", nullable: true),
                    reference = table.Column<string>(type: "varchar(100)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    supplier_code = table.Column<string>(type: "varchar(200)", nullable: true),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_general_ledgers", x => x.general_ledger_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_inventories",
                columns: table => new
                {
                    inventory_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cost_of_goods_sold = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    inventory_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    inventory_value = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    particulars = table.Column<string>(type: "varchar(50)", nullable: false),
                    product_code = table.Column<string>(type: "varchar(10)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    reference = table.Column<string>(type: "varchar(200)", nullable: false),
                    running_cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(10)", nullable: true),
                    total_cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    transaction_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit_cost_average = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    validated_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    validated_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_inventories", x => x.inventory_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_log_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    adjusted_value = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    modified_by = table.Column<string>(type: "text", nullable: false),
                    module = table.Column<string>(type: "text", nullable: false),
                    original_value = table.Column<string>(type: "text", nullable: true),
                    reference = table.Column<string>(type: "text", nullable: false),
                    reference_id = table.Column<int>(type: "integer", nullable: false),
                    time_stamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_log_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_lube_deliveries",
                columns: table => new
                {
                    lube_delivery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    cashiercode = table.Column<string>(type: "text", nullable: false),
                    cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    createdby = table.Column<string>(type: "varchar(50)", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    description = table.Column<string>(type: "varchar(200)", nullable: false),
                    drno = table.Column<string>(type: "varchar(50)", nullable: false),
                    freight = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    invoiceno = table.Column<string>(type: "varchar(50)", nullable: false),
                    pagenumber = table.Column<int>(type: "integer", nullable: false),
                    piece = table.Column<int>(type: "integer", nullable: false),
                    pono = table.Column<string>(type: "varchar(50)", nullable: false),
                    productcode = table.Column<string>(type: "varchar(10)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    rcvdby = table.Column<string>(type: "varchar(50)", nullable: false),
                    shiftdate = table.Column<DateOnly>(type: "date", nullable: false),
                    shiftnumber = table.Column<int>(type: "integer", nullable: false),
                    srp = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    stncode = table.Column<string>(type: "varchar(5)", nullable: false),
                    suppliercode = table.Column<string>(type: "varchar(10)", nullable: false),
                    unit = table.Column<string>(type: "varchar(10)", nullable: false),
                    unitprice = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_lube_deliveries", x => x.lube_delivery_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_lube_purchase_headers",
                columns: table => new
                {
                    lube_purchase_header_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    cashier_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    dr_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    lube_purchase_header_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    po_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    received_by = table.Column<string>(type: "varchar(50)", nullable: false),
                    sales_invoice = table.Column<string>(type: "varchar(50)", nullable: false),
                    shift_date = table.Column<DateOnly>(type: "date", nullable: false),
                    shift_no = table.Column<int>(type: "integer", nullable: false),
                    station_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    supplier_code = table.Column<string>(type: "varchar(10)", nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vatable_sales = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_lube_purchase_headers", x => x.lube_purchase_header_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_lubes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_db = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    business_date = table.Column<DateOnly>(type: "date", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    cashier = table.Column<string>(type: "varchar(20)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    inv_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    item_code = table.Column<string>(type: "varchar(16)", nullable: false),
                    lubes_qty = table.Column<decimal>(type: "numeric", nullable: false),
                    particulars = table.Column<string>(type: "varchar(100)", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    shift = table.Column<int>(type: "integer", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cust = table.Column<string>(type: "varchar(20)", nullable: true),
                    plateno = table.Column<string>(type: "varchar(20)", nullable: true),
                    pono = table.Column<string>(type: "varchar(20)", nullable: true),
                    x_corpcode = table.Column<int>(type: "integer", nullable: true),
                    x_day = table.Column<int>(type: "integer", nullable: true),
                    x_month = table.Column<int>(type: "integer", nullable: true),
                    x_oid = table.Column<string>(type: "varchar(10)", nullable: true),
                    x_sitecode = table.Column<int>(type: "integer", nullable: false),
                    x_stamp = table.Column<string>(type: "varchar(50)", nullable: false),
                    x_transaction = table.Column<int>(type: "integer", nullable: true),
                    x_ticket_id = table.Column<string>(type: "text", nullable: false),
                    x_year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_lubes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_offlines",
                columns: table => new
                {
                    offline_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    first_dsr_closing = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    first_dsr_no = table.Column<string>(type: "text", nullable: false),
                    first_dsr_opening = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_resolve = table.Column<bool>(type: "boolean", nullable: false),
                    last_updated_by = table.Column<string>(type: "text", nullable: true),
                    last_updated_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    liters = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    new_closing = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    product = table.Column<string>(type: "varchar(20)", nullable: false),
                    pump = table.Column<int>(type: "integer", nullable: false),
                    second_dsr_closing = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    second_dsr_no = table.Column<string>(type: "text", nullable: false),
                    second_dsr_opening = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    series_no = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_offlines", x => x.offline_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_offsettings",
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
                    source = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_offsettings", x => x.off_setting_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_po_sales",
                columns: table => new
                {
                    po_sales_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    cashier_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    contract_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    customer_code = table.Column<string>(type: "varchar(20)", nullable: false),
                    dr_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    driver = table.Column<string>(type: "varchar(50)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    po_sales_date = table.Column<DateOnly>(type: "date", nullable: false),
                    po_sales_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    po_sales_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    plate_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    product_code = table.Column<string>(type: "varchar(10)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    shift_no = table.Column<int>(type: "integer", nullable: false),
                    shift_rec_id = table.Column<string>(type: "varchar(20)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    trip_ticket = table.Column<string>(type: "varchar(20)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_po_sales", x => x.po_sales_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_po_sales_raw",
                columns: table => new
                {
                    po_sales_raw_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cashiercode = table.Column<string>(type: "text", nullable: false),
                    contractprice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    createdby = table.Column<string>(type: "varchar(50)", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    customercode = table.Column<string>(type: "varchar(20)", nullable: false),
                    driver = table.Column<string>(type: "varchar(50)", nullable: false),
                    drnumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    plateno = table.Column<string>(type: "varchar(50)", nullable: false),
                    podate = table.Column<DateOnly>(type: "date", nullable: false),
                    potime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    productcode = table.Column<string>(type: "varchar(10)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    shiftnumber = table.Column<int>(type: "integer", nullable: false),
                    shiftrecid = table.Column<string>(type: "varchar(20)", nullable: false),
                    stncode = table.Column<string>(type: "varchar(5)", nullable: false),
                    tripticket = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_po_sales_raw", x => x.po_sales_raw_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_products",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    product_code = table.Column<string>(type: "varchar(10)", nullable: false),
                    product_name = table.Column<string>(type: "varchar(50)", nullable: false),
                    product_unit = table.Column<string>(type: "varchar(2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_products", x => x.product_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_safe_drops",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    b_date = table.Column<DateOnly>(type: "date", nullable: true),
                    business_date = table.Column<DateOnly>(type: "date", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    inv_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    shift = table.Column<int>(type: "integer", nullable: false),
                    t_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    x_corpcode = table.Column<int>(type: "integer", nullable: true),
                    x_day = table.Column<int>(type: "integer", nullable: true),
                    x_month = table.Column<int>(type: "integer", nullable: true),
                    x_oid = table.Column<string>(type: "varchar(10)", nullable: true),
                    x_oname = table.Column<string>(type: "varchar(20)", nullable: false),
                    x_sitecode = table.Column<int>(type: "integer", nullable: true),
                    x_stamp = table.Column<string>(type: "varchar(50)", nullable: false),
                    x_ticket_id = table.Column<string>(type: "text", nullable: false),
                    x_year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_safe_drops", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_sales_headers",
                columns: table => new
                {
                    sales_header_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    actual_cash_on_hand = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    cashier = table.Column<string>(type: "varchar(20)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    customers = table.Column<string[]>(type: "varchar[]", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    fuel_sales_total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    gain_or_loss = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_modified = table.Column<bool>(type: "boolean", nullable: false),
                    is_transaction_normal = table.Column<bool>(type: "boolean", nullable: false),
                    lubes_total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    po_sales_amount = table.Column<decimal[]>(type: "numeric(18,4)[]", nullable: false),
                    po_sales_total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: false),
                    particular = table.Column<string>(type: "varchar(200)", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    safe_drop_total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    sales_no = table.Column<string>(type: "varchar(15)", nullable: false),
                    shift = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "varchar(10)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", nullable: false),
                    time_in = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    time_out = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    total_sales = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_sales_headers", x => x.sales_header_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_services",
                columns: table => new
                {
                    service_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    current_and_previous_no = table.Column<string>(type: "varchar(20)", nullable: true),
                    current_and_previous_title = table.Column<string>(type: "varchar(50)", nullable: true),
                    name = table.Column<string>(type: "varchar(50)", nullable: false),
                    percent = table.Column<int>(type: "integer", nullable: false),
                    service_no = table.Column<string>(type: "text", nullable: true),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    unearned_no = table.Column<string>(type: "varchar(20)", nullable: true),
                    unearned_title = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_services", x => x.service_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_station_employees",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<string>(type: "varchar(255)", nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    date_hired = table.Column<DateOnly>(type: "date", nullable: false),
                    date_resigned = table.Column<DateOnly>(type: "date", nullable: true),
                    department = table.Column<string>(type: "text", nullable: true),
                    employee_number = table.Column<string>(type: "varchar(10)", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    initial = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_managerial = table.Column<bool>(type: "boolean", nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    middle_name = table.Column<string>(type: "varchar(100)", nullable: true),
                    pagibig_no = table.Column<string>(type: "text", nullable: true),
                    paygrade = table.Column<string>(type: "text", nullable: true),
                    philhealth_no = table.Column<string>(type: "text", nullable: true),
                    position = table.Column<string>(type: "text", nullable: false),
                    salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    sss_no = table.Column<string>(type: "text", nullable: true),
                    station_code = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "varchar(5)", nullable: false),
                    suffix = table.Column<string>(type: "varchar(5)", nullable: true),
                    supervisor = table.Column<string>(type: "varchar(20)", nullable: false),
                    tel_no = table.Column<string>(type: "text", nullable: true),
                    tin_no = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_station_employees", x => x.employee_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_station_pumps",
                columns: table => new
                {
                    pump_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fms_pump = table.Column<int>(type: "integer", nullable: false),
                    pos_pump = table.Column<int>(type: "integer", nullable: false),
                    product_code = table.Column<string>(type: "varchar(15)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_station_pumps", x => x.pump_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_stations",
                columns: table => new
                {
                    station_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    folder_path = table.Column<string>(type: "varchar(255)", nullable: false),
                    initial = table.Column<string>(type: "varchar(5)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    pos_code = table.Column<string>(type: "text", nullable: false),
                    station_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    station_name = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_stations", x => x.station_code);
                });

            migrationBuilder.CreateTable(
                name: "mobility_suppliers",
                columns: table => new
                {
                    supplier_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    branch = table.Column<string>(type: "varchar(20)", nullable: true),
                    category = table.Column<string>(type: "varchar(20)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    default_expense_number = table.Column<string>(type: "varchar(100)", nullable: true),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    proof_of_exemption_file_name = table.Column<string>(type: "text", nullable: true),
                    proof_of_exemption_file_path = table.Column<string>(type: "varchar(1024)", nullable: true),
                    proof_of_registration_file_name = table.Column<string>(type: "text", nullable: true),
                    proof_of_registration_file_path = table.Column<string>(type: "varchar(1024)", nullable: true),
                    reason_of_exemption = table.Column<string>(type: "varchar(100)", nullable: true),
                    station_code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    supplier_address = table.Column<string>(type: "varchar(200)", nullable: false),
                    supplier_code = table.Column<string>(type: "varchar(7)", nullable: true),
                    supplier_name = table.Column<string>(type: "varchar(50)", nullable: false),
                    supplier_terms = table.Column<string>(type: "varchar(10)", nullable: false),
                    supplier_tin = table.Column<string>(type: "varchar(20)", nullable: false),
                    tax_type = table.Column<string>(type: "varchar(20)", nullable: false),
                    trade_name = table.Column<string>(type: "varchar(255)", nullable: true),
                    validity = table.Column<string>(type: "varchar(20)", nullable: true),
                    validity_date = table.Column<DateOnly>(type: "date", nullable: true),
                    vat_type = table.Column<string>(type: "varchar(10)", nullable: false),
                    withholding_tax_percent = table.Column<decimal>(type: "numeric", nullable: true),
                    withholding_taxtitle = table.Column<string>(type: "varchar(100)", nullable: true),
                    zip_code = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_suppliers", x => x.supplier_id);
                });

            migrationBuilder.CreateTable(
                name: "mobility_lube_purchase_details",
                columns: table => new
                {
                    lube_purchase_detail_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lube_purchase_header_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    cost_per_case = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    cost_per_piece = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    description = table.Column<string>(type: "varchar(200)", nullable: false),
                    lube_purchase_header_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    piece = table.Column<int>(type: "integer", nullable: false),
                    product_code = table.Column<string>(type: "varchar(10)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", nullable: false),
                    unit = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_lube_purchase_details", x => x.lube_purchase_detail_id);
                    table.ForeignKey(
                        name: "fk_mobility_lube_purchase_details_mobility_lube_purchase_heade",
                        column: x => x.lube_purchase_header_id,
                        principalTable: "mobility_lube_purchase_headers",
                        principalColumn: "lube_purchase_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_sales_details",
                columns: table => new
                {
                    sales_detail_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sales_header_id = table.Column<int>(type: "integer", nullable: false),
                    calibration = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    closing = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    liters = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    liters_sold = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    opening = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    particular = table.Column<string>(type: "varchar(50)", nullable: false),
                    previous_price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    product = table.Column<string>(type: "varchar(20)", nullable: false),
                    pump_number = table.Column<int>(type: "integer", nullable: false),
                    reference_no = table.Column<string>(type: "varchar(15)", nullable: true),
                    sale = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    sales_no = table.Column<string>(type: "varchar(15)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", nullable: false),
                    transaction_count = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_sales_details", x => x.sales_detail_id);
                    table.ForeignKey(
                        name: "fk_mobility_sales_details_mobility_sales_headers_sales_header_",
                        column: x => x.sales_header_id,
                        principalTable: "mobility_sales_headers",
                        principalColumn: "sales_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_customer_order_slips",
                columns: table => new
                {
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", nullable: false),
                    address = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    approved_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    check_no = table.Column<string>(type: "text", nullable: true),
                    check_picture_saved_file_name = table.Column<string>(type: "text", nullable: true),
                    check_picture_saved_url = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    customer_order_slip_no = table.Column<string>(type: "varchar(13)", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    disapproval_remarks = table.Column<string>(type: "varchar(200)", nullable: true),
                    disapproved_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    disapproved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    driver = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    edited_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    load_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    plate_no = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    price_per_liter = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    saved_file_name = table.Column<string>(type: "text", nullable: true),
                    saved_url = table.Column<string>(type: "text", nullable: true),
                    station_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", nullable: false),
                    terms = table.Column<string>(type: "varchar(10)", nullable: false),
                    trip_ticket = table.Column<string>(type: "varchar(20)", nullable: true),
                    uploaded_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    uploaded_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_customer_order_slips", x => x.customer_order_slip_id);
                    table.ForeignKey(
                        name: "fk_mobility_customer_order_slips_mobility_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "mobility_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mobility_customer_order_slips_mobility_stations_station_code",
                        column: x => x.station_code,
                        principalTable: "mobility_stations",
                        principalColumn: "station_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mobility_customer_order_slips_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mobility_customer_purchase_orders",
                columns: table => new
                {
                    customer_purchase_order_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    station_code = table.Column<string>(type: "varchar(5)", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    customer_purchase_order_no = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    station_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_customer_purchase_orders", x => x.customer_purchase_order_id);
                    table.ForeignKey(
                        name: "fk_mobility_customer_purchase_orders_mobility_customers_custom",
                        column: x => x.customer_id,
                        principalTable: "mobility_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mobility_customer_purchase_orders_mobility_stations_station",
                        column: x => x.station_code,
                        principalTable: "mobility_stations",
                        principalColumn: "station_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mobility_customer_purchase_orders_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mobility_service_invoices",
                columns: table => new
                {
                    service_invoice_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    customer_address = table.Column<string>(type: "text", nullable: false),
                    customer_tin = table.Column<string>(type: "text", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    instructions = table.Column<string>(type: "varchar(200)", nullable: true),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    payment_status = table.Column<string>(type: "varchar(20)", nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    service_invoice_no = table.Column<string>(type: "varchar(12)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_service_invoices", x => x.service_invoice_id);
                    table.ForeignKey(
                        name: "fk_mobility_service_invoices_mobility_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "mobility_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mobility_service_invoices_mobility_services_service_id",
                        column: x => x.service_id,
                        principalTable: "mobility_services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mobility_service_invoices_mobility_stations_station_code",
                        column: x => x.station_code,
                        principalTable: "mobility_stations",
                        principalColumn: "station_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mobility_check_voucher_headers",
                columns: table => new
                {
                    check_voucher_header_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_id = table.Column<int>(type: "integer", nullable: true),
                    employee_id = table.Column<int>(type: "integer", nullable: true),
                    supplier_id = table.Column<int>(type: "integer", nullable: true),
                    accrued_type = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal[]>(type: "numeric[]", nullable: true),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_per_month = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    category = table.Column<string>(type: "text", nullable: false),
                    check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    check_no = table.Column<string>(type: "text", nullable: true),
                    check_voucher_header_no = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    cv_type = table.Column<string>(type: "varchar(10)", nullable: true),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    dcp_date = table.Column<DateOnly>(type: "date", nullable: true),
                    dcr_date = table.Column<DateOnly>(type: "date", nullable: true),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    invoice_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_advances = table.Column<bool>(type: "boolean", nullable: false),
                    is_complete = table.Column<bool>(type: "boolean", nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    last_created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    number_of_months = table.Column<int>(type: "integer", nullable: false),
                    number_of_months_created = table.Column<int>(type: "integer", nullable: false),
                    po_no = table.Column<string[]>(type: "varchar[]", nullable: true),
                    particulars = table.Column<string>(type: "text", nullable: true),
                    payee = table.Column<string>(type: "text", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    rr_no = table.Column<string[]>(type: "varchar[]", nullable: true),
                    reference = table.Column<string>(type: "text", nullable: true),
                    si_no = table.Column<string[]>(type: "varchar[]", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    supporting_file_saved_file_name = table.Column<string>(type: "text", nullable: true),
                    supporting_file_saved_url = table.Column<string>(type: "text", nullable: true),
                    tin = table.Column<string>(type: "text", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_check_voucher_headers", x => x.check_voucher_header_id);
                    table.ForeignKey(
                        name: "fk_mobility_check_voucher_headers_mobility_bank_accounts_bank_",
                        column: x => x.bank_id,
                        principalTable: "mobility_bank_accounts",
                        principalColumn: "bank_account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mobility_check_voucher_headers_mobility_station_employees_e",
                        column: x => x.employee_id,
                        principalTable: "mobility_station_employees",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "fk_mobility_check_voucher_headers_mobility_suppliers_supplier_",
                        column: x => x.supplier_id,
                        principalTable: "mobility_suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_pick_up_points",
                columns: table => new
                {
                    pick_up_point_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    depot = table.Column<string>(type: "varchar(50)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_pick_up_points", x => x.pick_up_point_id);
                    table.ForeignKey(
                        name: "fk_mobility_pick_up_points_mobility_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "mobility_suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mobility_collection_receipts",
                columns: table => new
                {
                    collection_receipt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    cash_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_bank = table.Column<string>(type: "varchar(50)", nullable: true),
                    check_branch = table.Column<string>(type: "varchar(50)", nullable: true),
                    check_date = table.Column<string>(type: "text", nullable: true),
                    check_no = table.Column<string>(type: "varchar(50)", nullable: true),
                    collection_receipt_no = table.Column<string>(type: "varchar(12)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ewt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    f2306file_name = table.Column<string>(type: "text", nullable: true),
                    f2306file_path = table.Column<string>(type: "varchar(200)", nullable: true),
                    f2307file_name = table.Column<string>(type: "text", nullable: true),
                    f2307file_path = table.Column<string>(type: "varchar(200)", nullable: true),
                    is_certificate_upload = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    manager_check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    manager_check_bank = table.Column<string>(type: "varchar(50)", nullable: true),
                    manager_check_branch = table.Column<string>(type: "varchar(50)", nullable: true),
                    manager_check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    manager_check_no = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    reference_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    remarks = table.Column<string>(type: "varchar(100)", nullable: true),
                    sv_no = table.Column<string>(type: "varchar(12)", nullable: true),
                    series_number = table.Column<long>(type: "bigint", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    wvat = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_collection_receipts", x => x.collection_receipt_id);
                    table.ForeignKey(
                        name: "fk_mobility_collection_receipts_mobility_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "mobility_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mobility_collection_receipts_mobility_service_invoices_serv",
                        column: x => x.service_invoice_id,
                        principalTable: "mobility_service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_credit_memos",
                columns: table => new
                {
                    credit_memo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit_memo_no = table.Column<string>(type: "varchar(12)", nullable: true),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_credit_memos", x => x.credit_memo_id);
                    table.ForeignKey(
                        name: "fk_mobility_credit_memos_mobility_service_invoices_service_inv",
                        column: x => x.service_invoice_id,
                        principalTable: "mobility_service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_debit_memos",
                columns: table => new
                {
                    debit_memo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit_memo_no = table.Column<string>(type: "varchar(12)", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_debit_memos", x => x.debit_memo_id);
                    table.ForeignKey(
                        name: "fk_mobility_debit_memos_mobility_service_invoices_service_invo",
                        column: x => x.service_invoice_id,
                        principalTable: "mobility_service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_check_voucher_details",
                columns: table => new
                {
                    check_voucher_detail_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    check_voucher_header_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: true),
                    account_name = table.Column<string>(type: "text", nullable: false),
                    account_no = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    bank_id = table.Column<int>(type: "integer", nullable: true),
                    company_id = table.Column<int>(type: "integer", nullable: true),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: true),
                    ewt_percent = table.Column<decimal>(type: "numeric", nullable: false),
                    is_user_selected = table.Column<bool>(type: "boolean", nullable: false),
                    is_vatable = table.Column<bool>(type: "boolean", nullable: false),
                    station_id = table.Column<int>(type: "integer", nullable: true),
                    transaction_no = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_check_voucher_details", x => x.check_voucher_detail_id);
                    table.ForeignKey(
                        name: "fk_mobility_check_voucher_details_mobility_check_voucher_heade",
                        column: x => x.check_voucher_header_id,
                        principalTable: "mobility_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mobility_check_voucher_details_mobility_suppliers_supplier_",
                        column: x => x.supplier_id,
                        principalTable: "mobility_suppliers",
                        principalColumn: "supplier_id");
                });

            migrationBuilder.CreateTable(
                name: "mobility_cv_trade_payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    check_voucher_id = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    document_id = table.Column<int>(type: "integer", nullable: false),
                    document_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_cv_trade_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_mobility_cv_trade_payments_mobility_check_voucher_headers_c",
                        column: x => x.check_voucher_id,
                        principalTable: "mobility_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_journal_voucher_headers",
                columns: table => new
                {
                    journal_voucher_header_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cv_id = table.Column<int>(type: "integer", nullable: true),
                    cr_no = table.Column<string>(type: "text", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    jv_reason = table.Column<string>(type: "text", nullable: false),
                    journal_voucher_header_no = table.Column<string>(type: "text", nullable: true),
                    particulars = table.Column<string>(type: "text", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    references = table.Column<string>(type: "text", nullable: true),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_journal_voucher_headers", x => x.journal_voucher_header_id);
                    table.ForeignKey(
                        name: "fk_mobility_journal_voucher_headers_mobility_check_voucher_hea",
                        column: x => x.cv_id,
                        principalTable: "mobility_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_multiple_check_voucher_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_voucher_header_invoice_id = table.Column<int>(type: "integer", nullable: false),
                    check_voucher_header_payment_id = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_multiple_check_voucher_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_mobility_multiple_check_voucher_payments_mobility_check_vou",
                        column: x => x.check_voucher_header_invoice_id,
                        principalTable: "mobility_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mobility_multiple_check_voucher_payments_mobility_check_vou1",
                        column: x => x.check_voucher_header_payment_id,
                        principalTable: "mobility_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_purchase_orders",
                columns: table => new
                {
                    purchase_order_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pick_up_point_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    final_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    is_received = table.Column<bool>(type: "boolean", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    purchase_order_no = table.Column<string>(type: "varchar(15)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity_received = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    received_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    remarks = table.Column<string>(type: "varchar(200)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    supplier_address = table.Column<string>(type: "text", nullable: false),
                    supplier_sales_order_no = table.Column<string>(type: "varchar(100)", nullable: true),
                    supplier_tin = table.Column<string>(type: "text", nullable: false),
                    terms = table.Column<string>(type: "varchar(10)", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_purchase_orders", x => x.purchase_order_id);
                    table.ForeignKey(
                        name: "fk_mobility_purchase_orders_mobility_pick_up_points_pick_up_po",
                        column: x => x.pick_up_point_id,
                        principalTable: "mobility_pick_up_points",
                        principalColumn: "pick_up_point_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mobility_purchase_orders_mobility_products_product_id",
                        column: x => x.product_id,
                        principalTable: "mobility_products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mobility_purchase_orders_mobility_stations_station_code",
                        column: x => x.station_code,
                        principalTable: "mobility_stations",
                        principalColumn: "station_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mobility_purchase_orders_mobility_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "mobility_suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_journal_voucher_details",
                columns: table => new
                {
                    journal_voucher_detail_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    journal_voucher_header_id = table.Column<int>(type: "integer", nullable: false),
                    account_name = table.Column<string>(type: "text", nullable: false),
                    account_no = table.Column<string>(type: "text", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    transaction_no = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_journal_voucher_details", x => x.journal_voucher_detail_id);
                    table.ForeignKey(
                        name: "fk_mobility_journal_voucher_details_mobility_journal_voucher_h",
                        column: x => x.journal_voucher_header_id,
                        principalTable: "mobility_journal_voucher_headers",
                        principalColumn: "journal_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mobility_receiving_reports",
                columns: table => new
                {
                    receiving_report_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    authority_to_load_no = table.Column<string>(type: "varchar(100)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    canceled_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gain_or_loss = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    paid_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    purchase_order_no = table.Column<string>(type: "varchar(15)", nullable: true),
                    quantity_delivered = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity_received = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    received_date = table.Column<DateOnly>(type: "date", nullable: true),
                    receiving_report_no = table.Column<string>(type: "varchar(15)", nullable: false),
                    remarks = table.Column<string>(type: "varchar(200)", nullable: false),
                    station_code = table.Column<string>(type: "varchar(3)", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    supplier_dr_no = table.Column<string>(type: "varchar(50)", nullable: true),
                    supplier_invoice_date = table.Column<DateOnly>(type: "date", nullable: true),
                    supplier_invoice_number = table.Column<string>(type: "varchar(100)", nullable: true),
                    truck_or_vessels = table.Column<string>(type: "varchar(100)", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    withdrawal_certificate = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobility_receiving_reports", x => x.receiving_report_id);
                    table.ForeignKey(
                        name: "fk_mobility_receiving_reports_mobility_purchase_orders_purchas",
                        column: x => x.purchase_order_id,
                        principalTable: "mobility_purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mobility_chart_of_accounts_account_name",
                table: "mobility_chart_of_accounts",
                column: "account_name");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_chart_of_accounts_account_number",
                table: "mobility_chart_of_accounts",
                column: "account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mobility_check_voucher_details_check_voucher_header_id",
                table: "mobility_check_voucher_details",
                column: "check_voucher_header_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_check_voucher_details_supplier_id",
                table: "mobility_check_voucher_details",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_check_voucher_headers_bank_id",
                table: "mobility_check_voucher_headers",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_check_voucher_headers_employee_id",
                table: "mobility_check_voucher_headers",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_check_voucher_headers_supplier_id",
                table: "mobility_check_voucher_headers",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_collection_receipts_customer_id",
                table: "mobility_collection_receipts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_collection_receipts_service_invoice_id",
                table: "mobility_collection_receipts",
                column: "service_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_credit_memos_service_invoice_id",
                table: "mobility_credit_memos",
                column: "service_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_customer_order_slips_customer_id",
                table: "mobility_customer_order_slips",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_customer_order_slips_product_id",
                table: "mobility_customer_order_slips",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_customer_order_slips_station_code",
                table: "mobility_customer_order_slips",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_customer_purchase_orders_customer_id",
                table: "mobility_customer_purchase_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_customer_purchase_orders_product_id",
                table: "mobility_customer_purchase_orders",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_customer_purchase_orders_station_code",
                table: "mobility_customer_purchase_orders",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_customers_customer_id",
                table: "mobility_customers",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mobility_cv_trade_payments_check_voucher_id",
                table: "mobility_cv_trade_payments",
                column: "check_voucher_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_debit_memos_service_invoice_id",
                table: "mobility_debit_memos",
                column: "service_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fms_calibrations_shift_record_id",
                table: "mobility_fms_calibrations",
                column: "shift_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fms_calibrations_station_code",
                table: "mobility_fms_calibrations",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fms_cashier_shifts_shift_record_id",
                table: "mobility_fms_cashier_shifts",
                column: "shift_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fms_cashier_shifts_station_code",
                table: "mobility_fms_cashier_shifts",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fms_fuel_sales_shift_record_id",
                table: "mobility_fms_fuel_sales",
                column: "shift_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fms_fuel_sales_station_code",
                table: "mobility_fms_fuel_sales",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fms_lube_sales_shift_record_id",
                table: "mobility_fms_lube_sales",
                column: "shift_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fms_lube_sales_station_code",
                table: "mobility_fms_lube_sales",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuel_deliveries_pagenumber",
                table: "mobility_fuel_deliveries",
                column: "pagenumber");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuel_deliveries_stncode",
                table: "mobility_fuel_deliveries",
                column: "stncode");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuel_purchase_fuel_purchase_no",
                table: "mobility_fuel_purchase",
                column: "fuel_purchase_no");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuel_purchase_product_code",
                table: "mobility_fuel_purchase",
                column: "product_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuel_purchase_station_code",
                table: "mobility_fuel_purchase",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuels_inv_date",
                table: "mobility_fuels",
                column: "inv_date");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuels_item_code",
                table: "mobility_fuels",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuels_particulars",
                table: "mobility_fuels",
                column: "particulars");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuels_shift",
                table: "mobility_fuels",
                column: "shift");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuels_x_oname",
                table: "mobility_fuels",
                column: "x_oname");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuels_x_pump",
                table: "mobility_fuels",
                column: "x_pump");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuels_x_sitecode",
                table: "mobility_fuels",
                column: "x_sitecode");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_fuels_x_ticket_id",
                table: "mobility_fuels",
                column: "x_ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_account_number",
                table: "mobility_general_ledgers",
                column: "account_number");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_account_title",
                table: "mobility_general_ledgers",
                column: "account_title");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_customer_code",
                table: "mobility_general_ledgers",
                column: "customer_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_journal_reference",
                table: "mobility_general_ledgers",
                column: "journal_reference");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_product_code",
                table: "mobility_general_ledgers",
                column: "product_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_reference",
                table: "mobility_general_ledgers",
                column: "reference");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_station_code",
                table: "mobility_general_ledgers",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_supplier_code",
                table: "mobility_general_ledgers",
                column: "supplier_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_general_ledgers_transaction_date",
                table: "mobility_general_ledgers",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_inventories_product_code",
                table: "mobility_inventories",
                column: "product_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_inventories_station_code",
                table: "mobility_inventories",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_inventories_transaction_no",
                table: "mobility_inventories",
                column: "transaction_no");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_journal_voucher_details_journal_voucher_header_id",
                table: "mobility_journal_voucher_details",
                column: "journal_voucher_header_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_journal_voucher_headers_cv_id",
                table: "mobility_journal_voucher_headers",
                column: "cv_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lube_deliveries_pagenumber",
                table: "mobility_lube_deliveries",
                column: "pagenumber");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lube_deliveries_stncode",
                table: "mobility_lube_deliveries",
                column: "stncode");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lube_purchase_details_lube_purchase_header_id",
                table: "mobility_lube_purchase_details",
                column: "lube_purchase_header_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lube_purchase_details_lube_purchase_header_no",
                table: "mobility_lube_purchase_details",
                column: "lube_purchase_header_no");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lube_purchase_details_product_code",
                table: "mobility_lube_purchase_details",
                column: "product_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lube_purchase_headers_lube_purchase_header_no",
                table: "mobility_lube_purchase_headers",
                column: "lube_purchase_header_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lube_purchase_headers_station_code",
                table: "mobility_lube_purchase_headers",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lubes_cashier",
                table: "mobility_lubes",
                column: "cashier");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lubes_inv_date",
                table: "mobility_lubes",
                column: "inv_date");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_lubes_x_ticket_id",
                table: "mobility_lubes",
                column: "x_ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_multiple_check_voucher_payments_check_voucher_head",
                table: "mobility_multiple_check_voucher_payments",
                column: "check_voucher_header_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_multiple_check_voucher_payments_check_voucher_head1",
                table: "mobility_multiple_check_voucher_payments",
                column: "check_voucher_header_payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_pick_up_points_supplier_id",
                table: "mobility_pick_up_points",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_po_sales_po_sales_no",
                table: "mobility_po_sales",
                column: "po_sales_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mobility_po_sales_raw_shiftrecid",
                table: "mobility_po_sales_raw",
                column: "shiftrecid");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_po_sales_raw_stncode",
                table: "mobility_po_sales_raw",
                column: "stncode");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_po_sales_raw_tripticket",
                table: "mobility_po_sales_raw",
                column: "tripticket");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_purchase_orders_pick_up_point_id",
                table: "mobility_purchase_orders",
                column: "pick_up_point_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_purchase_orders_product_id",
                table: "mobility_purchase_orders",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_purchase_orders_purchase_order_no",
                table: "mobility_purchase_orders",
                column: "purchase_order_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mobility_purchase_orders_station_code",
                table: "mobility_purchase_orders",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_purchase_orders_supplier_id",
                table: "mobility_purchase_orders",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_receiving_reports_purchase_order_id",
                table: "mobility_receiving_reports",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_receiving_reports_receiving_report_no",
                table: "mobility_receiving_reports",
                column: "receiving_report_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mobility_receiving_reports_station_code",
                table: "mobility_receiving_reports",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_safe_drops_inv_date",
                table: "mobility_safe_drops",
                column: "inv_date");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_safe_drops_x_oname",
                table: "mobility_safe_drops",
                column: "x_oname");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_safe_drops_x_ticket_id",
                table: "mobility_safe_drops",
                column: "x_ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_sales_details_sales_header_id",
                table: "mobility_sales_details",
                column: "sales_header_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_sales_details_sales_no",
                table: "mobility_sales_details",
                column: "sales_no");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_sales_details_station_code",
                table: "mobility_sales_details",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_sales_headers_cashier",
                table: "mobility_sales_headers",
                column: "cashier");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_sales_headers_date",
                table: "mobility_sales_headers",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_sales_headers_sales_no",
                table: "mobility_sales_headers",
                column: "sales_no");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_sales_headers_shift",
                table: "mobility_sales_headers",
                column: "shift");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_sales_headers_station_code",
                table: "mobility_sales_headers",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_service_invoices_customer_id",
                table: "mobility_service_invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_service_invoices_service_id",
                table: "mobility_service_invoices",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_service_invoices_station_code",
                table: "mobility_service_invoices",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_station_employees_employee_number",
                table: "mobility_station_employees",
                column: "employee_number");

            migrationBuilder.CreateIndex(
                name: "ix_mobility_stations_pos_code",
                table: "mobility_stations",
                column: "pos_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mobility_stations_station_code",
                table: "mobility_stations",
                column: "station_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mobility_stations_station_name",
                table: "mobility_stations",
                column: "station_name",
                unique: true);
        }
    }
}
