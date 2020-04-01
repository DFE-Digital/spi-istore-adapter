namespace Dfe.Spi.IStoreAdapter.Domain.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Thrown when an unsupported search parameter is specified.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class UnsupportedSearchParameterException
        : InvalidOperationException
    {
        private new const string Message =
            "The specified search parameter, \"{0}\" is not supported. " +
            "Supported search parameters are: {1}.";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="UnsupportedSearchParameterException" /> class.
        /// </summary>
        /// <param name="unsupported">
        /// The unsupported search parameter.
        /// </param>
        /// <param name="supported">
        /// An array of supported search parameters.
        /// </param>
        public UnsupportedSearchParameterException(
            string unsupported,
            IEnumerable<string> supported)
            : base(BuildExceptionMessage(unsupported, supported))
        {
            // Nothing.
        }

        private static string BuildExceptionMessage(
            string unsupported,
            IEnumerable<string> supported)
        {
            string toReturn = null;

            string supportedCsl = string.Join(", ", supported);

            toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                unsupported,
                supportedCsl);

            return toReturn;
        }
    }
}