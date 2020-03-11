namespace Dfe.Spi.IStoreAdapter.Application.Factories
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions.Factories;
    using Dfe.Spi.IStoreAdapter.Domain.Models;

    /// <summary>
    /// Implements <see cref="IAggregatorFactory" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AggregatorFactory : IAggregatorFactory
    {
        /// <inheritdoc />
        public IAggregator Create(
            IEnumerable<string> resultSetFieldNames,
            string requestedQueryName,
            AggregateQuery aggregateQuery)
        {
            Aggregator toReturn = new Aggregator(
                resultSetFieldNames,
                requestedQueryName,
                aggregateQuery);

            return toReturn;
        }
    }
}