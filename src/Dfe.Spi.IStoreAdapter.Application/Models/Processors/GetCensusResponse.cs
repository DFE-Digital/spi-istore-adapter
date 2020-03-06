namespace Dfe.Spi.IStoreAdapter.Application.Models.Processors
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Response object for
    /// <see cref="ICensusProcessor.GetCensusAsync(GetCensusRequest, CancellationToken)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetCensusResponse : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="Spi.Models.Entities.Census" />.
        /// </summary>
        public Census Census
        {
            get;
            set;
        }
    }
}