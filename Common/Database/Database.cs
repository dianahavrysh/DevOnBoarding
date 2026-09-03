using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Common.Database {
    /// <summary>
    /// Abstract database implementation that contains common command execution logic.
    /// Provider-specific implementations supply connections, parameters and low-level execution.
    /// </summary>
    public abstract class Database {
        /// <summary>
        /// The connection string used to connect to the database.
        /// </summary>
        protected readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class with the specified connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        protected Database(string connectionString) {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Creates a provider-specific database connection.
        /// </summary>
        public abstract IDbConnection CreateConnection();

        /// <summary>
        /// Creates a command for the specified connection.
        /// </summary>
        public virtual IDbCommand CreateCommand(
            string commandText,
            IDbConnection connection,
            CommandType commandType = CommandType.StoredProcedure) {
            var command = connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandType = commandType;

            return command;
        }

        /// <summary>
        /// Creates a provider-specific database parameter.
        /// </summary>
        public abstract IDataParameter CreateParameter(
            string name,
            object? value,
            DbType? type = null);

        /// <summary>
        /// Creates a provider-specific output parameter.
        /// </summary>
        public abstract IDataParameter CreateOutputParameter(
            string name,
            DbType type);

        /// <summary>
        /// Executes a command and returns a data reader.
        /// The connection is closed when the reader is disposed.
        /// </summary>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(
            string commandText,
            CommandType commandType = CommandType.StoredProcedure,
            IEnumerable<IDataParameter>? parameters = null) {
            var connection = CreateConnection();

            try {
                using var command = CreateCommand(
                    commandText,
                    connection,
                    commandType);

                AddParameters(command, parameters);

                await OpenAsync(connection);

                return await ExecuteReaderCoreAsync(command);
            }
            catch {
                connection.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Executes a command that does not return rows.
        /// </summary>
        public virtual async Task<int> ExecuteNonQueryAsync(
            string commandText,
            CommandType commandType = CommandType.StoredProcedure,
            IEnumerable<IDataParameter>? parameters = null) {
            using var connection = CreateConnection();

            await OpenAsync(connection);

            using var command = CreateCommand(
                commandText,
                connection,
                commandType);

            AddParameters(command, parameters);

            return await ExecuteNonQueryCoreAsync(command);
        }

        /// <summary>
        /// Executes a command and returns the first column of the first row.
        /// </summary>
        public virtual async Task<object?> ExecuteScalarAsync(
            string commandText,
            CommandType commandType = CommandType.StoredProcedure,
            IEnumerable<IDataParameter>? parameters = null) {
            using var connection = CreateConnection();

            await OpenAsync(connection);

            using var command = CreateCommand(
                commandText,
                connection,
                commandType);

            AddParameters(command, parameters);

            return await ExecuteScalarCoreAsync(command);
        }

        /// <summary>
        /// Executes a command and converts the scalar result to the requested type.
        /// </summary>
        public virtual async Task<T?> ExecuteScalarAsync<T>(
            string commandText,
            CommandType commandType = CommandType.StoredProcedure,
            IEnumerable<IDataParameter>? parameters = null) {
            var value = await ExecuteScalarAsync(
                commandText,
                commandType,
                parameters);

            if (value == null || value == DBNull.Value) {
                return default;
            }

            if (value is T result) {
                return result;
            }

            if (typeof(T) == typeof(Guid) &&
                Guid.TryParse(value.ToString(), out var guid)) {
                return (T)(object)guid;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Adds the specified parameters to the command.
        /// </summary>
        private static void AddParameters(
            IDbCommand command,
            IEnumerable<IDataParameter>? parameters) {
            if (parameters == null) {
                return;
            }

            foreach (var parameter in parameters) {
                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Opens the specified connection asynchronously.
        /// </summary>
        protected abstract Task OpenAsync(IDbConnection connection);

        /// <summary>
        /// Executes the command and returns a data reader asynchronously.
        /// </summary>
        protected abstract Task<DbDataReader> ExecuteReaderCoreAsync(
            IDbCommand command);

        /// <summary>
        /// Executes the command that does not return rows asynchronously.
        /// </summary>
        protected abstract Task<int> ExecuteNonQueryCoreAsync(
            IDbCommand command);

        /// <summary>
        /// Executes the command and returns the first column of the first row asynchronously.
        /// </summary>
        protected abstract Task<object?> ExecuteScalarCoreAsync(
            IDbCommand command);
    }
}
