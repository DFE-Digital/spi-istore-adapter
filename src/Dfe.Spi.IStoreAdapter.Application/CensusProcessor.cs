namespace Dfe.Spi.IStoreAdapter.Application
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;

    /// <summary>
    /// Implements <see cref="ICensusProcessor" />.
    /// </summary>
    public class CensusProcessor : ICensusProcessor
    {
        private readonly IDatasetQueryFilesStorageAdapter datasetQueryFilesStorageAdapter;

        /// <summary>
        /// Initialises a new instance of the <see cref="CensusProcessor" />
        /// class.
        /// </summary>
        /// <param name="datasetQueryFilesStorageAdapter">
        /// An instance of <see cref="IDatasetQueryFilesStorageAdapter" />.
        /// </param>
        public CensusProcessor(
            IDatasetQueryFilesStorageAdapter datasetQueryFilesStorageAdapter)
        {
            this.datasetQueryFilesStorageAdapter = datasetQueryFilesStorageAdapter;
        }

        /// <inheritdoc />
        public async Task<GetCensusResponse> GetCensusAsync(
            GetCensusRequest getCensusRequest,
            CancellationToken cancellationToken)
        {
            GetCensusResponse toReturn = null;

            if (getCensusRequest == null)
            {
                throw new ArgumentNullException(nameof(getCensusRequest));
            }

            CensusIdentifier censusIdentifier =
                getCensusRequest.CensusIdentifier;

            DatasetQueryFile databaseQueryFile =
                await this.datasetQueryFilesStorageAdapter.GetDatabaseQueryFileAsync(
                    censusIdentifier.DatasetQueryFileId,
                    cancellationToken)
                    .ConfigureAwait(false);

            return toReturn;
        }
    }
}