namespace Dfe.Spi.IStoreAdapter.Domain.Definitions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.IStoreAdapter.Domain.Exceptions;
    using Dfe.Spi.IStoreAdapter.Domain.Models;

    /// <summary>
    /// Describes the operations of the Translation API adapter.
    /// </summary>
    public interface ITranslationApiAdapter
    {
        /// <summary>
        /// Gets enumeration mappings.
        /// </summary>
        /// <exception cref="TranslationApiAdapterException">
        /// Thrown when a non-successful status code is returned by the
        /// Translation API.
        /// </exception>
        /// <param name="enumerationName">
        /// The name of the enumeration in which to return mappings for.
        /// </param>
        /// <param name="adapterName">
        /// The name of the adapter in which to return mappings for.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="GetEnumerationMappingsResponse" />.
        /// </returns>
        Task<GetEnumerationMappingsResponse> GetEnumerationMappingsAsync(
            string enumerationName,
            string adapterName,
            CancellationToken cancellationToken);
    }
}