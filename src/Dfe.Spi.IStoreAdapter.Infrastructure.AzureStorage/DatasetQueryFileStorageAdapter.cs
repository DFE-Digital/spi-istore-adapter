namespace Dfe.Spi.IStoreAdapter.Infrastructure.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.AzureStorage;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Implements <see cref="IDatasetQueryFilesStorageAdapter" />.
    /// </summary>
    public class DatasetQueryFileStorageAdapter
        : IDatasetQueryFilesStorageAdapter
    {
        private readonly ILoggerWrapper loggerWrapper;

        private readonly CloudBlobClient cloudBlobClient;
        private readonly string datasetQueryFileStorageContainerName;
        private readonly string datasetQueryFileConfigFilename;
        private readonly string datasetQueryFileQueryFilename;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="DatasetQueryFileStorageAdapter" /> class.
        /// </summary>
        /// <param name="datasetQueryFilesStorageAdapterSettingsProvider">
        /// An instance of type
        /// <see cref="IDatasetQueryFilesStorageAdapterSettingsProvider" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public DatasetQueryFileStorageAdapter(
            IDatasetQueryFilesStorageAdapterSettingsProvider datasetQueryFilesStorageAdapterSettingsProvider,
            ILoggerWrapper loggerWrapper)
        {
            if (datasetQueryFilesStorageAdapterSettingsProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(datasetQueryFilesStorageAdapterSettingsProvider));
            }

            this.loggerWrapper = loggerWrapper;

            string datasetQueryFilesStorageConnectionString =
                datasetQueryFilesStorageAdapterSettingsProvider.DatasetQueryFilesStorageConnectionString;

            CloudStorageAccount cloudStorageAccount =
                CloudStorageAccount.Parse(datasetQueryFilesStorageConnectionString);

            this.cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            this.datasetQueryFileStorageContainerName =
                datasetQueryFilesStorageAdapterSettingsProvider.DatasetQueryFileStorageContainerName;
            this.datasetQueryFileConfigFilename =
                datasetQueryFilesStorageAdapterSettingsProvider.DatasetQueryFileConfigFilename;
            this.datasetQueryFileQueryFilename =
                datasetQueryFilesStorageAdapterSettingsProvider.DatasetQueryFileQueryFilename;
        }

        /// <inheritdoc />
        public async Task<DatasetQueryFile> GetDatabaseQueryFileAsync(
            string datasetQueryFileId,
            CancellationToken cancellationToken)
        {
            DatasetQueryFile toReturn = null;

            CloudBlobContainer cloudBlobContainer =
                await this.GetContainerAsync(
                    this.datasetQueryFileStorageContainerName)
                    .ConfigureAwait(false);

            IEnumerable<IListBlobItem> listBlobItems = await cloudBlobContainer
                .ListBlobsAsync()
                .ConfigureAwait(false);

            string prefix = $"{datasetQueryFileId}/";

            CloudBlobDirectory cloudBlobDirectory = listBlobItems
                .Where(x => x is CloudBlobDirectory)
                .Cast<CloudBlobDirectory>()
                .SingleOrDefault(x => x.Prefix == prefix);

            if (cloudBlobDirectory == null)
            {
                throw new FileNotFoundException(
                    $"Could not find directory for " +
                    $"{nameof(datasetQueryFileId)} = " +
                    $"\"{datasetQueryFileId}\".");
            }

            // TODO: Get the files out, return 'em.
            return toReturn;
        }

        private async Task<CloudBlobContainer> GetContainerAsync(
            string container)
        {
            CloudBlobContainer toReturn = null;

            this.loggerWrapper.Debug(
                $"Getting container reference for \"{container}\"...");

            toReturn = this.cloudBlobClient.GetContainerReference(container);

            await toReturn.CreateIfNotExistsAsync().ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"Container reference for \"{container}\" obtained.");

            return toReturn;
        }
    }
}