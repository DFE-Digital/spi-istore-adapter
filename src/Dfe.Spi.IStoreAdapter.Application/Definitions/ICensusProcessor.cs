namespace Dfe.Spi.IStoreAdapter.Application.Definitions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;

    /// <summary>
    /// Describes the operations of the census processor.
    /// </summary>
    public interface ICensusProcessor
    {
        /// <summary>
        /// The get census processor entry method.
        /// </summary>
        /// <param name="getCensusRequest">
        /// An instance of <see cref="GetCensusRequest" />.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="GetCensusResponse" />.
        /// </returns>
        Task<GetCensusResponse> GetCensusAsync(
            GetCensusRequest getCensusRequest,
            CancellationToken cancellationToken);
    }
}