namespace Dfe.Spi.IStoreAdapter.Application
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Globalization;
    using System.Linq;
    using Dfe.Spi.Common.Extensions;
    using Dfe.Spi.Common.Models;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Exceptions;
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
            DateTime toReturn = value.ToDateTime();

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

            IComparable actualUnboxedFieldValueComparable =
                actualUnboxedFieldValue as IComparable;

            // For reference, IComparable.CompareTo will return:
            // -1: Invoking item is smaller than the passed in item.
            //  0: Invoking item is the same as the passed in item.
            //  1: Invoking item is greater than the passed in item.
            int compareToResult;

            // We have the field value, now.
            // Take the operation...
            DataOperator dataOperator = dataFilter.Operator;

            string value = dataFilter.Value;
            object unboxedValue = null;

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

                case DataOperator.In:
                    // Split the string up, a comma seperated list.
                    string[] parts = value.Split(
                        new char[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries);

                    bool[] atLeastOneEquals = parts
                        .Select(x => this.EvaluateDataFilter(
                            dbDataReader,
                            new DataFilter()
                            {
                                Field = field,
                                Operator = DataOperator.Equals,
                                Value = x,
                            }))
                            .ToArray();

                    toReturn = atLeastOneEquals.Any(x => x);

                    break;

                case DataOperator.Equals:
                    // We'll not unbox the SQL result field. We'll just take it
                    // as an object, and perform an equalities operator over
                    // it.
                    // The string, however, we'll need to unbox, at least
                    // into an object.
                    unboxedValue = this.UnboxFilterValue(field, value);

                    toReturn = actualUnboxedFieldValue.Equals(unboxedValue);

                    break;

                case DataOperator.GreaterThan:
                    unboxedValue = this.UnboxFilterValue(field, value);

                    compareToResult = actualUnboxedFieldValueComparable
                        .CompareTo(unboxedValue);

                    toReturn = compareToResult > 0;

                    break;

                case DataOperator.GreaterThanOrEqualTo:
                    unboxedValue = this.UnboxFilterValue(field, value);

                    compareToResult = actualUnboxedFieldValueComparable
                        .CompareTo(unboxedValue);

                    toReturn = compareToResult >= 0;

                    break;

                case DataOperator.IsNotNull:
                    toReturn = actualUnboxedFieldValue != DBNull.Value;
                    break;

                case DataOperator.IsNull:
                    toReturn = actualUnboxedFieldValue == DBNull.Value;
                    break;

                case DataOperator.LessThan:
                    unboxedValue = this.UnboxFilterValue(field, value);

                    compareToResult = actualUnboxedFieldValueComparable
                        .CompareTo(unboxedValue);

                    toReturn = compareToResult < 0;
                    break;

                case DataOperator.LessThanOrEqualTo:
                    unboxedValue = this.UnboxFilterValue(field, value);

                    compareToResult = actualUnboxedFieldValueComparable
                        .CompareTo(unboxedValue);

                    toReturn = compareToResult <= 0;
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
                throw new InvalidBetweenValueException();
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

            try
            {
                string fieldTypeName = fieldType.Name;

                switch (fieldTypeName)
                {
                    case nameof(Int32):
                        toReturn = int.Parse(
                            value,
                            CultureInfo.InvariantCulture);
                        break;

                    case nameof(DateTime):
                        toReturn = UnboxDateTime(value);
                        break;

                    case nameof(String):
                        // Easiest conversion ever.
                        toReturn = value;
                        break;

                    case nameof(Boolean):
                        toReturn = bool.Parse(value);
                        break;

                    default:
                        throw new NotImplementedException(
                            $"The unboxing of type \"{fieldTypeName}\" " +
                            $"needs to be implemented!");
                }
            }
            catch (FormatException formatException)
            {
                throw new DataFilterValueUnboxingTypeException(
                    field,
                    fieldType,
                    value,
                    formatException);
            }

            return toReturn;
        }

        private TUnboxedValue UnboxSqlResultField<TUnboxedValue>(
            object unboxed)
        {
            TUnboxedValue toReturn = default(TUnboxedValue);

            if (!(unboxed is DBNull))
            {
                if (unboxed is TUnboxedValue)
                {
                    toReturn = (TUnboxedValue)unboxed;
                }
                else
                {
                    throw new SqlFieldValueUnboxingTypeException(
                        typeof(TUnboxedValue),
                        unboxed.GetType());
                }
            }

            return toReturn;
        }
    }
}