namespace Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles
{
    /// <summary>
    /// Represents a dataset query file, used in pulling back a dataset for
    /// aggregation purposes.
    /// </summary>
    public class DatasetQueryFile : ModelsBase
    {
        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="DatasetQueryFiles.QueryConfiguration" />.
        /// </summary>
        public QueryConfiguration QueryConfiguration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the SQL query, used in pulling back the dataset.
        /// </summary>
        public string Query
        {
            get;
            set;
        }
    }
}