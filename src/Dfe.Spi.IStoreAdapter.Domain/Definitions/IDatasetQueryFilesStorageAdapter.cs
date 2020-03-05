namespace Dfe.Spi.IStoreAdapter.Domain.Definitions
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;

    /// <summary>
    /// Describes the operations of the <see cref="DatasetQueryFile" /> storage
    /// adapter.
    /// </summary>
    public interface IDatasetQueryFilesStorageAdapter
    {
        /// <summary>
        /// Gets an individual <see cref="DatasetQueryFile" />.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the specified <paramref name="datasetQueryFileId" />
        /// does not exist.
        /// </exception>
        /// <param name="datasetQueryFileId">
        /// The id of the <see cref="DatasetQueryFile" /> to return.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of <see cref="DatasetQueryFile" />.
        /// </returns>
        Task<DatasetQueryFile> GetDatabaseQueryFileAsync(
            string datasetQueryFileId,
            CancellationToken cancellationToken);
    }
}