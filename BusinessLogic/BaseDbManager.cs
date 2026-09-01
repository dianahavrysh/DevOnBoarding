using Common;
using Common.Database;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DAL {
    /// <summary>
    /// Base class for database-backed managers.
    /// </summary>
    public abstract class BaseDbManager {
        private readonly Database _db;

        protected BaseDbManager(
            IDatabaseFactory factory,
            ConnectionContext connectionContext) {
            _db = factory.CreateDatabase(connectionContext);
        }

        protected IDataParameter CreateParam(
            string name,
            object? value) {
            return _db.CreateParameter(name, value);
        }

        protected IDataParameter CreateOutputParam(
            string name,
            DbType type) {
            return _db.CreateOutputParameter(name, type);
        }

        protected Task<int> ExecuteNonQueryAsync(
            string commandText,
            IEnumerable<IDataParameter>? parameters = null) {
            return _db.ExecuteNonQueryAsync(
                commandText,
                parameters: parameters);
        }

        protected Task<IDataReader> ExecuteReaderAsync(
            string commandText,
            IEnumerable<IDataParameter>? parameters = null) {
            return _db.ExecuteReaderAsync(
                commandText,
                parameters: parameters);
        }

        protected Task<T?> ExecuteScalarAsync<T>(
            string commandText,
            IEnumerable<IDataParameter>? parameters = null) {
            return _db.ExecuteScalarAsync<T>(
                commandText,
                parameters: parameters);
        }
    }
}
