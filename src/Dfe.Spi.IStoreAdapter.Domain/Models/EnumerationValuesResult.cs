namespace Dfe.Spi.IStoreAdapter.Domain.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Type/structure used by <see cref="GetEnumerationValuesResponse" />.
    /// </summary>
    public class EnumerationValuesResult : ModelsBase
    {
        /// <summary>
        /// Gets or sets the enumerations, as a set of <see cref="string" />
        /// values.
        /// </summary>
        public IEnumerable<string> EnumerationValues
        {
            get;
            set;
        }
    }
}
