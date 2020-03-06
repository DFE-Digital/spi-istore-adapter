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
    using Dfe.Spi.IStoreAdapter.Domain;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

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
        private readonly OperationContext operationContext;

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

            this.operationContext = new OperationContext();
        }

        /// <inheritdoc />
        public async Task<DatasetQueryFile> GetDatabaseQueryFileAsync(
            string datasetQueryFileId,
            CancellationToken cancellationToken)
        {
            DatasetQueryFile toReturn = null;

            CloudBlobDirectory cloudBlobDirectory =
                await this.GetDatasetQueryFileDirectory(datasetQueryFileId)
                .ConfigureAwait(false);

            if (cloudBlobDirectory == null)
            {
                throw new DatasetQueryFileNotFoundException(
                    datasetQueryFileId);
            }

            // List all the files in the directory.
            IEnumerable<IListBlobItem> listBlobItems =
                await cloudBlobDirectory.Container
                    .ListBlobsAsync(cloudBlobDirectory.Prefix)
                    .ConfigureAwait(false);

            IEnumerable<CloudBlockBlob> cloudBlockBlobs =
                listBlobItems
                    .Where(x => x is CloudBlockBlob)
                    .Cast<CloudBlockBlob>();

            CloudBlockBlob queryFile = cloudBlockBlobs.SingleOrDefault(
                x => x.Name.EndsWith(
                    this.datasetQueryFileQueryFilename,
                    StringComparison.InvariantCulture));

            if (queryFile == null)
            {
                throw new IncompleteDatasetQueryFileException(
                    this.datasetQueryFileQueryFilename);
            }

            string query =
                await this.GetFileContentsAsString(
                    queryFile,
                    cancellationToken)
                    .ConfigureAwait(false);

            CloudBlockBlob configFile = cloudBlockBlobs.SingleOrDefault(
                x => x.Name.EndsWith(
                    this.datasetQueryFileConfigFilename,
                    StringComparison.InvariantCulture));

            if (configFile == null)
            {
                throw new IncompleteDatasetQueryFileException(
                    this.datasetQueryFileConfigFilename);
            }

            string configurationStr = await this.GetFileContentsAsString(
                configFile,
                cancellationToken)
                .ConfigureAwait(false);

            QueryConfiguration queryConfiguration =
                JsonConvert.DeserializeObject<QueryConfiguration>(configurationStr);

            toReturn = new DatasetQueryFile()
            {
                Query = query,
                QueryConfiguration = queryConfiguration,
            };

            return toReturn;
        }

        private async Task<string> GetFileContentsAsString(
            CloudBlockBlob cloudBlockBlob,
            CancellationToken cancellationToken)
        {
            string toReturn = null;

            byte[] configFileBytes =
                new byte[cloudBlockBlob.Properties.Length];

            this.loggerWrapper.Debug(
                $"{cloudBlockBlob.Name}: Downloading " +
                $"{configFileBytes.Length} byte(s)...");

            BlobRequestOptions blobRequestOptions = null;
            await cloudBlockBlob.DownloadToByteArrayAsync(
                configFileBytes,
                0,
                AccessCondition.GenerateEmptyCondition(),
                blobRequestOptions,
                this.operationContext,
                cancellationToken)
                .ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"File downloaded ({configFileBytes.Length} byte(s)). " +
                $"Deserialising into a managable format...");

            using (MemoryStream memoryStream = new MemoryStream(configFileBytes))
            {
                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    toReturn = streamReader.ReadToEnd();
                }
            }

            return toReturn;
        }

        private async Task<CloudBlobDirectory> GetDatasetQueryFileDirectory(
            string datasetQueryFileId)
        {
            CloudBlobDirectory toReturn = null;

            CloudBlobContainer cloudBlobContainer =
                await this.GetContainerAsync(
                    this.datasetQueryFileStorageContainerName)
                    .ConfigureAwait(false);

            IEnumerable<IListBlobItem> listBlobItems = await cloudBlobContainer
                .ListBlobsAsync()
                .ConfigureAwait(false);

            string prefix = $"{datasetQueryFileId}/";

            toReturn = listBlobItems
                .Where(x => x is CloudBlobDirectory)
                .Cast<CloudBlobDirectory>()
                .SingleOrDefault(x => x.Prefix == prefix);

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