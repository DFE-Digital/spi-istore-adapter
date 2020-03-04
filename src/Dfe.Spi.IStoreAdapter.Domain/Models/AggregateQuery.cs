namespace Dfe.Spi.IStoreAdapter.Domain.Models
{
    using System.Collections.Generic;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Represents an individual aggregate query.
    /// </summary>
    public class AggregateQuery : ModelsBase
    {
        /// <summary>
        /// Gets or sets a set of <see cref="DataFilter" /> instances.
        /// </summary>
        public IEnumerable<DataFilter> DataFilters
        {
            get;
            set;
        }
    }
}