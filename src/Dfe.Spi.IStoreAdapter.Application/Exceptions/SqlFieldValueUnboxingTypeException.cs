namespace Dfe.Spi.IStoreAdapter.Application.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Thrown when a SQL field's value is not what was expected, whilst
    /// attempting to unbox to a particular type.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class SqlFieldValueUnboxingTypeException : InvalidCastException
    {
        private new const string Message =
            "Attempted to unbox value to type {0}, but the actual datatype " +
            "coming back from the SQL results is {1}. This could indicate " +
            "that a dataset query file is implementing pulling back a " +
            "different type than what is described in the Translation API " +
            "IStore mappings.";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="SqlFieldValueUnboxingTypeException" /> class.
        /// </summary>
        /// <param name="requiredType">
        /// The required datatype.
        /// </param>
        /// <param name="actualType">
        /// The actual datatype.
        /// </param>
        public SqlFieldValueUnboxingTypeException(
            Type requiredType,
            Type actualType)
            : base(BuildExceptionMessage(requiredType, actualType))
        {
            // Nothing.
        }

        private static string BuildExceptionMessage(
            Type requiredType,
            Type actualType)
        {
            string toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                requiredType.Name,
                actualType.Name);

            return toReturn;
        }
    }
}