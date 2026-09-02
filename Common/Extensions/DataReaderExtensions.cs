using System;
using System.Data;

namespace Common.Extensions {
    /// <summary>
    /// Extension methods for reading typed values off an <see cref="IDataReader"/> by column name,
    /// so callers don't need to manually call <c>GetOrdinal</c>/<c>IsDBNull</c>/<c>Get&lt;Type&gt;</c> for every field.
    /// </summary>
    public static class DataReaderExtensions {
        /// <summary>
        /// Gets the value with type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Value with type <typeparamref name="T"/>.</returns>
        public static T GetValue<T>(this IDataReader reader, string columnName, T defaultValue) {
            var columnIndex = reader.GetOrdinal(columnName);

            return reader.IsDBNull(columnIndex) ? defaultValue : (T)reader.GetValue(columnIndex);
        }

        /// <summary>
        /// Gets the value, converting it via <paramref name="converter"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="converter">Converter applied to the raw column value.</param>
        /// <param name="defaultValue">The default value if the column is DBNull.</param>
        /// <returns>Converted value with type <typeparamref name="TResult"/>.</returns>
        public static TResult GetValue<TSource, TResult>(
            this IDataReader reader,
            string columnName,
            Func<TSource, TResult> converter,
            TResult defaultValue) {
            var columnIndex = reader.GetOrdinal(columnName);

            return reader.IsDBNull(columnIndex)
                ? defaultValue
                : converter((TSource)reader.GetValue(columnIndex));
        }

        /// <summary>
        /// Gets the string value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>Value, or null if the column is DBNull/empty.</returns>
        public static string? GetValue(this IDataReader reader, string columnName) {
            var result = reader.GetValue(columnName, string.Empty);

            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        /// <summary>
        /// Gets the value with column index.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="columnIndex">Index of the column.</param>
        /// <returns>String value.</returns>
        public static string GetValue(this IDataReader reader, int columnIndex) =>
            reader.IsDBNull(columnIndex) ? string.Empty : reader.GetValue(columnIndex).ToString()!;
    }
}
