using System;
using System.Data;

namespace Common
{
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Safely get a typed value from an <see cref="IDataReader"/> by column name.
        /// Returns <paramref name="defaultValue"/> if the column does not exist, contains <see cref="DBNull"/>, or conversion fails.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="reader">The data reader instance.</param>
        /// <param name="columnName">The column name to read.</param>
        /// <param name="defaultValue">Default value to return when reading fails.</param>
        /// <returns>Value from the reader converted to <typeparamref name="T"/>, or <paramref name="defaultValue"/>.</returns>
        public static T GetValue<T>(this IDataReader reader, string columnName, T defaultValue = default!)
        {
            var ordinal = -1;
            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException)
            {
                return defaultValue;
            }

            if (reader.IsDBNull(ordinal))
                return defaultValue;

            var val = reader.GetValue(ordinal);
            if (val is T t)
                return t;

            
            try
            {
                if (typeof(T) == typeof(Guid))
                {
                    var s = val.ToString();
                    if (Guid.TryParse(s, out var g))
                        return (T)(object)g;
                    return defaultValue;
                }

                if (typeof(T) == typeof(Guid?))
                {
                    var s = val.ToString();
                    if (Guid.TryParse(s, out var g))
                        return (T)(object)(Guid?)g;
                    return defaultValue;
                }

                if (typeof(T).IsEnum)
                    return (T)Enum.Parse(typeof(T), val.ToString()!, true);

                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}

