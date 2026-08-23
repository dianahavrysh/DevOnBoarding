using System;
using System.Data;

namespace Common
{
    public static class DataReaderExtensions
    {
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

            // handle common conversions
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
