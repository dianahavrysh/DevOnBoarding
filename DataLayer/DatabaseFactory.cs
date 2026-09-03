using Common;
using Common.Database;
using Common.Enums;
using Common.Interfaces;
using DataLayer.MsSql;
using DataLayer.MySql;
using System;

namespace DataLayer {
    /// <summary>
    /// Factory that routes database creation to the correct provider-specific
    /// implementation based on <see cref="ConnectionContext.DbType"/>.
    /// </summary>
    public class DatabaseFactory : IDatabaseFactory {
        /// <inheritdoc/>
        public Database CreateDatabase(ConnectionContext context) {
            return context.DbType switch {
                DbType.MSSQL => new MsSqlDatabase(context.ConnectionString),
                DbType.MySQL => new MySqlDatabase(context.ConnectionString),
                _ => throw new NotSupportedException($"Unsupported database type: {context.DbType}")
            };
        }
    }
}
