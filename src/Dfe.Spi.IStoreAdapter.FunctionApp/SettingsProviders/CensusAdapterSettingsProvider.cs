namespace Dfe.Spi.IStoreAdapter.FunctionApp.SettingsProviders
{
    using System;
    using System.Collections.Generic;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders;

    /// <summary>
    /// Implements <see cref="ICensusAdapterSettingsProvider" />.
    /// </summary>
    public class CensusAdapterSettingsProvider : ICensusAdapterSettingsProvider
    {
        /// <inheritdoc />
        public IEnumerable<string> SupportedSearchParameterNames
        {
            get
            {
                string[] toReturn = null;

                string supportedParameterNamesCsl =
                    Environment.GetEnvironmentVariable(
                        "SupportedSearchParameterNames");

                toReturn = supportedParameterNamesCsl.Split(",");

                return toReturn;
            }
        }
    }
}