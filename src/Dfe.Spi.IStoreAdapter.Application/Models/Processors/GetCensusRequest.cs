namespace Dfe.Spi.IStoreAdapter.Application.Models.Processors
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Models;

    /// <summary>
    /// Request object for
    /// <see cref="ICensusProcessor.GetCensusAsync(GetCensusRequest, CancellationToken)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetCensusRequest : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets the aggregate queries.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Usage",
            "CA2227",
            Justification = "This is a DTO.")]
        public Dictionary<string, AggregateQuery> AggregateQueries
        {
            get;
            set;
        }
    }
}