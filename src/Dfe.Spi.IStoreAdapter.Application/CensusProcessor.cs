namespace Dfe.Spi.IStoreAdapter.Application
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;

    /// <summary>
    /// Implements <see cref="ICensusProcessor" />.
    /// </summary>
    public class CensusProcessor : ICensusProcessor
    {
        /// <inheritdoc />
        public Task<GetCensusResponse> GetCensusAsync(
            GetCensusRequest getCensusRequest,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}