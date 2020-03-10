﻿namespace Dfe.Spi.IStoreAdapter.Infrastructure.SqlServer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
    using Dfe.Spi.IStoreAdapter.Domain.Models.DatasetQueryFiles;
    using Dfe.Spi.Models.Entities;

    /// <summary>
    /// Implements <see cref="ICensusAdapter" />.
    /// </summary>
    public class CensusAdapter : ICensusAdapter
    {
        private readonly ILoggerWrapper loggerWrapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="CensusAdapter" />
        /// class.
        /// </summary>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public CensusAdapter(ILoggerWrapper loggerWrapper)
        {
            this.loggerWrapper = loggerWrapper;
        }

        /// <inheritdoc />
        public async Task<Census> GetCensusAsync(
            IEnumerable<string> aggregationFields,
            DatasetQueryFile datasetQueryFile,
            Dictionary<string, AggregateQuery> aggregateQueries,
            string parameterName,
            string parameterValue,
            BuildCensusResultsCallback buildCensusResultsCallback,
            CancellationToken cancellationToken)
        {
            Census toReturn = null;

            if (datasetQueryFile == null)
            {
                throw new ArgumentNullException(nameof(datasetQueryFile));
            }

            if (buildCensusResultsCallback == null)
            {
                throw new ArgumentNullException(
                    nameof(buildCensusResultsCallback));
            }

            QueryConfiguration queryConfiguration =
                datasetQueryFile.QueryConfiguration;

            string connectionString = this.GetConnectionString(
                queryConfiguration);

            string query = datasetQueryFile.Query;

            Census census = await this.ExecuteQueryAsync(
                connectionString,
                query,
                parameterName,
                parameterValue,
                buildCensusResultsCallback,
                cancellationToken)
                .ConfigureAwait(false);

            toReturn = new Census()
            {
                // Nothing, for now.
            };

            return toReturn;
        }

        private async Task<Census> ExecuteQueryAsync(
            string connectionString,
            string query,
            string parameterName,
            string parameterValue,
            BuildCensusResultsCallback buildCensusResultsCallback,
            CancellationToken cancellationToken)
        {
            Census toReturn = null;

            Stopwatch stopwatch = new Stopwatch();

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                this.loggerWrapper.Debug(
                    $"Opening {nameof(SqlConnection)}...");

                await sqlConnection.OpenAsync(cancellationToken)
                        .ConfigureAwait(false);

                this.loggerWrapper.Info(
                    $"Connection opened with success " +
                    $"({nameof(sqlConnection.ClientConnectionId)}: " +
                    $"{sqlConnection.ClientConnectionId}).");

                DbDataReader dbDataReader = null;
                using (SqlCommand sqlCommand = this.GetSqlCommand(sqlConnection, query, parameterName, parameterValue))
                {
                    stopwatch.Start();

                    this.loggerWrapper.Debug(
                        $"Executing query \"{query}\" with parameter name " +
                        $"\"{parameterName}\", value \"{parameterValue}\"...");

                    dbDataReader =
                        await sqlCommand.ExecuteReaderAsync(cancellationToken)
                        .ConfigureAwait(false);

                    stopwatch.Stop();

                    TimeSpan elapsed = stopwatch.Elapsed;

                    this.loggerWrapper.Info(
                        $"Query \"{query}\" with parameter name " +
                        $"\"{parameterName}\", value \"{parameterValue}\" " +
                        $"executed in {elapsed}.");

                    this.loggerWrapper.Debug(
                        $"Building results from {nameof(SqlDataReader)}...");

                    toReturn = buildCensusResultsCallback(dbDataReader);

                    this.loggerWrapper.Info(
                        $"{nameof(Census)} constructed: {toReturn}. " +
                        $"Returning.");
                }
            }

            return toReturn;
        }

        [SuppressMessage(
            "Microsoft.Security",
            "CA2100",
            Justification = "Does not contain any user input.")]
        private SqlCommand GetSqlCommand(
            SqlConnection sqlConnection,
            string query,
            string parameterName,
            string parameterValue)
        {
            SqlCommand toReturn = null;

            CommandType commandType = CommandType.Text;

            this.loggerWrapper.Debug(
                $"Setting up {nameof(SqlCommand)} with query \"{query}\" " +
                $"and {nameof(CommandType)} \"{commandType}\"...");

            toReturn = new SqlCommand(query, sqlConnection)
            {
                CommandType = commandType,
            };

            SqlParameter sqlParameter = new SqlParameter(
                parameterName,
                parameterValue);

            this.loggerWrapper.Debug(
                $"Setting {nameof(SqlParameter)}: {sqlParameter} on " +
                $"{nameof(SqlCommand)}...");

            toReturn.Parameters.Add(sqlParameter);

            return toReturn;
        }

        private string GetConnectionString(QueryConfiguration queryConfiguration)
        {
            string toReturn = null;

            string connectionStringEnvironmentVariable =
                queryConfiguration.ConnectionStringEnvironmentVariable;

            this.loggerWrapper.Debug(
                $"Reading environment variable " +
                $"\"{connectionStringEnvironmentVariable}\"...");

            string serverConnectionString = Environment.GetEnvironmentVariable(
                connectionStringEnvironmentVariable);

            this.loggerWrapper.Info(
                $"{nameof(serverConnectionString)} = " +
                $"\"{new string('*', serverConnectionString.Length)}\"");

            DbConnectionStringBuilder dbConnectionStringBuilder =
                new DbConnectionStringBuilder();

            dbConnectionStringBuilder.ConnectionString =
                serverConnectionString;

            string databaseName = queryConfiguration.DatabaseName;

            this.loggerWrapper.Debug(
                $"Adding in {nameof(databaseName)} \"{databaseName}\"...");

            dbConnectionStringBuilder.Add("Initial Catalog", databaseName);

            toReturn = dbConnectionStringBuilder.ConnectionString;

            this.loggerWrapper.Debug(
                $"Connection string built: " +
                $"\"{new string('*', toReturn.Length)}\".");

            return toReturn;
        }
    }
}