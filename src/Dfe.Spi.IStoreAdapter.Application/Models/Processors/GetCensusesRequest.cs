namespace Dfe.Spi.IStoreAdapter.Application.Models.Processors
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Models;

    /// <summary>
    /// Request object for
    /// <see cref="ICensusProcessor.GetCensusAsync(GetCensusesRequest, CancellationToken)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetCensusesRequest : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="Processors.CensusIdentifier" />.
        /// </summary>
        public CensusIdentifier[] CensusIdentifiers
        {
            get;
            set;
        }

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