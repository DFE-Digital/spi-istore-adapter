namespace Dfe.Spi.IStoreAdapter.FunctionApp.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Http.Server.Definitions;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;
    using Dfe.Spi.IStoreAdapter.Domain.Models;
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

        private string id;

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

            this.id = id;

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

            try
            {
                // TODO: Wire up to processor response.
                GetCensusResponse getCensusResponse =
                    await this.censusProcessor.GetCensusAsync(
                        getCensusRequest,
                        cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (NotImplementedException)
            {
                // Nothing, just want to return.
            }

            // TODO: Temporary stubbing - to be removed.
            Aggregation[] aggregations = null;
            if (getCensusRequest != null)
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
                Name = $"Requested Census: Id {this.id}",
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

            return toReturn;
        }
    }
}