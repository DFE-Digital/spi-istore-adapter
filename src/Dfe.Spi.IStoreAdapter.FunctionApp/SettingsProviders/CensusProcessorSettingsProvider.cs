namespace Dfe.Spi.IStoreAdapter.FunctionApp.SettingsProviders
{
    using System;
    using Dfe.Spi.IStoreAdapter.Application.Definitions.SettingsProvider;

    /// <summary>
    /// Implements <see cref="ICensusProcessorSettingsProvider" />.
    /// </summary>
    public class CensusProcessorSettingsProvider : ICensusProcessorSettingsProvider
    {
        /// <inheritdoc />
        public string AggregationFieldsEnumerationName
        {
            get
            {
                string toReturn = Environment.GetEnvironmentVariable(
                    "AggregationFieldsEnumerationName");

                return toReturn;
            }
        }
    }
}