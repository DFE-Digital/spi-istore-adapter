namespace Dfe.Spi.IStoreAdapter.Domain.Models
{
    using System.Threading;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;

    /// <summary>
    /// Response type for the
    /// <see cref="ITranslationApiAdapter.GetEnumerationMappingsAsync(string, string, CancellationToken)" />
    /// method.
    /// </summary>
    public class GetEnumerationMappingsResponse : ModelsBase
    {
        /// <summary>
        /// Gets or sets an instance of
        /// <see cref="Models.MappingsResult" />.
        /// </summary>
        public MappingsResult MappingsResult
        {
            get;
            set;
        }
    }
}