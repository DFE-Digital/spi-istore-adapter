namespace Dfe.Spi.IStoreAdapter.Application.Definitions.Factories
{
    using Dfe.Spi.IStoreAdapter.Domain.Models;

    /// <summary>
    /// Describes the operations of the <see cref="IAggregator" /> factory.
    /// </summary>
    public interface IAggregatorFactory
    {
        /// <summary>
        /// Creates an instance of type <see cref="IAggregator" />.
        /// </summary>
        /// <param name="aggregateQuery">
        /// An instance of <see cref="AggregateQuery" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IAggregator" />.
        /// </returns>
        IAggregator Create(AggregateQuery aggregateQuery);
    }
}