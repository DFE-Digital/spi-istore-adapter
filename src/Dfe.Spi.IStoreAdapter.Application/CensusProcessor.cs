namespace Dfe.Spi.IStoreAdapter.Application
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Models;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Implements <see cref="ICensusProcessor" />.
    /// </summary>
    public class CensusProcessor : ICensusProcessor
    {
        private readonly AggregationFieldsCache aggregationFieldsCache;
        private readonly ICensusAdapter censusAdapter;
        private readonly IDatasetQueryFilesStorageAdapter datasetQueryFilesStorageAdapter;
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="CensusProcessor" />
        /// class.
        /// </summary>
        /// <param name="aggregationFieldsCache">
        /// An instance of <see cref="AggregationFieldsCache" />.
        /// </param>
        /// <param name="censusAdapter">
        /// An instance of type <see cref="ICensusAdapter" />.
        /// </param>
        /// <param name="datasetQueryFilesStorageAdapter">
        /// An instance of type
        /// <see cref="IDatasetQueryFilesStorageAdapter" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public CensusProcessor(
            AggregationFieldsCache aggregationFieldsCache,
            ICensusAdapter censusAdapter,
            IDatasetQueryFilesStorageAdapter datasetQueryFilesStorageAdapter,
            ILoggerWrapper loggerWrapper)
        {
            this.aggregationFieldsCache = aggregationFieldsCache;
            this.censusAdapter = censusAdapter;
            this.datasetQueryFilesStorageAdapter = datasetQueryFilesStorageAdapter;
            this.loggerWrapper = loggerWrapper;
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

            string datasetQueryFileId = censusIdentifier.DatasetQueryFileId;

            this.loggerWrapper.Debug(
                $"Pulling back {nameof(DatasetQueryFile)} with id " +
                $"\"{datasetQueryFileId}\"...");

            DatasetQueryFile datasetQueryFile =
                await this.datasetQueryFilesStorageAdapter.GetDatabaseQueryFileAsync(
                    censusIdentifier.DatasetQueryFileId,
                    cancellationToken)
                    .ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"Got {nameof(DatasetQueryFile)} for id " +
                $"\"{datasetQueryFileId}\": {datasetQueryFile}.");

            // Get an entire list of aggregation fields that we can use.
            if (this.aggregationFieldsCache.AggregationFields == null)
            {
                this.loggerWrapper.Debug(
                    "Fetching available aggregation fields from the " +
                    "Translator API...");

                // TODO: Then get from the Translator API the aggregation
                //       fields, and cache 'em.
            }

            IEnumerable<string> aggregationFields =
                this.aggregationFieldsCache.AggregationFields;

            string parameterName = censusIdentifier.ParameterName;
            string parameterValue = censusIdentifier.ParameterValue;

            Dictionary<string, AggregateQuery> aggregateQueries =
                getCensusRequest.AggregateQueries;

            this.loggerWrapper.Debug(
                $"Fetching {nameof(Census)} using {datasetQueryFile} and " +
                $"supplied aggregate queries...");

            Census census = await this.censusAdapter.GetCensusAsync(
                aggregationFields,
                datasetQueryFile,
                aggregateQueries,
                parameterName,
                parameterValue,
                this.BuildCensusResults,
                cancellationToken)
                .ConfigureAwait(false);

            this.loggerWrapper.Info(
                $"Fetched {nameof(Census)}: {census} using " +
                $"{datasetQueryFile} and supplied aggregate queries.");

            toReturn = new GetCensusResponse()
            {
                Census = census,
            };

            return toReturn;
        }

        private Census BuildCensusResults(DbDataReader dbDataReader)
        {
            Census toReturn = null;

            toReturn = new Census()
            {
                // Nothing for now...
            };

            return toReturn;
        }
    }
}