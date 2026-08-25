using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Common.Database
{
    /// <summary>
    /// Abstract base class for provider-specific database implementations.
    /// Provides common async helpers for executing commands and queries.
    /// Concrete providers must implement connection and command creation.
    /// </summary>
    public abstract class Database : IDisposable
    {
        protected readonly string _connectionString;

        protected Database(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Create a new <see cref="IDbConnection"/> for the underlying provider.
        /// </summary>
        /// <returns>A closed <see cref="IDbConnection"/> instance ready to be opened.</returns>
        public abstract IDbConnection CreateConnection();

        /// <summary>
        /// Create an <see cref="IDbCommand"/> for the specified connection.
        /// </summary>
        /// <param name="commandText">The command text or stored procedure name.</param>
        /// <param name="connection">The connection to associate with the command.</param>
        /// <param name="commandType">The <see cref="CommandType"/> (Text or StoredProcedure).</param>
        /// <returns>An initialized <see cref="IDbCommand"/> instance.</returns>
        public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType = CommandType.StoredProcedure);

        /// <summary>
        /// Create a provider-specific parameter for commands.
        /// </summary>
        /// <param name="name">Parameter name including any provider prefix (e.g. "@Name").</param>
        /// <param name="value">Parameter value or <c>null</c> to represent <see cref="DBNull.Value"/>.</param>
        /// <param name="type">Optional <see cref="DbType"/> for the parameter.</param>
        /// <returns>An <see cref="IDataParameter"/> instance.</returns>
        public abstract IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null);

        /// <summary>
        /// Execute a command that returns a data reader asynchronously.
        /// The caller must dispose the returned <see cref="IDataReader"/>; the underlying connection will be closed when the reader is disposed.
        /// </summary>
        /// <param name="commandText">Command text or stored procedure name.</param>
        /// <param name="commandType">Command type (Text or StoredProcedure).</param>
        /// <param name="parameters">Optional parameters to add to the command.</param>
        /// <returns>An open <see cref="IDataReader"/> for reading results.</returns>
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

                
                var task = Task.Factory.StartNew(() => cmd.ExecuteReader(CommandBehavior.CloseConnection));
                return await task.ConfigureAwait(false);
            }
            catch
            {
                conn.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Execute a command that does not return rows (INSERT/UPDATE/DELETE) asynchronously.
        /// </summary>
        /// <param name="commandText">Command text or stored procedure name.</param>
        /// <param name="commandType">Command type.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <returns>The number of affected rows.</returns>
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

        /// <summary>
        /// Execute a command and return the first column of the first row in the result set.
        /// </summary>
        /// <param name="commandText">Command text or stored procedure name.</param>
        /// <param name="commandType">Command type.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <returns>The first column of the first row, or <c>null</c> if no result.</returns>
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

        /// <summary>
        /// Dispose pattern implementation.
        /// </summary>
        /// <param name="disposing">True when called from Dispose, false when called from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                
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

