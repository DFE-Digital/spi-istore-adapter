namespace Dfe.Spi.IStoreAdapter.FunctionApp.SettingsProviders
{
    using System;
    using Dfe.Spi.IStoreAdapter.FunctionApp.Definitions.SettingsProviders;

    /// <summary>
    /// Implements <see cref="IStartupSettingsProvider" />.
    /// </summary>
    public class StartupSettingsProvider : IStartupSettingsProvider
    {
        /// <inheritdoc />
        public string KeyVaultInstanceName
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    "KeyVaultInstanceName");

                return toReturn;
            }
        }
    }
}