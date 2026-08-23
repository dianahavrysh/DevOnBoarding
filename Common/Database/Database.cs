using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Abstract base class for provider-specific database implementations.
    /// </summary>
    public abstract class Database : IDisposable
    {
        protected readonly string _connectionString;

        protected Database(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public abstract IDbConnection CreateConnection();

        public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType = CommandType.StoredProcedure);

        public abstract IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null);

        public virtual async Task<IDataReader> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            var conn = CreateConnection();
            var cmd = CreateCommand(commandText, conn, commandType);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }

            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                // Note: CommandBehavior.CloseConnection so reader closes connection when disposed by caller
                var task = Task.Factory.StartNew(() => cmd.ExecuteReader(CommandBehavior.CloseConnection));
                return await task.ConfigureAwait(false);
            }
            catch
            {
                conn.Dispose();
                throw;
            }
        }

        public virtual async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(commandText, conn, commandType);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }

            if (conn.State != ConnectionState.Open)
                conn.Open();

            return await Task.Factory.StartNew(() => cmd.ExecuteNonQuery()).ConfigureAwait(false);
        }

        public virtual async Task<object?> ExecuteScalarAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            using var conn = CreateConnection();
            using var cmd = CreateCommand(commandText, conn, commandType);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }

            if (conn.State != ConnectionState.Open)
                conn.Open();

            return await Task.Factory.StartNew(() => cmd.ExecuteScalar()).ConfigureAwait(false);
        }

        #region IDisposable
        protected bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Nothing to dispose at base level; derived classes may hold resources.
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
