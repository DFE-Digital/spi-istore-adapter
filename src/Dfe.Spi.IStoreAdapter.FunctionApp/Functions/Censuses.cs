namespace Dfe.Spi.IStoreAdapter.FunctionApp.Functions
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Http.Server.Definitions;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;
    using Dfe.Spi.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Newtonsoft.Json;

    /// <summary>
    /// Entry class for the <c>censuses</c> function.
    /// </summary>
    public class Censuses : FunctionsBase<GetCensusRequest>
    {
        private readonly ICensusProcessor censusProcessor;
        private readonly IHttpErrorBodyResultProvider httpErrorBodyResultProvider;
        private readonly ILoggerWrapper loggerWrapper;

        private CensusIdentifier censusIdentifier;

        /// <summary>
        /// Initialises a new instance of the <see cref="Censuses" /> class.
        /// </summary>
        /// <param name="censusProcessor">
        /// An instance of type <see cref="ICensusProcessor" />.
        /// </param>
        /// <param name="httpErrorBodyResultProvider">
        /// An instance of type <see cref="IHttpErrorBodyResultProvider" />.
        /// </param>
        /// <param name="httpSpiExecutionContextManager">
        /// An instance of type <see cref="IHttpSpiExecutionContextManager" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public Censuses(
            ICensusProcessor censusProcessor,
            IHttpErrorBodyResultProvider httpErrorBodyResultProvider,
            IHttpSpiExecutionContextManager httpSpiExecutionContextManager,
            ILoggerWrapper loggerWrapper)
            : base(httpSpiExecutionContextManager, loggerWrapper)
        {
            this.censusProcessor = censusProcessor;
            this.httpErrorBodyResultProvider = httpErrorBodyResultProvider;
            this.loggerWrapper = loggerWrapper;
        }

        /// <summary>
        /// Entry method for the <c>censuses</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpContext" />.
        /// </param>
        /// <param name="id">
        /// The id of the requested census.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IActionResult" />.
        /// </returns>
        [FunctionName("censuses")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "GET", "POST", Route = "censuses/{id}")]
            HttpRequest httpRequest,
            string id,
            CancellationToken cancellationToken)
        {
            IActionResult toReturn = null;

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Parse and validate the id to a CensusIdentifier.
            this.censusIdentifier = this.ParseIdentifier(id);

            if (this.censusIdentifier != null)
            {
                switch (httpRequest.Method)
                {
                    case "POST":
                        toReturn = await this.ValidateAndRunAsync(
                            httpRequest,
                            cancellationToken)
                            .ConfigureAwait(false);
                        break;

                    case "GET":
                        toReturn = await this.ProcessWellFormedRequestAsync(
                            null,
                            cancellationToken)
                            .ConfigureAwait(false);
                        break;
                }
            }
            else
            {
                toReturn =
                    this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                        HttpStatusCode.BadRequest,
                        3);
            }

            return toReturn;
        }

        /// <inheritdoc />
        protected override HttpErrorBodyResult GetMalformedErrorResponse()
        {
            HttpErrorBodyResult toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    1);

            return toReturn;
        }

        /// <inheritdoc />
        protected override HttpErrorBodyResult GetSchemaValidationResponse(
            string message)
        {
            HttpErrorBodyResult toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    2,
                    message);

            return toReturn;
        }

        /// <inheritdoc />
        protected async override Task<IActionResult> ProcessWellFormedRequestAsync(
            GetCensusRequest getCensusRequest,
            CancellationToken cancellationToken)
        {
            IActionResult toReturn = null;

            if (getCensusRequest == null)
            {
                getCensusRequest = new GetCensusRequest();
            }

            getCensusRequest.CensusIdentifier = this.censusIdentifier;

            try
            {
                // TODO: Wire up result to processor response, when the below
                //       is removed.
                GetCensusResponse getCensusResponse =
                    await this.censusProcessor.GetCensusAsync(
                        getCensusRequest,
                        cancellationToken)
                        .ConfigureAwait(false);

                // TODO: Temporary stubbing - to be removed.
                Aggregation[] aggregations = null;
                if (getCensusRequest.AggregateQueries != null)
                {
                    aggregations = getCensusRequest
                        .AggregateQueries
                        .Select(x => new Aggregation()
                        {
                            Name = x.Key,
                            Value = x.Key.GetHashCode(StringComparison.InvariantCulture),
                        })
                        .ToArray();
                }

                Models.Entities.Census census = new Models.Entities.Census()
                {
                    Name = $"Requested Census: Id {this.censusIdentifier}",
                    _Aggregations = aggregations,
                };

                JsonSerializerSettings jsonSerializerSettings =
                    JsonConvert.DefaultSettings();

                if (jsonSerializerSettings == null)
                {
                    toReturn = new JsonResult(census);
                }
                else
                {
                    toReturn = new JsonResult(census, jsonSerializerSettings);
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                string datasetQueryFileId =
                    this.censusIdentifier.DatasetQueryFileId;

                this.loggerWrapper.Info(
                    $"The requested {nameof(datasetQueryFileId)}, " +
                    $"\"{datasetQueryFileId}\", could not be found.",
                    fileNotFoundException);

                toReturn =
                    this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                        HttpStatusCode.NotFound,
                        4,
                        datasetQueryFileId);
            }

            return toReturn;
        }

        private CensusIdentifier ParseIdentifier(string censusIdentifierStr)
        {
            CensusIdentifier toReturn = null;

            this.loggerWrapper.Debug(
                $"Splitting \"{censusIdentifierStr}\" up by its hyphens...");

            string[] identifierParts = censusIdentifierStr.Split(
                '-',
                StringSplitOptions.RemoveEmptyEntries);

            this.loggerWrapper.Debug(
                $"{nameof(identifierParts)}.{nameof(identifierParts.Length)} " +
                $"= {identifierParts.Length}");

            if (identifierParts.Length == 3)
            {
                toReturn = new CensusIdentifier()
                {
                    DatasetQueryFileId = identifierParts[0],
                    ParameterName = identifierParts[1],
                    ParameterValue = identifierParts[2],
                };
            }

            return toReturn;
        }
    }
}