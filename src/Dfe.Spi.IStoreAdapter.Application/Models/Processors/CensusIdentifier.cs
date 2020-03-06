namespace Dfe.Spi.IStoreAdapter.Application.Models.Processors
{
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;

    /// <summary>
    /// Represents an identifier for an individual census.
    /// </summary>
    public class CensusIdentifier : ModelsBase
    {
        /// <summary>
        /// Gets or sets the <see cref="DatasetQueryFile" /> id.
        /// </summary>
        public string DatasetQueryFileId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parameter name, used to query the data.
        /// </summary>
        public string ParameterName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parameter value, used to query the data.
        /// </summary>
        public string ParameterValue
        {
            get;
            set;
        }
    }
}