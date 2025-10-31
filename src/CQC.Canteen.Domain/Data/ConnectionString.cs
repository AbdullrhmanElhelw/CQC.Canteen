namespace CQC.Canteen.Domain.Data
{
    /// <summary>
    /// Wrapper class for the database connection string.
    /// This allows us to inject the connection string safely.
    /// </summary>
    public sealed class ConnectionString
    {
        /// <summary>
        /// The key used in the configuration file (e.g., appsettings.json)
        /// </summary>
        public const string DefaultValue = "DefaultConnection";

        /// <summary>
        /// The actual connection string value
        /// </summary>
        public string Value { get; }

        public ConnectionString(string value)
        {
            Value = value;
        }
    }
}
