namespace Dfe.Spi.IStoreAdapter.Application.Factories
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions.Factories;
    using Dfe.Spi.IStoreAdapter.Application.Models;
    using Dfe.Spi.IStoreAdapter.Domain.Models;

    /// <summary>
    /// Implements <see cref="IAggregatorFactory" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AggregatorFactory : IAggregatorFactory
    {
        private readonly AggregationFieldsCache aggregationFieldsCache;

        /// <summary>
        /// Initialises a new instance of the <see cref="AggregatorFactory" />
        /// class.
        /// </summary>
        /// <param name="aggregationFieldsCache">
        /// An instance of <see cref="AggregationFieldsCache" />.
        /// </param>
        public AggregatorFactory(AggregationFieldsCache aggregationFieldsCache)
        {
            this.aggregationFieldsCache = aggregationFieldsCache;
        }

        /// <inheritdoc />
        public IAggregator Create(
            IEnumerable<string> resultSetFieldNames,
            string requestedQueryName,
            AggregateQuery aggregateQuery)
        {
            Aggregator toReturn = new Aggregator(
                resultSetFieldNames,
                this.aggregationFieldsCache,
                requestedQueryName,
                aggregateQuery);

            return toReturn;
        }
    }
}