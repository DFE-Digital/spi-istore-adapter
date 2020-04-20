namespace Dfe.Spi.IStoreAdapter.Application.Models.Processors
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Response object for
    /// <see cref="ICensusProcessor.GetCensusAsync(GetCensusesRequest, CancellationToken)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetCensusesResponse : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets an array of
        /// <see cref="Spi.Models.Entities.Census" />.
        /// </summary>
        public Census[] Censuses
        {
            get;
            set;
        }
    }
}