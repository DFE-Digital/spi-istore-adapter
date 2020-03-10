namespace Dfe.Spi.IStoreAdapter.Application.Models
{
    using System.Collections.Generic;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Represents a model in which to store aggregation fields.
    /// </summary>
    public class AggregationFieldsCache : ModelsBase
    {
        /// <summary>
        /// Gets or sets a set of aggregation fields, as <see cref="string" />
        /// values.
        /// </summary>
        public IEnumerable<string> AggregationFields
        {
            get;
            set;
        }
    }
}