namespace Dfe.Spi.IStoreAdapter.Application.Definitions
{
    using System.Data.Common;
    using Dfe.Spi.Models;

    /// <summary>
    /// Describes the operations of the aggregator.
    /// </summary>
    public interface IAggregator
    {
        /// <summary>
        /// Returns an instance of <see cref="Aggregation" /> - the result of
        /// the <see cref="IAggregator" /> at this point.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="Aggregation" />.
        /// </returns>
        Aggregation GetResult();

        /// <summary>
        /// Processes a single row, based on the instance's configuration.
        /// </summary>
        /// <param name="dbDataReader">
        /// An instance of type <see cref="DbDataReader" />.
        /// </param>
        void ProcessRow(DbDataReader dbDataReader);
    }
}