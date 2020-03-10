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
        /// Gets enumeration values.
        /// </summary>
        /// <exception cref="TranslationApiAdapterException">
        /// Thrown when a non-successful status code is returned by the
        /// Translation API.
        /// </exception>
        /// <param name="enumerationName">
        /// The name of the enumeration to return.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="GetEnumerationValuesResponse" />.
        /// </returns>
        Task<GetEnumerationValuesResponse> GetEnumerationValuesAsync(
            string enumerationName,
            CancellationToken cancellationToken);
    }
}