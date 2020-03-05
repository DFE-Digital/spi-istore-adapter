namespace Dfe.Spi.IStoreAdapter.Domain
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;

    /// <summary>
    /// Thrown when a file, required to build up a
    /// <see cref="DatasetQueryFile" />, is missing.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class IncompleteDatasetQueryFileException
        : InvalidOperationException
    {
        private new const string Message =
            "Whilst the requested dataset query file directory exists " +
            "within storage, we were unable to locate the file \"{0}\", " +
            "which is required.";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="IncompleteDatasetQueryFileException" /> class.
        /// </summary>
        /// <param name="missingFile">
        /// The name of the missing file.
        /// </param>
        public IncompleteDatasetQueryFileException(string missingFile)
            : base(BuildExceptionMessage(missingFile))
        {
            // Nothing.
        }

        private static string BuildExceptionMessage(string missingFile)
        {
            string toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                missingFile);

            return toReturn;
        }
    }
}