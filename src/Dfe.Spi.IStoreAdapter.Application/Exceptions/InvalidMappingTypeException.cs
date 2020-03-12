namespace Dfe.Spi.IStoreAdapter.Application.Exceptions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Thrown when the adapter attempts to extract a <see cref="Type" /> given
    /// to it by the aggregate field mappings stored in the Translation API,
    /// but no such type could be instantiated.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class InvalidMappingTypeException : Exception
    {
        private new const string Message =
            "The requested type name, \"{0}\", could not be instantiated as " +
            "an actual {1}. The Translation API mappings for the IStore " +
            "Adapter must relate to .NET types sitting in the {2} namespace " +
            "(for example, {3}).";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="InvalidMappingTypeException" /> class.
        /// </summary>
        /// <param name="requestedType">
        /// A type name that could not be instantiated.
        /// </param>
        public InvalidMappingTypeException(string requestedType)
            : base(BuildExceptionMessage(requestedType))
        {
            // Nothing.
        }

        private static string BuildExceptionMessage(string requestedType)
        {
            string toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                requestedType,
                nameof(Type),
                nameof(System),
                nameof(Int32));

            return toReturn;
        }
    }
}