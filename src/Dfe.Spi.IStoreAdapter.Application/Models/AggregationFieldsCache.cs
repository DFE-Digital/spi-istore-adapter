namespace Dfe.Spi.IStoreAdapter.Application.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.IStoreAdapter.Domain.Models;

    /// <summary>
    /// Acts as a holding/caching object for
    /// <see cref="AggregationFieldsAndTypes" />.
    /// </summary>
    public class AggregationFieldsCache : ModelsBase
    {
        /// <summary>
        /// Gets or sets the available aggregation fields and their respective
        /// types.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Usage",
            "CA2227",
            Justification = "This is a DTO.")]
        public Dictionary<string, Type> AggregationFieldsAndTypes
        {
            get;
            set;
        }
    }
}