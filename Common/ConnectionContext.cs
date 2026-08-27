namespace Common
{
    /// <summary>
    /// Carries per-request database connection information used to construct provider-specific <see cref="Common.Database.Database"/> instances.
    /// This type does not hold a live database connection; it only contains the DbType and ConnectionString for the current user/request.
    /// </summary>
    public class ConnectionContext
    {
        /// <summary>
        /// The target database provider type for this context.
        /// </summary>
        public Common.Enums.DbType DbType { get; set; }

        /// <summary>
        /// The connection string to use for creating database connections for this context.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
    }
}
