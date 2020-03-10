namespace Dfe.Spi.IStoreAdapter.FunctionApp.SettingsProviders
{
    using System;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders;

    /// <summary>
    /// Implements <see cref="ITranslationApiAdapterSettingsProvider" />.
    /// </summary>
    public class TranslationApiAdapterSettingsProvider
        : ITranslationApiAdapterSettingsProvider
    {
        /// <inheritdoc />
        public Uri TranslationApiBaseUri
        {
            get
            {
                Uri toReturn = null;

                string toReturnStr = Environment.GetEnvironmentVariable(
                    "TranslationApiBaseUri");

                toReturn = new Uri(toReturnStr, UriKind.Absolute);

                return toReturn;
            }
        }

        /// <inheritdoc />
        public string TranslationApiSubscriptionKey
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    "TranslationApiSubscriptionKey");

                return toReturn;
            }
        }
    }
}