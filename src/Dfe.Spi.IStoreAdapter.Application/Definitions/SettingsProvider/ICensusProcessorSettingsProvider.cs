namespace Dfe.Spi.IStoreAdapter.Application.Definitions.SettingsProvider
{
    /// <summary>
    /// Describes the operations of the <see cref="ICensusProcessor" />
    /// settings provider.
    /// </summary>
    public interface ICensusProcessorSettingsProvider
    {
        /// <summary>
        /// Gets the adapter name used in pulling back the census aggregation
        /// fields from the Translation API, as a mapping.
        /// </summary>
        string AggregationFieldsAdapterName
        {
            get;
        }

        /// <summary>
        /// Gets the enumeration name used in pulling back the census
        /// aggregation fields from the Translation API, as a mapping.
        /// </summary>
        string AggregationFieldsEnumerationName
        {
            get;
        }
    }
}