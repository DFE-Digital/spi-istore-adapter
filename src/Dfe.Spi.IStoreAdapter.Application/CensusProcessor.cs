namespace Dfe.Spi.IStoreAdapter.Application
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions.SettingsProvider;
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
        private readonly ICensusAdapter censusAdapter;
        private readonly IDatasetQueryFilesStorageAdapter datasetQueryFilesStorageAdapter;
        private readonly ILoggerWrapper loggerWrapper;
        private readonly ITranslationApiAdapter translationApiAdapter;

        private readonly AggregationFieldsCache aggregationFieldsCache;
        private readonly string aggregationFieldsEnumerationName;

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
        /// <param name="censusProcessorSettingsProvider">
        /// An instance of type
        /// <see cref="ICensusProcessorSettingsProvider" />.
        /// </param>
        /// <param name="datasetQueryFilesStorageAdapter">
        /// An instance of type
        /// <see cref="IDatasetQueryFilesStorageAdapter" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        /// <param name="translationApiAdapter">
        /// An instance of type <see cref="ITranslationApiAdapter" />.
        /// </param>
        public CensusProcessor(
            AggregationFieldsCache aggregationFieldsCache,
            ICensusAdapter censusAdapter,
            ICensusProcessorSettingsProvider censusProcessorSettingsProvider,
            IDatasetQueryFilesStorageAdapter datasetQueryFilesStorageAdapter,
            ILoggerWrapper loggerWrapper,
            ITranslationApiAdapter translationApiAdapter)
        {
            if (censusProcessorSettingsProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(censusProcessorSettingsProvider));
            }

            this.aggregationFieldsCache = aggregationFieldsCache;
            this.censusAdapter = censusAdapter;
            this.datasetQueryFilesStorageAdapter = datasetQueryFilesStorageAdapter;
            this.loggerWrapper = loggerWrapper;
            this.translationApiAdapter = translationApiAdapter;

            this.aggregationFieldsEnumerationName =
                censusProcessorSettingsProvider.AggregationFieldsEnumerationName;
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

                GetEnumerationValuesResponse getEnumerationValuesResponse =
                    await this.translationApiAdapter.GetEnumerationValuesAsync(
                        this.aggregationFieldsEnumerationName,
                        cancellationToken)
                        .ConfigureAwait(false);

                IEnumerable<string> enumerationValues = getEnumerationValuesResponse
                    .EnumerationValuesResult
                    .EnumerationValues;

                this.loggerWrapper.Info(
                    $"Aggregation fields returned - " +
                    $"{enumerationValues.Count()} in total.");

                this.aggregationFieldsCache.AggregationFields =
                    enumerationValues;
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