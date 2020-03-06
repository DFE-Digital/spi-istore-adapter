namespace Dfe.Spi.IStoreAdapter.FunctionApp.SettingsProviders
{
    using System;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders;

    /// <summary>
    /// Implements
    /// <see cref="IDatasetQueryFilesStorageAdapterSettingsProvider" />.
    /// </summary>
    public class DatasetQueryFilesStorageAdapterSettingsProvider
        : IDatasetQueryFilesStorageAdapterSettingsProvider
    {
        /// <inheritdoc />
        public string DatasetQueryFilesStorageConnectionString
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    "DatasetQueryFilesStorageConnectionString");

                return toReturn;
            }
        }

        /// <inheritdoc />
        public string DatasetQueryFileStorageContainerName
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    "DatasetQueryFileStorageContainerName");

                return toReturn;
            }
        }

        /// <inheritdoc />
        public string DatasetQueryFileQueryFilename
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    "DatasetQueryFileQueryFilename");

                return toReturn;
            }
        }

        /// <inheritdoc />
        public string DatasetQueryFileConfigFilename
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    "DatasetQueryFileConfigFilename");

                return toReturn;
            }
        }
    }
}