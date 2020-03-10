namespace Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders
{
    using System;

    /// <summary>
    /// Describes the operations of the <see cref="ITranslationApiAdapter" />
    /// settings provider.
    /// </summary>
    public interface ITranslationApiAdapterSettingsProvider
    {
        /// <summary>
        /// Gets the base URI of the Translation API.
        /// </summary>
        Uri TranslationApiBaseUri
        {
            get;
        }

        /// <summary>
        /// Gets the subscription key used to talk to the Translation API.
        /// </summary>
        string TranslationApiSubscriptionKey
        {
            get;
        }
    }
}