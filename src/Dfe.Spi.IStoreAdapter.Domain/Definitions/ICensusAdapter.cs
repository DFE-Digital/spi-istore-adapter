namespace Dfe.Spi.IStoreAdapter.Domain.Definitions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Describes the operations of the <see cref="Census" /> adapter.
    /// </summary>
    public interface ICensusAdapter
    {
        /// <summary>
        /// Gets a <see cref="Census" />.
        /// </summary>
        /// <param name="aggregationFields">
        /// A set of available aggregation fields.
        /// </param>
        /// <param name="datasetQueryFile">
        /// An instance of <see cref="DatasetQueryFile" />.
        /// </param>
        /// <param name="aggregateQueries">
        /// The aggregate queries in which to run.
        /// </param>
        /// <param name="parameterName">
        /// The parameter name, used to query the data.
        /// </param>
        /// <param name="parameterValue">
        /// The parameter value, used to query the data.
        /// </param>
        /// /// <param name="buildCensusResultsCallback">
        /// An instance of <see cref="BuildCensusResultsCallback" />.
        /// This method is invoked after a successful pull of data, and
        /// provides the logic to create the returned <see cref="Census" />.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="Census" />.
        /// </returns>
        Task<Census> GetCensusAsync(
            IEnumerable<string> aggregationFields,
            DatasetQueryFile datasetQueryFile,
            Dictionary<string, AggregateQuery> aggregateQueries,
            string parameterName,
            string parameterValue,
            BuildCensusResultsCallback buildCensusResultsCallback,
            CancellationToken cancellationToken);
    }
}