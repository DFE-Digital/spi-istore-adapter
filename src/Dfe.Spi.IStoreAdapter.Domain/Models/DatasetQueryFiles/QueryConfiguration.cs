namespace Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles
{
    /// <summary>
    /// Represents the configuration aspect of a
    /// <see cref="DatasetQueryFile" />.
    /// </summary>
    public class QueryConfiguration : ModelsBase
    {
        /// <summary>
        /// Gets or sets the environment variable, holding a SQL connection
        /// string.
        /// </summary>
        public string ConnectionStringEnvironmentVariable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a database name.
        /// </summary>
        public string DatabaseName
        {
            get;
            set;
        }
    }
}