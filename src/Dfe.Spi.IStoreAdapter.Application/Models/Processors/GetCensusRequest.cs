namespace Dfe.Spi.IStoreAdapter.Application.Models.Processors
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;

    /// <summary>
    /// Request object for
    /// <see cref="ICensusProcessor.GetCensusAsync(GetCensusRequest, CancellationToken)" />.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GetCensusRequest : RequestResponseBase
    {
        // Nothing for now.
    }
}