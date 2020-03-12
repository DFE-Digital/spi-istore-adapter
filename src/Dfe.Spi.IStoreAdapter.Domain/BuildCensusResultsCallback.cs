namespace Dfe.Spi.IStoreAdapter.Domain
{
    using System.Collections.Generic;
    using System.Data.Common;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Builds a <see cref="Census" /> instance with the supplied
    /// <see cref="DbDataReader" /> instance.
    /// </summary>
    /// <param name="aggregateQuery">
    /// The originally requested <see cref="AggregateQuery" />s.
    /// </param>
    /// <param name="dbDataReader">
    /// An instance of <see cref="DbDataReader" />.
    /// </param>
    /// <returns>
    /// An instance of <see cref="Census" />.
    /// </returns>
    public delegate Census BuildCensusResultsCallback(
        Dictionary<string, AggregateQuery> aggregateQuery,
        DbDataReader dbDataReader);
}