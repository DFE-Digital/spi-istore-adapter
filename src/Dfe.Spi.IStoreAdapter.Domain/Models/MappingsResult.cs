namespace Dfe.Spi.IStoreAdapter.Domain.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Type/structure used by <see cref="GetEnumerationMappingsResponse" />.
    /// </summary>
    public class MappingsResult : ModelsBase
    {
        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Usage",
            "CA2227",
            Justification = "This is a DTO.")]
        public Dictionary<string, string[]> Mappings
        {
            get;
            set;
        }
    }
}
