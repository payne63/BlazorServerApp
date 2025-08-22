using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace BlazorServerApp.Services
{
    /// <summary>
    /// Simple service to connect to a Microsoft Access .accdb database and read data.
    /// Requires Microsoft Access Database Engine (ACE OLE DB) installed on the host machine.
    /// </summary>
    public class AccessDatabaseService
    {
        public record ForeignKeyInfo(string FkColumn, string PkTable, string PkColumn);
        public record ColumnInfo(string ColumnName, int DataType, bool IsNullable);

        private readonly IHostEnvironment _env;
        private readonly string _dbPath;
        private readonly string _connectionString;

        public AccessDatabaseService(IHostEnvironment env)
        {
            _env = env;
            _dbPath = Path.Combine(_env.ContentRootPath, "database", "DatabaseAVITECH.accdb");
            
            if (!File.Exists(_dbPath))
            {
                throw new FileNotFoundException($"Access database not found at '{_dbPath}'.");
            }

            // Use ACE OLEDB provider (most common). If 12.0 is not available but 16.0 is, try 16.0.
            // We can't reliably detect installed providers here, so we try 12.0 by default.
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={_dbPath};Persist Security Info=False;";
        }

        private OleDbConnection CreateConnection() => new OleDbConnection(_connectionString);

        /// <summary>
        /// Returns the list of user tables in the Access database.
        /// </summary>
        public async Task<IList<string>> GetTablesAsync()
        {
            var tables = new List<string>();
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            // Retrieve schema for tables
            DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null)!;
            foreach (DataRow row in schema.Rows)
            {
                var tableType = row["TABLE_TYPE"]?.ToString();
                var tableName = row["TABLE_NAME"]?.ToString();

                // Keep only regular user tables (exclude system tables, MSys*, etc.)
                if (string.Equals(tableType, "TABLE", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(tableName)
                    && !tableName.StartsWith("MSys", StringComparison.OrdinalIgnoreCase))
                {
                    tables.Add(tableName);
                }
            }

            // Sort alphabetically
            return tables.OrderBy(n => n).ToList();
        }

        /// <summary>
        /// Returns a DataTable with up to 'take' rows from the specified table.
        /// </summary>
        public async Task<DataTable> GetTopAsync(string tableName, int take = 100)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name is required", nameof(tableName));

            // Escape closing brackets in table name and wrap with [] to avoid injection via name.
            var escapedTable = tableName.Replace("]", "]]");

            // TOP syntax in Access: SELECT TOP 100 * FROM [TableName]
            string sql = $"SELECT TOP {Math.Max(1, take)} * FROM [{escapedTable}]";

            await using var conn = CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new OleDbCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        /// <summary>
        /// Detects foreign keys where FKTABLE_NAME = tableName.
        /// </summary>
        public async Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string tableName)
        {
            var list = new List<ForeignKeyInfo>();
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            // Access provider supports Foreign_Keys schema
            DataTable? fkSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, null);
            if (fkSchema != null)
            {
                foreach (DataRow row in fkSchema.Rows)
                {
                    var fkTable = row["FK_TABLE_NAME"]?.ToString();
                    if (!string.Equals(fkTable, tableName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var fkColumn = row["FK_COLUMN_NAME"]?.ToString();
                    var pkTable = row["PK_TABLE_NAME"]?.ToString();
                    var pkColumn = row["PK_COLUMN_NAME"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(fkColumn) && !string.IsNullOrWhiteSpace(pkTable) && !string.IsNullOrWhiteSpace(pkColumn))
                    {
                        list.Add(new ForeignKeyInfo(fkColumn!, pkTable!, pkColumn!));
                    }
                }
            }

            // Heuristic fallback if no relationships are declared in the Access file
            if (list.Count == 0)
            {
                // Guess by columns named *Id
                var cols = await GetTableColumnsAsync(tableName);
                var idCols = cols.Where(c => c.ColumnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase)).ToList();

                var allTables = await GetTablesAsync();
                foreach (var idCol in idCols)
                {
                    var raw = idCol.ColumnName;
                    var baseName = raw.Substring(0, raw.Length - 2); // remove 'Id'
                    // try to find a table matching baseName variations
                    string? target = allTables.FirstOrDefault(t => string.Equals(t, baseName, StringComparison.OrdinalIgnoreCase))
                                     ?? allTables.FirstOrDefault(t => string.Equals(t, baseName + "s", StringComparison.OrdinalIgnoreCase))
                                     ?? allTables.FirstOrDefault(t => string.Equals(t, baseName + "es", StringComparison.OrdinalIgnoreCase));
                    if (target == null) continue;

                    // choose PK column in target table
                    var targetCols = await GetTableColumnsAsync(target);
                    string? pkCandidate = new[] { "ID", target + "Id", baseName + "Id", "Id" }
                        .FirstOrDefault(n => targetCols.Any(c => string.Equals(c.ColumnName, n, StringComparison.OrdinalIgnoreCase)));
                    if (pkCandidate == null) continue;

                    list.Add(new ForeignKeyInfo(raw, target, pkCandidate));
                }
            }

            return list;
        }

        /// <summary>
        /// Gets columns and basic metadata for a table.
        /// </summary>
        public async Task<List<ColumnInfo>> GetTableColumnsAsync(string tableName)
        {
            var list = new List<ColumnInfo>();
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            DataTable cols = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object?[] { null, null, tableName, null })!;
            foreach (DataRow row in cols.Rows)
            {
                var name = row["COLUMN_NAME"]?.ToString() ?? string.Empty;
                var dataTypeObj = row["DATA_TYPE"];
                int dataType = dataTypeObj is int i ? i : 0;
                bool isNullable = string.Equals(row["IS_NULLABLE"]?.ToString(), "YES", StringComparison.OrdinalIgnoreCase);
                list.Add(new ColumnInfo(name, dataType, isNullable));
            }
            return list.OrderBy(c => c.ColumnName).ToList();
        }

        /// <summary>
        /// Tries to choose a user-friendly display column from a table's columns.
        /// Prefers text columns with common names, otherwise first text column.
        /// </summary>
        private static string? ChooseDisplayColumn(string tableName, IEnumerable<ColumnInfo> columns)
        {
            // Access OleDbType string values typically: 130 = VarWChar, 202/203 = VarChar/LongVarChar, 201 = LongVarChar, 202 = VarChar, 203 = LongVarWChar
            static bool IsTextType(int t) => t is 130 or 202 or 203 or 201;

            var preferredNames = new[]
            {
                "Name", "Libelle", "Label", "Title", "Description", "Code", "Nom", "Designation"
            };

            // 1) Try preferred names
            var byName = columns.FirstOrDefault(c => preferredNames.Any(p => string.Equals(p, c.ColumnName, StringComparison.OrdinalIgnoreCase)) && IsTextType(c.DataType));
            if (byName != null) return byName.ColumnName;

            // 2) Try first text column not containing Id
            var firstText = columns.FirstOrDefault(c => IsTextType(c.DataType) && !c.ColumnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
            if (firstText != null) return firstText.ColumnName;

            // 3) Fallback to first column
            return columns.FirstOrDefault()?.ColumnName;
        }

        /// <summary>
        /// Builds a dictionary mapping PK value => display string from a referenced table.
        /// </summary>
        public async Task<Dictionary<object, string>> BuildLookupAsync(string pkTable, string pkColumn, string? displayColumn = null)
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            // Determine display column if not provided
            if (string.IsNullOrWhiteSpace(displayColumn))
            {
                var cols = await GetTableColumnsAsync(pkTable);
                displayColumn = ChooseDisplayColumn(pkTable, cols) ?? pkColumn;
            }

            string tableEsc = pkTable.Replace("]", "]]");
            string pkEsc = pkColumn.Replace("]", "]]");
            string dispEsc = displayColumn!.Replace("]", "]]");
            string sql = $"SELECT [{pkEsc}] AS K, [{dispEsc}] AS V FROM [{tableEsc}]";
            await using var cmd = new OleDbCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var dict = new Dictionary<object, string>();
            while (await reader.ReadAsync())
            {
                var key = reader["K"];
                var valObj = reader["V"];
                var val = valObj == DBNull.Value ? string.Empty : Convert.ToString(valObj) ?? string.Empty;
                if (key != DBNull.Value)
                {
                    dict[key] = val;
                }
            }
            return dict;
        }

        /// <summary>
        /// Returns top rows and adds side-by-side display columns for FK IDs using their referenced tables.
        /// For each FK column 'X', a new string column 'X_Display' is added and filled with a human-readable value.
        /// The original FK numeric column is kept intact to avoid type conversion errors.
        /// </summary>
        public async Task<DataTable> GetTopWithLookupsAsync(string tableName, int take = 100)
        {
            var dt = await GetTopAsync(tableName, take);
            var fks = await GetForeignKeysAsync(tableName);
            if (fks.Count == 0) return dt; // nothing to enhance

            // Preload lookups for each FK
            var lookups = new Dictionary<string, Dictionary<object, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var fk in fks)
            {
                if (!dt.Columns.Contains(fk.FkColumn)) continue; // only consider FKs that exist in the result set
                var dict = await BuildLookupAsync(fk.PkTable, fk.PkColumn);
                lookups[fk.FkColumn] = dict;
            }

            // Add display columns (type string) next to FK columns without altering original values
            foreach (var fk in fks)
            {
                if (!dt.Columns.Contains(fk.FkColumn)) continue;
                var displayColName = fk.FkColumn + "_Display";
                if (!dt.Columns.Contains(displayColName))
                {
                    var displayCol = new DataColumn(displayColName, typeof(string))
                    {
                        AllowDBNull = true
                    };
                    dt.Columns.Add(displayCol);
                }
            }

            // Fill display columns
            foreach (DataRow row in dt.Rows)
            {
                foreach (var fk in fks)
                {
                    if (!dt.Columns.Contains(fk.FkColumn)) continue;
                    var displayColName = fk.FkColumn + "_Display";
                    if (!dt.Columns.Contains(displayColName)) continue;

                    var val = row[fk.FkColumn];
                    if (val == DBNull.Value)
                    {
                        row[displayColName] = null;
                        continue;
                    }

                    if (lookups.TryGetValue(fk.FkColumn, out var dict) && dict.TryGetValue(val, out var display))
                    {
                        row[displayColName] = display;
                    }
                    else
                    {
                        // Fallback: keep original value as string if no mapping is found
                        row[displayColName] = Convert.ToString(val);
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Tests opening a connection and returns basic info.
        /// </summary>
        public async Task<string> TestConnectionAsync()
        {
            try
            {
                await using var conn = CreateConnection();
                await conn.OpenAsync();
                return $"Connected to Access DB at '{_dbPath}' using provider 'Microsoft.ACE.OLEDB.12.0'.";
            }
            catch (OleDbException ex)
            {
                return "Erreur de connexion à la base Access: " + ex.Message + "\nAssurez-vous que Microsoft Access Database Engine (ACE OLEDB 12.0/16.0) est installé.";
            }
        }
    }
}
