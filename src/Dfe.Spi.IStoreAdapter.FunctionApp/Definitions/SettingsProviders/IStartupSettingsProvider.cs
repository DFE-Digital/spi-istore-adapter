namespace Dfe.Spi.IStoreAdapter.FunctionApp.Definitions.SettingsProviders
{
    /// <summary>
    /// Describes the operations of the <see cref="Startup" /> settings
    /// provider.
    /// </summary>
    public interface IStartupSettingsProvider
    {
        /// <summary>
        /// Gets the name of the KeyVault instance to use.
        /// </summary>
        string KeyVaultInstanceName
        {
            get;
        }
    }
}