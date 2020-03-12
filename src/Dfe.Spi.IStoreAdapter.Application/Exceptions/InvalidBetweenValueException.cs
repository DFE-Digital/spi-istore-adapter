namespace Dfe.Spi.IStoreAdapter.Application.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Thrown when an incorrectly formatted
    /// <see cref="DataOperator.Between"/> value is supplied.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class InvalidBetweenValueException : Exception
    {
        private new const string Message =
            "Between values need to contain 2 valid {0}s, seperated by the " +
            "keyword \"to\". For example, \"2018-06-29\" to \"2018-07-01\".";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="InvalidBetweenValueException" /> class.
        /// </summary>
        public InvalidBetweenValueException()
            : base(BuildExceptionMessage())
        {
            // Nothing.
        }

        private static string BuildExceptionMessage()
        {
            string toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                nameof(DateTime));

            return toReturn;
        }
    }
}