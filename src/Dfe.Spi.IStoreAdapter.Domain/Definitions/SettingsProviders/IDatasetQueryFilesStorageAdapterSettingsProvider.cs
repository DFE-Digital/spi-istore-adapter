namespace Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders
{
    /// <summary>
    /// Describes the operations of the
    /// <see cref="IDatasetQueryFilesStorageAdapter" /> settings provider.
    /// </summary>
    public interface IDatasetQueryFilesStorageAdapterSettingsProvider
    {
        /// <summary>
        /// Gets the storage connection string containing database query files.
        /// </summary>
        string DatasetQueryFilesStorageConnectionString
        {
            get;
        }

        /// <summary>
        /// Gets the storage container name containing database query files.
        /// </summary>
        string DatasetQueryFileStorageContainerName
        {
            get;
        }

        /// <summary>
        /// Gets the name of the file containing the SQL statement aspect
        /// of the database query file.
        /// </summary>
        string DatasetQueryFileQueryFilename
        {
            get;
        }

        /// <summary>
        /// Gets the name of the file containing the configuration aspect of
        /// the database query file.
        /// </summary>
        string DatasetQueryFileConfigFilename
        {
            get;
        }
    }
}