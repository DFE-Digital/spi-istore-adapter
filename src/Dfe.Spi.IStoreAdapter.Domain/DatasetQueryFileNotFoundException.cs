namespace Dfe.Spi.IStoreAdapter.Domain
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;

    /// <summary>
    /// Thrown when a requested <see cref="DatasetQueryFile" /> was requested,
    /// but not found.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Design",
        "CA1032",
        Justification = "Not a public library.")]
    public class DatasetQueryFileNotFoundException : FileNotFoundException
    {
        private new const string Message =
            "Could not find a blob storage directory for the dataset query " +
            "file id \"{0}\".";

        /// <summary>
        /// Initialises a new instance of the
        /// <see cref="DatasetQueryFileNotFoundException" /> class.
        /// </summary>
        /// <param name="datasetQueryFileId">
        /// A <see cref="DatasetQueryFile" /> identifier.
        /// </param>
        public DatasetQueryFileNotFoundException(string datasetQueryFileId)
            : base(BuildExceptionMessage(datasetQueryFileId))
        {
            // Nothing.
        }

        private static string BuildExceptionMessage(string datasetQueryFileId)
        {
            string toReturn = string.Format(
                CultureInfo.InvariantCulture,
                Message,
                datasetQueryFileId);

            return toReturn;
        }
    }
}