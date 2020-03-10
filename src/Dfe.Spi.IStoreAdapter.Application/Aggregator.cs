namespace Dfe.Spi.IStoreAdapter.Application
{
    using System.Data.Common;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
    using Dfe.Spi.Models;

    /// <summary>
    /// Implements <see cref="IAggregator" />.
    /// </summary>
    public class Aggregator : IAggregator
    {
        private readonly AggregateQuery aggregateQuery;

        /// <summary>
        /// Initialises a new instance of the <see cref="Aggregator" /> class.
        /// </summary>
        /// <param name="aggregateQuery">
        /// An instance of <see cref="AggregateQuery" />.
        /// </param>
        public Aggregator(AggregateQuery aggregateQuery)
        {
            this.aggregateQuery = aggregateQuery;
        }

        /// <inheritdoc />
        public Aggregation Result
        {
            get
            {
                // TODO ...
                throw new System.NotImplementedException();
            }
        }

        /// <inheritdoc />
        public void ProcessRow(DbDataReader dbDataReader)
        {
            // TODO ...
            throw new System.NotImplementedException();
        }
    }
}