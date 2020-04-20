namespace Dfe.Spi.IStoreAdapter.Application
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions.Factories;
    using Dfe.Spi.IStoreAdapter.Application.Definitions.SettingsProvider;
    using Dfe.Spi.IStoreAdapter.Application.Exceptions;
    using Dfe.Spi.IStoreAdapter.Application.Models;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;
    using Dfe.Spi.Models;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Implements <see cref="ICensusProcessor" />.
    /// </summary>
    public class CensusProcessor : ICensusProcessor
    {
        private readonly IAggregatorFactory aggregatorFactory;
        private readonly ICensusAdapter censusAdapter;
        private readonly IDatasetQueryFilesStorageAdapter datasetQueryFilesStorageAdapter;
        private readonly ILoggerWrapper loggerWrapper;
        private readonly ITranslationApiAdapter translationApiAdapter;

        private readonly AggregationFieldsCache aggregationFieldsCache;
        private readonly string aggregationFieldsAdapterName;
        private readonly string aggregationFieldsEnumerationName;

        /// <summary>
        /// Initialises a new instance of the <see cref="CensusProcessor" />
        /// class.
        /// </summary>
        /// <param name="aggregationFieldsCache">
        /// An instance of <see cref="AggregationFieldsCache" />.
        /// </param>
        /// <param name="aggregatorFactory">
        /// An instance of type <see cref="IAggregatorFactory" />.
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
            IAggregatorFactory aggregatorFactory,
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

            this.aggregatorFactory = aggregatorFactory;
            this.aggregationFieldsCache = aggregationFieldsCache;
            this.censusAdapter = censusAdapter;
            this.datasetQueryFilesStorageAdapter = datasetQueryFilesStorageAdapter;
            this.loggerWrapper = loggerWrapper;
            this.translationApiAdapter = translationApiAdapter;

            this.aggregationFieldsAdapterName =
                censusProcessorSettingsProvider.AggregationFieldsAdapterName;
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

            Dictionary<string, AggregateQuery> aggregateQueries =
                getCensusRequest.AggregateQueries;

            Census census = null;
            if (aggregateQueries != null)
            {
                census = await this.GetCensusAggregates(
                    getCensusRequest,
                    datasetQueryFile,
                    cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                census = new Census();
            }

            census.Name = $"Census results from " +
                $"{datasetQueryFile.QueryConfiguration.DatabaseName}";

            this.loggerWrapper.Info(
                $"Fetched {nameof(Census)}: {census} using " +
                $"{datasetQueryFile} and supplied aggregate queries.");

            toReturn = new GetCensusResponse()
            {
                Census = census,
            };

            return toReturn;
        }

        /// <inheritdoc />
        public async Task<GetCensusesResponse> GetCensusesAsync(
            GetCensusesRequest getCensusesRequest,
            CancellationToken cancellationToken)
        {
            var censuses = new Census[getCensusesRequest.CensusIdentifiers.Length];
            
            for (var i = 0; i < censuses.Length; i++)
            {
                var singleRequest = new GetCensusRequest
                {
                    CensusIdentifier = getCensusesRequest.CensusIdentifiers[i],
                    AggregateQueries = getCensusesRequest.AggregateQueries,
                };
                
                var singleResponse = await GetCensusAsync(singleRequest, cancellationToken);
                
                censuses[i] = singleResponse.Census;
            }

            return new GetCensusesResponse
            {
                Censuses = censuses,
            };
        }

        
        
        private static void AssertFieldsAreSupported(
            IEnumerable<string> aggregationFields,
            Dictionary<string, AggregateQuery> aggregateQueries)
        {
            // First, get a distinct list of all the requested columns.
            string[] requestedFields = aggregateQueries
                .SelectMany(x => x.Value.DataFilters)
                .Select(x => x.Field)
                .Distinct()
                .ToArray();

            // Then, check if any columns been requested that are not
            // available.
            string[] unsupportedFields = requestedFields
                .Where(x => !aggregationFields.Any(y => y == x))
                .ToArray();

            if (unsupportedFields.Length > 0)
            {
                throw new UnsupportedAggregateColumnRequestException(
                    unsupportedFields,
                    aggregationFields);
            }
        }

        private static List<string> GetResultSetFieldNames(
            DbDataReader dbDataReader)
        {
            List<string> toReturn = new List<string>();

            string fieldName = null;
            for (int i = 0; i < dbDataReader.FieldCount; i++)
            {
                fieldName = dbDataReader.GetName(i);

                toReturn.Add(fieldName);
            }

            return toReturn;
        }

        private static Dictionary<string, Type> ConvertMappingsResponseToFieldsAndTypes(
            GetEnumerationMappingsResponse getEnumerationMappingsResponse)
        {
            Dictionary<string, Type> toReturn = null;

            toReturn = getEnumerationMappingsResponse
                .MappingsResult
                .Mappings
                .ToDictionary(
                    x => x.Key,
                    x =>
                    {
                        Type type = null;

                        string typeStr = x.Value.FirstOrDefault();

                        type = Type.GetType(typeStr);

                        if (type == null)
                        {
                            throw new InvalidMappingTypeException(typeStr);
                        }

                        return type;
                    });

            return toReturn;
        }

        private Census BuildCensusResults(
            Dictionary<string, AggregateQuery> aggregateQueries,
            DbDataReader dbDataReader)
        {
            Census toReturn = null;

            // Extract the field names in the result set.
            List<string> resultSetFieldNames =
                GetResultSetFieldNames(dbDataReader);

            IAggregator[] aggregators = aggregateQueries
                .Select(x => this.aggregatorFactory.Create(
                    resultSetFieldNames,
                    x.Key,
                    x.Value))
                .ToArray();

            this.loggerWrapper.Info(
                $"{aggregators.Length} {nameof(IAggregator)}s created. " +
                $"Cycling through rows...");

            int rowNumber = 0;
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            while (dbDataReader.Read())
            {
                foreach (IAggregator aggregator in aggregators)
                {
                    aggregator.ProcessRow(dbDataReader);
                }

                rowNumber++;
            }

            stopwatch.Stop();

            TimeSpan elapsed = stopwatch.Elapsed;

            this.loggerWrapper.Info(
                $"{rowNumber} row(s) cycled through in {elapsed}.");

            this.loggerWrapper.Debug(
                $"Quizzing {nameof(IAggregator)}s for results...");

            Aggregation[] aggregations = aggregators
                .Select(x => x.GetResult())
                .ToArray();

            this.loggerWrapper.Info(
                $"{aggregations.Length} {nameof(Aggregation)}(s) returned.");

            toReturn = new Census()
            {
                _Aggregations = aggregations.ToArray(),
            };

            return toReturn;
        }

        private async Task<Census> GetCensusAggregates(
            GetCensusRequest getCensusRequest,
            DatasetQueryFile datasetQueryFile,
            CancellationToken cancellationToken)
        {
            Census toReturn = null;

            CensusIdentifier censusIdentifier =
                getCensusRequest.CensusIdentifier;

            // Get an entire list of aggregation fields that we can use.
            Dictionary<string, Type> aggregationFieldsAndTypes = null;
            if (this.aggregationFieldsCache.AggregationFieldsAndTypes == null)
            {
                this.loggerWrapper.Debug(
                    "Fetching available aggregation mappings from the " +
                    "Translator API...");

                GetEnumerationMappingsResponse getEnumerationMappingsResponse =
                    await this.translationApiAdapter.GetEnumerationMappingsAsync(
                        this.aggregationFieldsEnumerationName,
                        this.aggregationFieldsAdapterName,
                        cancellationToken)
                        .ConfigureAwait(false);

                aggregationFieldsAndTypes =
                    ConvertMappingsResponseToFieldsAndTypes(
                        getEnumerationMappingsResponse);

                this.loggerWrapper.Info(
                    $"Aggregation fields and types obtained - " +
                    $"{aggregationFieldsAndTypes.Count} in total.");

                this.aggregationFieldsCache.AggregationFieldsAndTypes =
                    aggregationFieldsAndTypes;
            }

            aggregationFieldsAndTypes =
                this.aggregationFieldsCache.AggregationFieldsAndTypes;

            IEnumerable<string> aggregationFields =
                aggregationFieldsAndTypes.Keys.ToList();

            string parameterName = censusIdentifier.ParameterName;
            string parameterValue = censusIdentifier.ParameterValue;

            Dictionary<string, AggregateQuery> aggregateQueries =
                getCensusRequest.AggregateQueries;

            AssertFieldsAreSupported(aggregationFields, aggregateQueries);

            this.loggerWrapper.Debug(
                $"Fetching {nameof(Census)} using {datasetQueryFile} and " +
                $"supplied aggregate queries...");

            toReturn = await this.censusAdapter.GetCensusAsync(
                aggregationFields,
                datasetQueryFile,
                aggregateQueries,
                parameterName,
                parameterValue,
                this.BuildCensusResults,
                cancellationToken)
                .ConfigureAwait(false);

            return toReturn;
        }
    }
}