namespace Dfe.Spi.IStoreAdapter.Domain
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Thrown when a non-successful status code is returned by the Translation
    /// API.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class TranslationApiAdapterException : Exception
    {
        private new const string Message =
            "A non-successful status code ({0}) was thrown by the " +
            "Translation API. Inspect the {1} property for more information.";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="TranslationApiAdapterException" /> class.
        /// </summary>
        /// <param name="httpErrorBody">
        /// An instance of <see cref="Common.Models.HttpErrorBody" />.
        /// </param>
        public TranslationApiAdapterException(HttpErrorBody httpErrorBody)
            : base(BuildExceptionMessage(httpErrorBody))
        {
            this.HttpErrorBody = httpErrorBody;
        }

        /// <summary>
        /// Gets the <see cref="Common.Models.HttpErrorBody" /> instance,
        /// returned from the Translation API.
        /// </summary>
        public HttpErrorBody HttpErrorBody
        {
            get;
            private set;
        }

        private static string BuildExceptionMessage(
            HttpErrorBody httpErrorBody)
        {
            string toReturn = null;

            string statusCode = httpErrorBody == null
                ?
                "Status Code Unavailable!"
                :
                httpErrorBody.StatusCode.ToString();

            toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                statusCode,
                nameof(HttpErrorBody));

            return toReturn;
        }
    }
}