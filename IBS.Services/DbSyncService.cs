using IBS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IBS.Services
{
    public class DbSyncService : IDbSyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DbSyncService> _logger;

        public DbSyncService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<DbSyncService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SyncAsync(string sourceConnectionString, CancellationToken cancellationToken = default)
        {
            var targetConnectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(targetConnectionString))
            {
                throw new InvalidOperationException("Target connection string (DefaultConnection) is not configured.");
            }

            var targetBuilder = new NpgsqlConnectionStringBuilder(targetConnectionString);
            var sourceBuilder = new NpgsqlConnectionStringBuilder(sourceConnectionString);

            // Ensure long timeout for synchronization
            targetBuilder.CommandTimeout = 600; // 10 minutes
            sourceBuilder.CommandTimeout = 600;

            // 1. Prevent source and target from being the same database
            if (targetBuilder.Host == sourceBuilder.Host &&
                targetBuilder.Port == sourceBuilder.Port &&
                targetBuilder.Database == sourceBuilder.Database)
            {
                throw new InvalidOperationException("Source and target databases cannot be the same.");
            }

            // 2. Safety Check
            var safeHosts = new[] { "localhost", "127.0.0.1", "db" };
            if (!safeHosts.Contains(targetBuilder.Host?.ToLower() ?? "") && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IBS_FORCE_SYNC")))
            {
                throw new InvalidOperationException($"Safety check failed: Target host '{targetBuilder.Host}' is not considered local or safe (localhost, 127.0.0.1, db).");
            }

            _logger.LogInformation("Starting database synchronization from {Source} to {Target}...",
                sourceBuilder.Host,
                targetBuilder.Host);

            // Get all table names from EF Core metadata
            var entityTypes = _context.Model.GetEntityTypes();
            var tables = entityTypes
                .Select(t => new { 
                    Schema = t.GetSchema() ?? "public", 
                    Table = t.GetTableName() 
                })
                .Where(t => t.Table != null)
                .Distinct()
                .ToList();

            _logger.LogInformation("Discovered {Count} tables to synchronize from EF Core metadata.", tables.Count);

            using var sourceConn = new NpgsqlConnection(sourceBuilder.ConnectionString);
            using var targetConn = new NpgsqlConnection(targetBuilder.ConnectionString);

            await sourceConn.OpenAsync(cancellationToken);
            await targetConn.OpenAsync(cancellationToken);

            using var transaction = await targetConn.BeginTransactionAsync(cancellationToken);

            try
            {
                // Disable all triggers and constraints for the session to avoid FK issues
                try
                {
                    using (var cmd = new NpgsqlCommand("SET session_replication_role = 'replica';", targetConn, transaction))
                    {
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not set session_replication_role to 'replica'. This usually requires superuser permissions. FK constraints may cause issues. Error: {Message}", ex.Message);
                }

                foreach (var item in tables)
                {
                    var localFullTableName = $"\"{item.Schema}\".\"{item.Table}\"";
                    
                    // 1. Find matching table on source (handle casing/prefix differences)
                    var sourceTableInfo = await FindSourceTableAsync(sourceConn, item.Schema, item.Table!, cancellationToken);
                    
                    if (sourceTableInfo == null)
                    {
                        _logger.LogWarning("Table {TableName} not found on source database. Skipping.", localFullTableName);
                        continue;
                    }

                    var sourceFullTableName = $"\"{sourceTableInfo.Value.Schema}\".\"{sourceTableInfo.Value.Table}\"";

                    // 2. Check source row count
                    long sourceRowCount;
                    using (var countCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM {sourceFullTableName}", sourceConn))
                    {
                        sourceRowCount = (long)(await countCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);
                    }

                    if (sourceRowCount == 0)
                    {
                        _logger.LogInformation("Source table {TableName} is empty. Skipping sync for this table.", sourceFullTableName);
                        continue;
                    }

                    // 3. Match columns with proper casing for both sides
                    var sourceColumns = await GetTableColumns(sourceConn, sourceTableInfo.Value.Schema, sourceTableInfo.Value.Table, cancellationToken);
                    var targetColumns = await GetTableColumns(targetConn, item.Schema, item.Table!, cancellationToken);
                    
                    var columnMapping = sourceColumns
                        .Join(targetColumns, 
                              s => s, 
                              t => t, 
                              (s, t) => new { Source = s, Target = t }, 
                              StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (columnMapping.Count == 0)
                    {
                        _logger.LogWarning("No common columns found for table {TableName} (Source: {SourceCount}, Target: {TargetCount}). Skipping sync.", 
                            localFullTableName, sourceColumns.Count, targetColumns.Count);
                        continue;
                    }

                    _logger.LogInformation("Syncing {RowCount} rows for table {TableName} using {ColumnCount} columns...", 
                        sourceRowCount, localFullTableName, columnMapping.Count);

                    // 4. Wipe target table (using DELETE instead of TRUNCATE to avoid CASCADE wiping skipped tables)
                    using (var deleteCmd = new NpgsqlCommand($"DELETE FROM {localFullTableName};", targetConn, transaction))
                    {
                        await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
                    }

                    // 5. Stream data using COPY
                    var sourceColumnList = string.Join(", ", columnMapping.Select(c => $"\"{c.Source}\""));
                    var targetColumnList = string.Join(", ", columnMapping.Select(c => $"\"{c.Target}\""));

                    using (var reader = await sourceConn.BeginRawBinaryCopyAsync($"COPY {sourceFullTableName} ({sourceColumnList}) TO STDOUT (FORMAT BINARY)", cancellationToken))
                    using (var writer = await targetConn.BeginRawBinaryCopyAsync($"COPY {localFullTableName} ({targetColumnList}) FROM STDIN (FORMAT BINARY)", cancellationToken))
                    {
                        await reader.CopyToAsync(writer, cancellationToken);
                    }
                    
                    _logger.LogInformation("Successfully synced table {TableName}.", localFullTableName);
                }

                // Re-enable triggers
                try
                {
                    using (var cmd = new NpgsqlCommand("SET session_replication_role = 'origin';", targetConn, transaction))
                    {
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                catch { /* Ignore */ }

                _logger.LogInformation("Committing transaction...");
                await transaction.CommitAsync(cancellationToken);

                // 6. Reset sequences after commit to ensure auto-increment IDs work correctly
                await ResetSequencesAsync(targetConn, cancellationToken);

                _logger.LogInformation("Database synchronization completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database synchronization. Rolling back changes...");
                if (transaction.Connection != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                throw;
            }
        }

        private async Task ResetSequencesAsync(NpgsqlConnection conn, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Resetting database sequences...");
            
            var sql = @"
                DO $$
                DECLARE
                    r RECORD;
                BEGIN
                    FOR r IN (SELECT table_schema, table_name, column_name, serial_sequence
                              FROM (SELECT table_schema, table_name, column_name, 
                                           pg_get_serial_sequence(quote_ident(table_schema) || '.' || quote_ident(table_name), column_name) AS serial_sequence
                                    FROM information_schema.columns
                                    WHERE table_schema = 'public') s
                              WHERE serial_sequence IS NOT NULL)
                    LOOP
                        EXECUTE 'SELECT setval(''' || r.serial_sequence || ''', COALESCE(MAX(' || quote_ident(r.column_name) || '), 1)) FROM ' || quote_ident(r.table_schema) || '.' || quote_ident(r.table_name);
                    END LOOP;
                END $$;";

            using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("All sequences have been reset.");
        }

        private async Task<(string Schema, string Table)?> FindSourceTableAsync(NpgsqlConnection conn, string localSchema, string localTable, CancellationToken cancellationToken)
        {
            // 1. Try exact match in the same schema
            using (var cmd = new NpgsqlCommand("SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema = @schema AND table_name = @table", conn))
            {
                cmd.Parameters.AddWithValue("schema", localSchema);
                cmd.Parameters.AddWithValue("table", localTable);
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                    return (reader.GetString(0), reader.GetString(1));
            }

            // 2. Try case-insensitive match in the same schema
            using (var cmd = new NpgsqlCommand("SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema = @schema AND LOWER(table_name) = LOWER(@table)", conn))
            {
                cmd.Parameters.AddWithValue("schema", localSchema);
                cmd.Parameters.AddWithValue("table", localTable);
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                    return (reader.GetString(0), reader.GetString(1));
            }

            // 3. Try case-insensitive match + ignore underscores in any non-system schema
            using (var cmd = new NpgsqlCommand(
                "SELECT table_schema, table_name FROM information_schema.tables " +
                "WHERE table_schema NOT IN ('information_schema', 'pg_catalog') " +
                "AND LOWER(REPLACE(table_name, '_', '')) = LOWER(REPLACE(@table, '_', '')) " +
                "LIMIT 1", conn))
            {
                cmd.Parameters.AddWithValue("table", localTable);
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                    return (reader.GetString(0), reader.GetString(1));
            }

            return null;
        }

        private async Task<List<string>> GetTableColumns(NpgsqlConnection conn, string schema, string table, CancellationToken cancellationToken)
        {
            var columns = new List<string>();
            using var cmd = new NpgsqlCommand(
                "SELECT column_name FROM information_schema.columns WHERE table_schema = @schema AND table_name = @table ORDER BY ordinal_position", 
                conn);
            cmd.Parameters.AddWithValue("schema", schema);
            cmd.Parameters.AddWithValue("table", table);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(reader.GetString(0));
            }

            _logger.LogDebug("Fetched {Count} columns for {Schema}.{Table} from {Host}", 
                columns.Count, schema, table, new NpgsqlConnectionStringBuilder(conn.ConnectionString).Host);

            return columns;
        }
    }
}
