namespace Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders
{
    using System.Collections.Generic;

    /// <summary>
    /// Describes the operations of the <see cref="ICensusAdapter" /> settings
    /// provider.
    /// </summary>
    public interface ICensusAdapterSettingsProvider
    {
        /// <summary>
        /// Gets the supported parameter names, that need to be specified in
        /// advance.
        /// </summary>
        IEnumerable<string> SupportedSearchParameterNames
        {
            get;
        }
    }
}