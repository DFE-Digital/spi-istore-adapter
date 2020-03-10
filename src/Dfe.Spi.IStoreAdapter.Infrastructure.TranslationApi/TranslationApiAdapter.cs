namespace Dfe.Spi.IStoreAdapter.Infrastructure.TranslationApi
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Context.Definitions;
    using Dfe.Spi.Common.Context.Models;
    using Dfe.Spi.Common.Http;
    using Dfe.Spi.Common.Http.Client;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.Common.Models;
    using Dfe.Spi.IStoreAdapter.Domain;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders;
    using Dfe.Spi.IStoreAdapter.Domain.Exceptions;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Implements <see cref="ITranslationApiAdapter" />.
    /// </summary>
    public class TranslationApiAdapter : ITranslationApiAdapter
    {
        private const string GetEnumerationValuesPath = "./enumerations/{0}";

        private readonly ILoggerWrapper loggerWrapper;
        private readonly IRestClient restClient;
        private readonly ISpiExecutionContextManager spiExecutionContextManager;

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="TranslationApiAdapter" /> class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        /// <param name="spiExecutionContextManager">
        /// An instance of type <see cref="ISpiExecutionContextManager" />.
        /// </param>
        /// <param name="translationApiAdapterSettingsProvider">
        /// An instance of type
        /// <see cref="ITranslationApiAdapterSettingsProvider" />.
        /// </param>
        public TranslationApiAdapter(
            ILoggerWrapper loggerWrapper,
            ISpiExecutionContextManager spiExecutionContextManager,
            ITranslationApiAdapterSettingsProvider translationApiAdapterSettingsProvider)
        {
            if (translationApiAdapterSettingsProvider == null)
            {
                throw new ArgumentNullException(
                    nameof(translationApiAdapterSettingsProvider));
            }

            this.loggerWrapper = loggerWrapper;
            this.spiExecutionContextManager = spiExecutionContextManager;

            Uri translationApiBaseUri =
                translationApiAdapterSettingsProvider.TranslationApiBaseUri;

            this.restClient = new RestClient(translationApiBaseUri);

            string translationApiSubscriptionKey =
                translationApiAdapterSettingsProvider.TranslationApiSubscriptionKey;

            this.restClient.AddDefaultHeader(
                CommonHeaderNames.EapimSubscriptionKeyHeaderName,
                translationApiSubscriptionKey);
        }

        /// <inheritdoc />
        public async Task<GetEnumerationValuesResponse> GetEnumerationValuesAsync(
            string enumerationName,
            CancellationToken cancellationToken)
        {
            GetEnumerationValuesResponse toReturn = null;

            string getEnumerationValuesPath = string.Format(
                CultureInfo.InvariantCulture,
                GetEnumerationValuesPath,
                enumerationName);

            Uri getEnumerationValuesUri = new Uri(
                getEnumerationValuesPath,
                UriKind.Relative);

            RestRequest restRequest = new RestRequest(
                getEnumerationValuesUri,
                Method.GET);

            SpiExecutionContext spiExecutionContext =
                this.spiExecutionContextManager.SpiExecutionContext;

            restRequest.AppendContext(spiExecutionContext);

            IRestResponse<GetEnumerationValuesResponse> restResponse =
                await this.restClient.ExecuteTaskAsync<GetEnumerationValuesResponse>(
                    restRequest,
                    cancellationToken)
                    .ConfigureAwait(false);

            if (!restResponse.IsSuccessful)
            {
                this.ParseErrorInformationThrowException(restResponse);
            }

            toReturn = restResponse.Data;

            return toReturn;
        }

        private void ParseErrorInformationThrowException(
            IRestResponse<GetEnumerationValuesResponse> restResponse)
        {
            this.loggerWrapper.Warning(
                $"A non-successful status code was returned " +
                $"({restResponse.StatusCode}).");

            // Deserialise the data as the standard error model.
            string content = restResponse.Content;

            this.loggerWrapper.Debug($"content = \"{content}\"");
            this.loggerWrapper.Debug(
                $"Attempting to de-serialise the body (\"{content}\") " +
                $"as a {nameof(HttpErrorBody)} instance...");

            HttpErrorBody httpErrorBody = null;
            try
            {
                httpErrorBody =
                    JsonConvert.DeserializeObject<HttpErrorBody>(content);

                this.loggerWrapper.Warning(
                    $"{nameof(httpErrorBody)} = {httpErrorBody}");
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.loggerWrapper.Warning(
                    $"Could not de-serialise error body to an instance " +
                    $"of {nameof(HttpErrorBody)}.",
                    jsonReaderException);
            }

            throw new TranslationApiAdapterException(httpErrorBody);
        }
    }
}