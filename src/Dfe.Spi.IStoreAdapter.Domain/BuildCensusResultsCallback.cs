namespace Dfe.Spi.IStoreAdapter.Domain
{
    using System.Data.Common;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Builds a <see cref="Census" /> instance with the supplied
    /// <see cref="DbDataReader" /> instance.
    /// </summary>
    /// <param name="dbDataReader">
    /// An instance of <see cref="DbDataReader" />.
    /// </param>
    /// <returns>
    /// An instance of <see cref="Census" />.
    /// </returns>
    public delegate Census BuildCensusResultsCallback(
        DbDataReader dbDataReader);
}