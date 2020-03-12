namespace Dfe.Spi.IStoreAdapter.Application.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Dfe.Spi.Common.Models;

    /// <summary>
    /// Thrown when an <see cref="DataFilter.Value" /> cannot be unboxed from
    /// it's <see cref="string" /> form, according to the datatype expected for
    /// the <see cref="DataFilter.Field" />.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class DataFilterValueUnboxingTypeException : FormatException
    {
        private new const string Message =
            "Field \"{0}\" is of type \"{1}\", and the value provided in " +
            "the {2} (\"{3}\") could not be parsed accordingly.";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="DataFilterValueUnboxingTypeException" /> class.
        /// </summary>
        /// <param name="fieldName">
        /// The name of the field.
        /// </param>
        /// <param name="fieldType">
        /// The type of the field.
        /// </param>
        /// <param name="fieldValueUnboxedAsString">
        /// The problem unboxed field value, as a <see cref="string" />.
        /// </param>
        /// <param name="formatException">
        /// The original <see cref="FormatException" />.
        /// </param>
        public DataFilterValueUnboxingTypeException(
            string fieldName,
            Type fieldType,
            string fieldValueUnboxedAsString,
            FormatException formatException)
            : base(
                  BuildExceptionMessage(
                    fieldName,
                    fieldType,
                    fieldValueUnboxedAsString),
                  formatException)
        {
            // Nothing.
        }

        private static string BuildExceptionMessage(
            string fieldName,
            Type fieldType,
            string fieldValueUnboxedAsString)
        {
            string toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                fieldName,
                fieldType.Name,
                nameof(DataFilter),
                fieldValueUnboxedAsString);

            return toReturn;
        }
    }
}