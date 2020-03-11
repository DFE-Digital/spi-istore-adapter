namespace Dfe.Spi.IStoreAdapter.Application
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Globalization;
    using System.Linq;
    using Dfe.Spi.Common.Models;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Models;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
    using Dfe.Spi.Models;

    /// <summary>
    /// Implements <see cref="IAggregator" />.
    /// </summary>
    public class Aggregator : IAggregator
    {
        private readonly IEnumerable<string> resultSetFieldNames;

        private readonly AggregationFieldsCache aggregationFieldsCache;
        private readonly string requestedQueryName;
        private readonly AggregateQuery aggregateQuery;

        private decimal aggregateTotal;

        /// <summary>
        /// Initialises a new instance of the <see cref="Aggregator" /> class.
        /// </summary>
        /// <param name="resultSetFieldNames">
        /// A set of field names, obtained from the result set itself, upfront.
        /// </param>
        /// <param name="aggregationFieldsCache">
        /// An instance of <see cref="AggregationFieldsCache" />.
        /// </param>
        /// <param name="requestedQueryName">
        /// The name of the originally requested query.
        /// </param>
        /// <param name="aggregateQuery">
        /// An instance of <see cref="AggregateQuery" />.
        /// </param>
        public Aggregator(
            IEnumerable<string> resultSetFieldNames,
            AggregationFieldsCache aggregationFieldsCache,
            string requestedQueryName,
            AggregateQuery aggregateQuery)
        {
            this.resultSetFieldNames = resultSetFieldNames;

            this.aggregationFieldsCache = aggregationFieldsCache;
            this.requestedQueryName = requestedQueryName;
            this.aggregateQuery = aggregateQuery;

            this.aggregateTotal = 0;
        }

        /// <inheritdoc />
        public Aggregation GetResult()
        {
            Aggregation toReturn = new Aggregation()
            {
                Name = this.requestedQueryName,
                Value = this.aggregateTotal,
            };

            return toReturn;
        }

        /// <inheritdoc />
        public void ProcessRow(DbDataReader dbDataReader)
        {
            // 1) Check if row matches conditions set out in the
            //    AggregateQuery.
            IEnumerable<DataFilter> dataFilters =
                this.aggregateQuery.DataFilters;

            bool[] queryResults = dataFilters
                .Select(x => this.EvaluateDataFilter(dbDataReader, x))
                .ToArray();

            bool matchesFilter = queryResults.All(x => x);

            if (matchesFilter)
            {
                AggregateType aggregateType =
                    this.aggregateQuery.AggregateType;

                // 2) Apply the requested aggregate.
                switch (aggregateType)
                {
                    case AggregateType.Count:
                        this.aggregateTotal++;
                        break;

                    default:
                        throw new NotImplementedException(
                            $"Unsupported {nameof(AggregateType)}, " +
                            $"\"{aggregateType}\"! This needs implementing!");
                }
            }
        }

        private static DateTime UnboxDateTime(string value)
        {
            DateTime toReturn = DateTime.Parse(
                value,
                CultureInfo.InvariantCulture);

            // TODO: Replace with Simon's incoming DateTime parsing function.
            return toReturn;
        }

        private bool EvaluateDataFilter(
            DbDataReader dbDataReader,
            DataFilter dataFilter)
        {
            bool toReturn = false;

            string field = dataFilter.Field;

            // The requested fields should be validated by this point - so we
            // don't need to worry about it for now.
            // We also know the name of the columns in the result set.
            object actualUnboxedFieldValue = null;
            if (this.resultSetFieldNames.Contains(field))
            {
                actualUnboxedFieldValue = dbDataReader[field];
            }

            // We have the field value, now.
            // Take the operation...
            DataOperator dataOperator = dataFilter.Operator;

            string value = dataFilter.Value;

            switch (dataOperator)
            {
                case DataOperator.Between:
                    // The SQL field value needs to be DateTime for a between.
                    DateTime? actualFieldValue =
                        this.UnboxSqlResultField<DateTime?>(
                            actualUnboxedFieldValue);

                    // The input DateTimes also need to be DateTimes.
                    Tuple<DateTime, DateTime> betweenDates =
                        this.UnboxBetweenDataFilterValue(value);

                    toReturn =
                        (actualFieldValue < betweenDates.Item2)
                            &&
                        (actualFieldValue > betweenDates.Item1);

                    break;

                case DataOperator.Equals:
                    // We'll not unbox the SQL result field. We'll just take it
                    // as an object, and perform an equalities operator over
                    // it.
                    // The string, however, we'll need to unbox, at least
                    // into an object.
                    object unboxedValue = this.UnboxFilterValue(field, value);

                    toReturn = actualUnboxedFieldValue.Equals(unboxedValue);

                    break;

                default:
                    throw new NotImplementedException(
                        $"Unsupported {nameof(DataOperator)}, " +
                        $"\"{dataOperator}\"! This needs implementing!");
            }

            return toReturn;
        }

        private Tuple<DateTime, DateTime> UnboxBetweenDataFilterValue(
            string value)
        {
            Tuple<DateTime, DateTime> toReturn = null;

            string[] datePartsStr = value.Split(
                new string[] { " to " },
                StringSplitOptions.RemoveEmptyEntries);

            if (datePartsStr.Length != 2)
            {
                // TODO: Review exception type.
                throw new FormatException(
                    $"Between values need to contain 2 valid " +
                    $"{nameof(DateTime)}s, seperated by the keyword " +
                    $"\"to\". For example, \"2018-06-29T00:00:00Z\" to " +
                    $"\"2018-07-01T00:00:00Z\".");
            }

            DateTime from = UnboxDateTime(datePartsStr.First());
            DateTime to = UnboxDateTime(datePartsStr.Last());

            toReturn = new Tuple<DateTime, DateTime>(from, to);

            return toReturn;
        }

        private object UnboxFilterValue(string field, string value)
        {
            object toReturn = null;

            Dictionary<string, Type> aggregationFieldsAndTypes =
                this.aggregationFieldsCache.AggregationFieldsAndTypes;

            Type fieldType = aggregationFieldsAndTypes[field];

            string fieldTypeName = fieldType.Name;

            switch (fieldTypeName)
            {
                case nameof(Int32):
                    toReturn = int.Parse(value, CultureInfo.InvariantCulture);
                    break;

                case nameof(DateTime):
                    toReturn = UnboxDateTime(value);
                    break;

                case nameof(String):
                    // Easiest conversion ever.
                    toReturn = value;
                    break;

                default:
                    throw new NotImplementedException(
                        $"The unboxing of type \"{fieldTypeName}\" needs " +
                        $"to be implemented!");
            }

            return toReturn;
        }

        private TUnboxedValue UnboxSqlResultField<TUnboxedValue>(
            object unboxed)
        {
            TUnboxedValue toReturn = default(TUnboxedValue);

            if (unboxed is TUnboxedValue)
            {
                toReturn = (TUnboxedValue)unboxed;
            }
            else
            {
                // TODO: Throw exception.
            }

            return toReturn;
        }
    }
}