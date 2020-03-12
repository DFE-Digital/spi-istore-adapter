namespace Dfe.Spi.IStoreAdapter.Application.Definitions.Factories
{
    using System.Collections.Generic;
    using Dfe.Spi.IStoreAdapter.Domain.Models;

    /// <summary>
    /// Describes the operations of the <see cref="IAggregator" /> factory.
    /// </summary>
    public interface IAggregatorFactory
    {
        /// <summary>
        /// Creates an instance of type <see cref="IAggregator" />.
        /// </summary>
        /// <param name="resultSetFieldNames">
        /// A set of field names, obtained from the result set itself, upfront.
        /// </param>
        /// <param name="requestedQueryName">
        /// The name of the originally requested query.
        /// </param>
        /// <param name="aggregateQuery">
        /// An instance of <see cref="AggregateQuery" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IAggregator" />.
        /// </returns>
        IAggregator Create(
            IEnumerable<string> resultSetFieldNames,
            string requestedQueryName,
            AggregateQuery aggregateQuery);
    }
}