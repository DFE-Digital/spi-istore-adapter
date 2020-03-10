namespace Dfe.Spi.IStoreAdapter.Application.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Thrown when a field is specified in a <see cref="DataFilter" /> that
    /// isn't supported.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class UnsupportedAggregateColumnRequestException : Exception
    {
        private new const string Message =
            "The following fields were specified in one or many {0}s: {1}. " +
            "The current supported list of fields is: {2}.";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="UnsupportedAggregateColumnRequestException" /> class.
        /// </summary>
        /// <param name="unsupported">
        /// A list of (unsupproted) requested fields.
        /// </param>
        /// <param name="supported">
        /// A list of the currently supported fields.
        /// </param>
        public UnsupportedAggregateColumnRequestException(
            IEnumerable<string> unsupported,
            IEnumerable<string> supported)
            : base(BuildExceptionMessage(unsupported, supported))
        {
            // Nothing.
        }

        private static string BuildExceptionMessage(
            IEnumerable<string> unsupported,
            IEnumerable<string> supported)
        {
            string toReturn = null;

            string unsupportedList = string.Join(
                ", ",
                unsupported);

            string supportedList = string.Join(
                ", ",
                supported);

            toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                nameof(DataFilter),
                unsupportedList,
                supportedList);

            return toReturn;
        }
    }
}