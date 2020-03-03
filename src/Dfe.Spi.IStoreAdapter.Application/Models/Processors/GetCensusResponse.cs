namespace Dfe.Spi.IStoreAdapter.Application.Models.Processors
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;

    /// <summary>
    /// Response object for
    /// <see cref="ICensusProcessor.GetCensusAsync(GetCensusRequest, CancellationToken)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetCensusResponse : RequestResponseBase
    {
        // Nothing for now...
    }
}