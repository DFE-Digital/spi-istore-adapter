namespace Dfe.Spi.IStoreAdapter.Domain.Models
{
    using System.Threading;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;

    /// <summary>
    /// Response type for the
    /// <see cref="ITranslationApiAdapter.GetEnumerationValuesAsync(string, CancellationToken)" />
    /// method.
    /// </summary>
    public class GetEnumerationValuesResponse : ModelsBase
    {
        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="Models.EnumerationValuesResult" />.
        /// </summary>
        public EnumerationValuesResult EnumerationValuesResult
        {
            get;
            set;
        }
    }
}